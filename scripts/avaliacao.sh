#!/usr/bin/env bash
# Operação do ambiente de avaliação remota: docker-compose.yml + docker-compose.avaliacao.yml.
# Passo a passo completo em docs/AVALIACAO_REMOTA.md.
set -euo pipefail
cd "$(dirname "$0")/.."

# Registro local (não versionado) de cada subida e reset, para cruzar com os relatos dos avaliadores.
LOG=avaliacao-registro.log
# Convites: uma linha "<rótulo> <código>" por avaliador (não versionado).
INVITES=avaliacao/convites.txt
INVITES_NGINX=avaliacao/nginx/convites.conf

env_get() { if [ -f .env ]; then grep -E "^$1=" .env | tail -1 | cut -d= -f2- || true; fi; }

# Docker que roda a avaliação: "default" é o Docker Engine do sistema, que sobe no boot sem login (o
# Docker Desktop só inicia depois que alguém entra na sessão). Vazio: o contexto atual da CLI.
ctx=$(env_get DOCKER_CONTEXT_AVALIACAO); [ -n "$ctx" ] && export DOCKER_CONTEXT=$ctx

# TUNNEL=funnel no .env: Tailscale Funnel (URL estável). Vazio ou "quick": quick tunnel da Cloudflare
# (URL temporária, para teste ou plano B).
if [ "$(env_get TUNNEL)" = funnel ]; then
  MODE=funnel; TUNNEL=tailscale; OTHER=tunnel-quick
else
  MODE=quick; TUNNEL=tunnel-quick; OTHER=tailscale
fi
APP_PORT=$(env_get APP_PORT); APP_PORT=${APP_PORT:-8088}
# ACESSO_RESTRITO=true no .env liga o portão de convite; o padrão é acesso aberto a qualquer pessoa.
[ "$(env_get ACESSO_RESTRITO)" = true ] && RESTRICTED=1 || RESTRICTED=0

compose() { docker compose -f docker-compose.yml -f docker-compose.avaliacao.yml --profile "$MODE" "$@"; }
compose_all() { docker compose -f docker-compose.yml -f docker-compose.avaliacao.yml --profile quick --profile funnel "$@"; }

# Os comandos de dados rodam num segundo processo dentro do container da API já em execução,
# sem reiniciar nenhum serviço (o túnel continua no ar).
demo() { compose exec -T backend dotnet StudyENEM.API.dll demo "$1"; }

record() { printf '%s  %s\n' "$(date '+%Y-%m-%d %H:%M:%S %z')" "$*" >> "$LOG"; }

public_url() {
  if [ "$MODE" = funnel ]; then
    compose exec -T tailscale tailscale status --json 2>/dev/null \
      | python3 -c 'import json,sys; n=json.load(sys.stdin)["Self"]["DNSName"].rstrip("."); print("https://"+n if n else "")' 2>/dev/null || true
  else
    compose logs "$TUNNEL" 2>/dev/null | grep -oE 'https://[a-z0-9-]+\.trycloudflare\.com' | tail -1 || true
  fi
}

# Cria os códigos que faltam (um por conta demoNN), gera o mapa do nginx e o recarrega se estiver no ar.
invites_build() {
  local accounts; accounts=$(env_get DEMO_ACCOUNTS); accounts=${accounts:-5}
  touch "$INVITES"; chmod 600 "$INVITES"
  # O código "organizador" é seu: usado pelo status e pelos seus testes, sem gastar o de um avaliador.
  grep -qE "^organizador " "$INVITES" || echo "organizador $(openssl rand -hex 12)" >> "$INVITES"
  for i in $(seq -f '%02g' 1 "$accounts"); do
    grep -qE "^avaliador$i " "$INVITES" || echo "avaliador$i $(openssl rand -hex 12)" >> "$INVITES"
  done
  local codes; codes=$(grep -vE '^\s*(#|$)' "$INVITES" | awk '{print $2}' | grep -E '^[A-Za-z0-9]+$' || true)
  # Escrito com ">" (mesmo arquivo): o container enxerga a mudança pelo bind mount.
  {
    echo "# Gerado por scripts/avaliacao.sh a partir de $INVITES. Não edite."
    echo 'map $uri $convite_link_valido {'
    echo '    default "";'
    for c in $codes; do echo "    ~^/convite/$c/?\$ $c;"; done
    echo '}'
    echo 'map $cookie_studyenem_convite $convite_cookie_valido {'
    echo '    default 0;'
    for c in $codes; do echo "    $c 1;"; done
    echo '}'
    if [ "$RESTRICTED" = 1 ]; then
      echo '# Acesso restrito: sem convite válido, só ficam livres o link de convite e o /health do monitor.'
      echo 'map "$convite_cookie_valido:$uri" $convite_bloqueado {'
      echo '    default 1;'
      echo '    ~^1: 0;'
      echo '    ~^0:/convite/ 0;'
      echo '    "0:/health" 0;'
      echo '}'
    else
      echo '# Acesso aberto (ACESSO_RESTRITO diferente de true): nada é bloqueado; os links de convite continuam válidos.'
      echo 'map $uri $convite_bloqueado { default 0; }'
    fi
  } > "$INVITES_NGINX"
  if [ -n "$(compose ps -q frontend 2>/dev/null)" ]; then
    compose exec -T frontend nginx -t -q && compose exec -T frontend nginx -s reload
  fi
}

first_invite() { awk '$1=="organizador"{print $2; exit}' "$INVITES" 2>/dev/null; }

ok() { printf '  \033[32mOK\033[0m    %s\n' "$*"; }
fail() { printf '  \033[31mFALHA\033[0m %s\n' "$*"; FAILED=1; }
warn() { printf '  \033[33mAVISO\033[0m %s\n' "$*"; }

status() {
  FAILED=0
  echo "Serviços (túnel: $MODE)"
  for s in postgres backend frontend "$TUNNEL"; do
    state=$(compose ps --format '{{.State}}' "$s" 2>/dev/null || true)
    [ "$state" = running ] && ok "$s" || fail "$s: ${state:-não está rodando}"
  done

  local base="http://localhost:$APP_PORT" code body resp invite; invite=$(first_invite)
  echo "Aplicação local ($base)"
  code=$(curl -s -o /dev/null -w '%{http_code}' -m 10 -H "Cookie: studyenem_convite=$invite" "$base/" || true)
  [ "$code" = 200 ] && ok "frontend com convite: $code" || fail "frontend com convite: $code"
  code=$(curl -s -o /dev/null -w '%{http_code}' -m 10 "$base/" || true)
  if [ "$RESTRICTED" = 1 ]; then
    [ "$code" = 403 ] && ok "sem convite: $code (acesso restrito ativo)" || fail "sem convite: $code (esperado 403: portão de convite inativo)"
  else
    [ "$code" = 200 ] && ok "sem convite: $code (acesso aberto)" || fail "sem convite: $code (esperado 200 com acesso aberto)"
  fi
  body=$(curl -s -m 10 "$base/health" || true)
  [ "$body" = Healthy ] && ok "API e banco: $body" || fail "API e banco (/health): ${body:-sem resposta}"

  url=$(public_url)
  echo "Acesso público (${url:-URL desconhecida})"
  if [ -z "$url" ]; then
    fail "sem URL pública: o túnel ainda não subiu ou não foi autenticado (scripts/avaliacao.sh logs $TUNNEL)"
  else
    # 200 (acesso aberto) ou 403 "Acesso restrito" (portão ligado): o túnel e o nginx respondem.
    code=$(curl -s -o /dev/null -w '%{http_code}' -m 20 "$url/" || true)
    expected=$([ "$RESTRICTED" = 1 ] && echo 403 || echo 200)
    [ "$code" = "$expected" ] && ok "$url responde $code" || fail "$url responde ${code:-sem resposta} (esperado $expected)"
    # /health passa pelo túnel, nginx, API e banco.
    resp=$(curl -s -m 20 -w $'\n%{http_code}' "$url/health" || true)
    code=${resp##*$'\n'}; body=${resp%$'\n'*}
    [ "$body" = Healthy ] && ok "$url/health: Healthy" || fail "$url/health: ${body:-sem resposta} ($code)"
  fi

  [ "$FAILED" = 0 ] && echo "Tudo no ar." || { echo "Há falhas: veja 'Se algo cair' em docs/AVALIACAO_REMOTA.md."; return 1; }
}

preflight() {
  FAILED=0
  echo "Máquina"
  # Verifica o Docker que de fato roda a stack: o Docker Desktop só inicia depois do login na sessão.
  if [ "$(docker context show 2>/dev/null)" = desktop-linux ]; then
    [ "$(systemctl --user is-enabled docker-desktop 2>/dev/null)" = enabled ] && ok "Docker Desktop inicia sozinho no login" \
      || fail "Docker Desktop não inicia sozinho: systemctl --user enable docker-desktop"
    warn "Docker Desktop só sobe depois que alguém entra na sessão: após um reinício, a URL fica fora do ar até o login (runbook 1.1)"
  else
    [ "$(systemctl is-enabled docker 2>/dev/null)" = enabled ] && ok "Docker Engine sobe com o sistema, sem login" || fail "Docker não sobe com o sistema: sudo systemctl enable docker"
  fi
  if command -v gsettings >/dev/null; then
    s=$(gsettings get org.gnome.settings-daemon.plugins.power sleep-inactive-ac-type 2>/dev/null || true)
    [ "$s" = "'nothing'" ] && ok "sem suspensão automática na tomada" \
      || fail "a máquina suspende sozinha na tomada ($s): gsettings set org.gnome.settings-daemon.plugins.power sleep-inactive-ac-type 'nothing'"
  fi
  lid=$(systemd-analyze cat-config systemd/logind.conf 2>/dev/null | grep -E '^HandleLidSwitchExternalPower=' | tail -1 | cut -d= -f2 || true)
  [ "$lid" = ignore ] && ok "fechar a tampa na tomada não suspende" \
    || warn "fechar a tampa pode suspender (HandleLidSwitchExternalPower=${lid:-padrão}): mantenha a tampa aberta ou veja o runbook"

  echo "Configuração"
  for v in POSTGRES_PASSWORD JWT_KEY DEMO_PASSWORD; do
    [ -n "$(env_get $v)" ] && ok "$v definida" || fail "$v vazia"
  done
  [ "$MODE" = funnel ] && ok "Tailscale Funnel (URL estável)" \
    || fail "quick tunnel: URL temporária, não envie aos avaliadores. Configure o Tailscale Funnel (runbook 1.3) e TUNNEL=funnel no .env"
  if [ "$RESTRICTED" = 1 ]; then
    n=$(grep -cvE '^\s*(#|$)' "$INVITES" 2>/dev/null || true)
    [ "${n:-0}" -gt 0 ] && ok "acesso restrito: $n convites em $INVITES" || fail "nenhum convite: scripts/avaliacao.sh convites"
  else
    warn "acesso aberto: qualquer pessoa com a URL entra e pode se cadastrar (ACESSO_RESTRITO=true no .env para exigir convite)"
  fi
  echo
  local machine_failed=$FAILED
  status || true
  if [ "$FAILED" != 0 ] || [ "$machine_failed" != 0 ]; then
    echo "Preflight com falhas: corrija os itens marcados antes de abrir a coleta."
    return 1
  fi
}

invites_list() {
  local url; url=$(public_url)
  [ -n "$url" ] || url="<URL pública>"
  printf '%-14s %-24s %s\n' "AVALIADOR" "CONTA DA PLATAFORMA" "LINK DE CONVITE"
  grep -vE '^\s*(#|$)' "$INVITES" | while read -r label code _; do
    account=-; [[ $label =~ ^avaliador([0-9]+)$ ]] && account="demo${BASH_REMATCH[1]}@studyenem.com"
    printf '%-14s %-24s %s\n' "$label" "$account" "$url/convite/$code"
  done
}

case "${1:-}" in
  up)
    invites_build
    compose up -d --build
    compose_all rm -sf "$OTHER" >/dev/null 2>&1 || true
    record "up (túnel $MODE)"
    echo "Aguardando a URL pública..."
    for _ in $(seq 1 30); do url=$(public_url); [ -n "$url" ] && break; sleep 2; done
    echo "URL pública: ${url:-ainda não disponível, rode: scripts/avaliacao.sh url}"
    if [ "$MODE" = funnel ] && [ -z "$url" ] && [ -z "$(env_get TS_AUTHKEY)" ]; then
      echo "O Tailscale não está autenticado: defina TS_AUTHKEY no .env (docs/AVALIACAO_REMOTA.md, seção 1.3) e rode up de novo."
    fi
    [ "$MODE" = quick ] && echo "(quick tunnel: URL temporária, não envie aos avaliadores)" || true ;;
  down)      compose_all down; record "down" ;;   # mantém os volumes (banco e login do Tailscale)
  seed)      demo seed ;;
  reset)     demo reset; record "reset" ;;
  convites)  invites_build; record "convites gerados/recarregados"; invites_list ;;
  url)       public_url ;;
  status)    status ;;
  preflight) preflight ;;
  ps)        compose ps ;;
  logs)      shift; compose logs -f --tail=100 "$@" ;;
  *)
    cat >&2 <<'EOF'
Uso: scripts/avaliacao.sh <comando>

  up         constrói e sobe tudo, inclusive o túnel (as contas demoNN são criadas na subida)
  status     verificação diária: serviços, portão de convite, aplicação e URL pública
  preflight  checklist antes da coleta: máquina, configuração e status
  convites   cria os convites que faltam, recarrega o nginx e lista os links por avaliador
  url        mostra a URL pública
  reset      volta ao estado inicial sem derrubar o túnel (registrado em avaliacao-registro.log)
  seed       cria as contas demoNN e os históricos que faltam; não altera o que já existe
  logs       acompanha os logs (opcional: serviço, ex.: logs backend)
  ps         estado dos serviços
  down       para e remove os containers, mantendo o banco e o login do Tailscale
EOF
    exit 2 ;;
esac
