# Avaliação heurística remota: operação

Runbook para publicar o StudyENEM a partir desta máquina durante a coleta (cerca de duas semanas, de 3 a 5
especialistas, de forma assíncrona), sem domínio próprio e sem custo. Todos os comandos rodam na raiz do
repositório.

```
navegador do avaliador
  → Tailscale Funnel (HTTPS em https://studyenem.<tailnet>.ts.net; conexão de saída, nenhuma porta aberta no roteador)
  → nginx (container "frontend": Angular em /, API em /api; portão de convite opcional)
  → API (container "backend")  → PostgreSQL (container "postgres", volume próprio)
```

O ambiente de avaliação é o `docker-compose.yml` com o override `docker-compose.avaliacao.yml`, operado pelo
`scripts/avaliacao.sh`. Ele é separado do ambiente de desenvolvimento: tem containers, banco e porta próprios
(`localhost:8088`) e não publica a API nem o banco. O acesso à URL pública é **aberto** por padrão; o portão de
convite é opcional (seção 1.4).

| Comando | O que faz |
|---|---|
| `scripts/avaliacao.sh up` | constrói e sobe tudo, inclusive a publicação; cria as contas demoNN e os convites que faltarem |
| `scripts/avaliacao.sh status` | verificação diária: serviços, aplicação e URL pública |
| `scripts/avaliacao.sh preflight` | checklist da máquina e da configuração, seguido do `status` |
| `scripts/avaliacao.sh convites` | aplica o modo de acesso e lista os links de convite (usados só com acesso restrito) |
| `scripts/avaliacao.sh url` | URL pública atual |
| `scripts/avaliacao.sh reset` | volta ao estado inicial **sem derrubar a publicação** |
| `scripts/avaliacao.sh logs [serviço]` | logs ao vivo (`backend`, `frontend`, `tailscale`…) |
| `scripts/avaliacao.sh down` | para tudo e mantém o banco e o login do Tailscale |

O script registra cada `up`, `reset`, `down` e geração de convites, com data e hora, em `avaliacao-registro.log`.
Esse arquivo não é versionado.

---

## 1. Preparação (uma vez, antes da coleta)

### 1.1 Máquina sempre ligada

Na avaliação assíncrona, ninguém avisa que o link caiu: o avaliador só desiste da tarefa. A máquina precisa
ficar ligada, acordada e conectada durante as duas semanas.

- **Docker no boot:** `systemctl is-enabled docker` deve responder `enabled`. Se não responder:
  `sudo systemctl enable docker`. Com isso e com `restart: unless-stopped`, os containers voltam sozinhos depois de
  um reinício.
- **Sem suspensão automática.** Por padrão, o GNOME suspende a máquina depois de 30 min ociosa, **mesmo na
  tomada**. Para desligar:
  ```bash
  gsettings set org.gnome.settings-daemon.plugins.power sleep-inactive-ac-type 'nothing'
  ```
  Para desfazer depois da coleta: `gsettings reset org.gnome.settings-daemon.plugins.power sleep-inactive-ac-type`.
- **Tampa do notebook.** Fechar a tampa suspende a máquina. Mantenha a tampa aberta ou configure o sistema para
  ignorar o fechamento na tomada. A configuração passa a valer no próximo boot:
  ```bash
  sudo mkdir -p /etc/systemd/logind.conf.d
  printf '[Login]\nHandleLidSwitchExternalPower=ignore\n' | sudo tee /etc/systemd/logind.conf.d/avaliacao.conf
  ```
- **Queda de energia.** A bateria do notebook segura a máquina, mas o roteador cai. Quando a energia volta, o
  Tailscale reconecta sozinho. Se a máquina chegar a desligar, ela **não** liga sozinha: confira a verificação
  diária (seção 5).
- **Atualizações:** evite atualizar o sistema ou reiniciar a máquina durante a coleta. Se precisar reiniciar,
  rode `scripts/avaliacao.sh status` logo depois.

`scripts/avaliacao.sh preflight` confere o Docker, a suspensão e a tampa automaticamente.

### 1.2 Variáveis do `.env`

Crie o `.env` a partir do `.env.example` (`cp .env.example .env`) e preencha:

| Variável | Valor |
|---|---|
| `POSTGRES_PASSWORD` | `openssl rand -hex 16`. Trocar depois exige recriar o banco |
| `JWT_KEY` | `openssl rand -hex 32` |
| `DEMO_ACCOUNTS` | quantidade de contas demoNN e de convites de avaliador (padrão 5) |
| `DEMO_PASSWORD` | senha única das contas demoNN, entregue aos avaliadores |
| `TUNNEL` | `funnel` (Tailscale, URL estável). `quick` só para teste ou plano B (seção 2) |
| `ACESSO_RESTRITO` | `false` (padrão, aberto a qualquer pessoa) ou `true` (exige link de convite, seção 1.4) |
| `TS_AUTHKEY` | chave de autenticação do Tailscale, usada só no primeiro `up` (seção 1.3) |

O `GOOGLE_CLIENT_ID` também vale aqui (seção 1.5). Para desligar o Google só na avaliação: `AVALIACAO_GOOGLE_CLIENT_ID=`
(vazio).

### 1.3 Tailscale Funnel (URL estável, gratuita, sem domínio)

O Funnel publica o nginx em `https://studyenem.<tailnet>.ts.net`. O certificado HTTPS é automático, e a URL se
mantém entre reinícios porque o login fica guardado num volume. O plano gratuito (Personal) basta. Os nomes de menu
são os do painel atual; procure os termos equivalentes se algum mudar.

1. **Conta e chave de autenticação:** entre em https://login.tailscale.com com Google, GitHub ou Microsoft (a
   conta é criada no primeiro acesso). Em **Settings → Keys → Generate auth key**:
   - *Reusable* desligado;
   - *Expiration* de 1 dia, porque ela só é usada uma vez;
   - *Ephemeral* desligado;
   - *Pre-approved* ligado, se a opção aparecer;
   - sem tags.

   Copie a chave (`tskey-auth-...`) para o `.env` em `TS_AUTHKEY=`. O login por link não funciona no container:
   sem chave, ele reinicia a cada minuto antes de o login terminar.
2. **Nome do tailnet (antes de enviar qualquer link):** em https://login.tailscale.com/admin/dns → *Tailnet
   DNS name*. O padrão é algo como `tail1a2b3c.ts.net`. Você pode trocá-lo **uma vez** por um nome sugerido mais
   legível. Decida antes dos convites: trocar depois muda a URL.
3. **HTTPS:** na mesma página, em *HTTPS Certificates* → **Enable HTTPS**. Mantenha o *MagicDNS* ligado.
4. **Permissão de Funnel:** em https://login.tailscale.com/admin/acls, a política precisa conceder o atributo
   `funnel`. Tailnets novos já vêm com isto; se faltar, acrescente:
   ```json
   "nodeAttrs": [
     { "target": ["autogroup:member"], "attr": ["funnel"] }
   ]
   ```
5. **Expiração do login da máquina (opcional):** o padrão é de 180 dias, mais que a coleta. Para não depender
   disso: https://login.tailscale.com/admin/machines → máquina `studyenem` → menu `…` → **Disable key
   expiry**. Faça isso depois do primeiro `up`, quando a máquina aparecer conectada.
6. No `.env`: `TUNNEL=funnel`. Depois `scripts/avaliacao.sh up`. No fim, o script mostra a URL
   `https://studyenem.<tailnet>.ts.net`. O certificado e o DNS público podem levar alguns minutos na primeira vez.

A configuração do Funnel fica versionada em `avaliacao/tailscale/serve.json`: porta 443 → `http://frontend:80`.

### 1.4 Quem pode abrir a URL: acesso aberto ou restrito

**Padrão: acesso aberto** (`ACESSO_RESTRITO=false`). Qualquer pessoa com a URL abre a aplicação, entra com uma
conta demoNN ou o Google e pode se cadastrar. Os avaliadores recebem só a URL. Para isso funcionar:
- um `robots.txt` pede aos buscadores que não indexem a URL de teste;
- contas criadas por terceiros ou bots são apagadas no próximo `reset` (seção 6).

**Opcional: acesso restrito** (`ACESSO_RESTRITO=true` no `.env`, depois `scripts/avaliacao.sh convites`, sem
reiniciar nada). O nginx passa a exigir um link de convite antes de qualquer tela:

- Cada avaliador recebe um **link próprio**, `https://studyenem.<tailnet>.ts.net/convite/<código>`. Ao abrir o
  link, o nginx grava um cookie (válido por 30 dias) e leva à tela de login, sem nenhuma tela extra.
- Sem o cookie de um convite válido, qualquer endereço da aplicação, inclusive a API e o cadastro, responde com a
  página **"Acesso restrito"**.
- O `/health` fica livre para o monitor externo (seção 5).

Os códigos ficam em `avaliacao/convites.txt` (não versionado), uma linha `<rótulo> <código>`: `avaliador01` …
`avaliadorNN` (um por conta demoNN) e `organizador` (o seu, usado pelo `status` e nos seus testes). O `up` cria os
que faltam. Para ver os links:

```bash
scripts/avaliacao.sh convites
```

- **Revogar um convite:** apague a linha em `avaliacao/convites.txt` e rode `scripts/avaliacao.sh convites`. O
  nginx recarrega sem reiniciar, e o cookie daquele código deixa de valer na hora.
- **Novo avaliador:** aumente `DEMO_ACCOUNTS` no `.env` e rode `scripts/avaliacao.sh up`, que cria a conta demoNN e
  o convite.
- **Outro navegador ou computador:** basta abrir o link de convite de novo.
- O link é pessoal: quem o receber encaminhado também entra. Peça no convite que não seja repassado.
- Os links continuam funcionando com o acesso aberto (só levam à tela de login). Então dá para enviá-los desde já
  e ligar o modo restrito depois, sem reenviar nada.

### 1.5 Login com Google

O botão "Continuar com Google" fica ligado na URL pública, com o mesmo `GOOGLE_CLIENT_ID` do desenvolvimento. Para
funcionar, a origem `https://studyenem.<tailnet>.ts.net` precisa estar cadastrada como origem JavaScript autorizada
no Google Cloud Console (veja `docs/LOGIN_GOOGLE.md`). Nunca cadastre a URL do quick tunnel, porque ela muda a cada
reinício.

- **Avaliadores:** enquanto o app OAuth estiver *Em teste*, só os usuários de teste conseguem entrar pelo Google.
  Um avaliador que clicar no botão recebe *Erro 403: access_denied* do Google. Quem entra pelo Google cai numa conta
  vazia, sem o histórico das demoNN.
- **Desligar só na avaliação:** `AVALIACAO_GOOGLE_CLIENT_ID=` (vazio) no `.env` e `scripts/avaliacao.sh up`. O
  botão some, e o desenvolvimento continua com o Google.

---

## 2. Teste rápido e plano B: quick tunnel da Cloudflare

Com `TUNNEL=quick` (ou vazio) no `.env`, o `up` publica por um quick tunnel da Cloudflare: URL aleatória
`https://<palavras>.trycloudflare.com`, sem conta. Serve para testar antes do Tailscale ou como **plano B** se o
Funnel ficar indisponível.

```bash
scripts/avaliacao.sh up        # no fim, mostra a URL
scripts/avaliacao.sh convites  # links de convite com essa URL (se usar acesso restrito)
```

A URL muda sempre que o container do túnel reinicia (reboot, queda). Com acesso restrito, os cookies de convite valem só para o
endereço em que foram gravados, **trocar de URL exige enviar os links de novo** aos avaliadores. Use como plano B
só por um intervalo curto.

---

## 3. Abrir a coleta

> **Nenhum convite sai antes da aprovação do projeto pelo CEP-IFSP.** A coleta com os especialistas é o Grupo 1
> do projeto submetido. O TCLE do formulário precisa estar com o **CAAE preenchido** antes do primeiro envio.
> Sem isso, os dados não podem ser usados no TCC.

1. `scripts/avaliacao.sh up`
2. `scripts/avaliacao.sh preflight`: tudo deve aparecer OK, inclusive "Tailscale Funnel (URL estável)".
3. `scripts/avaliacao.sh reset`, **logo antes de enviar os convites**. O reset recalcula as datas dos históricos a
   partir deste momento: o simulado mais recente fica 6 dias antes.
4. Numa janela anônima, de outra rede (o celular no 4G, por exemplo):
   - abra a URL. Com acesso aberto, aparece a tela de login; com acesso restrito, aparece "Acesso restrito", e
     você entra pelo link `organizador` (`scripts/avaliacao.sh convites`);
   - entre com `demo01`;
   - confira que MT é a área mais fraca, que Eletricidade e magnetismo é o primeiro item do plano de estudos e que
     o gráfico de evolução está preenchido.
5. Envie os convites (seção 4).

## 4. O que entregar a cada avaliador

Cada avaliador usa **uma conta demoNN só dele** (e, com acesso restrito, um link de convite só dele). As contas têm históricos idênticos, então
todos veem os mesmos dados, e um simulado feito por um não aparece para os outros.

| Avaliador | Link | Conta da plataforma | Senha | E-mail para a T4 (cadastro) |
|---|---|---|---|---|
| 1 | convite `avaliador01` | `demo01@studyenem.com` | `DEMO_PASSWORD` | `e01@studyenem.test` |
| 2 | convite `avaliador02` | `demo02@studyenem.com` | `DEMO_PASSWORD` | `e02@studyenem.test` |
| 3 | convite `avaliador03` | `demo03@studyenem.com` | `DEMO_PASSWORD` | `e03@studyenem.test` |
| 4 | convite `avaliador04` | `demo04@studyenem.com` | `DEMO_PASSWORD` | `e04@studyenem.test` |
| 5 | convite `avaliador05` | `demo05@studyenem.com` | `DEMO_PASSWORD` | `e05@studyenem.test` |

`scripts/avaliacao.sh convites` mostra os links completos já com a conta de cada avaliador.

- **O e-mail da T4 precisa ser diferente para cada avaliador.** A T4 cria uma conta nova. Se dois avaliadores
  usarem o mesmo e-mail e não houver reset entre eles, o segundo recebe "Já existe uma conta com este e-mail" e a
  tarefa se desvia do roteiro.
- O convite deve trazer:
  - a URL (com acesso aberto) ou o **link de convite** dele (com acesso restrito);
  - a conta, a senha e o e-mail da T4;
  - um pedido para não repassar o link;
  - o aviso de que o escopo é a versão web em computador.
- Anote em `avaliacao-registro.log`, ou no seu diário de campo, quando cada avaliador recebeu o convite e quando
  concluiu.

## 5. Verificação diária

Uma vez por dia, de preferência sempre no mesmo horário:

```bash
scripts/avaliacao.sh status
```

Deve terminar em **"Tudo no ar."**. Ele confere:
- os serviços;
- o acesso: sem convite, 200 (aberto) ou 403 (restrito);
- o `/health` local e o público. O `/health` só responde `Healthy` quando o Funnel, o nginx, a API e o banco estão
  no ar.

Além disso, abra o seu link `organizador` no celular, fora da rede de casa.

**Recomendado: um monitor externo**, que avisa por e-mail sem depender de você lembrar. Um serviço gratuito como o
UptimeRobot basta:
- **URL monitorada:** `https://studyenem.<tailnet>.ts.net/health`, e não a URL principal. A página principal é
  servida pelo nginx e responde mesmo com a API ou o banco fora do ar. Com acesso restrito, ela também responderia
  "Acesso restrito".
- **Tipo:** *Keyword*, procurando `Healthy`, a cada 5 minutos, com alerta por e-mail.
- Ele dispara quando a publicação cai (sem resposta) e quando a API ou o banco caem (`Unhealthy`, 503).
- Teste o alerta uma vez: `docker stop studyenem-avaliacao-postgres-1`, espere o e-mail e depois
  `docker start studyenem-avaliacao-postgres-1`.

## 6. Reset entre rodadas

```bash
scripts/avaliacao.sh reset
```

- **O reset:**
  - apaga todos os simulados;
  - apaga todas as contas criadas durante a avaliação (as da T4);
  - restaura nome e senha das contas demoNN;
  - recria os históricos.
- **Ele não reinicia nenhum container:** a URL e os convites continuam valendo, e quem estiver logado numa conta
  demoNN continua logado.
- **Rode quando:** antes de abrir a coleta, ou se uma conta demoNN precisar ser reutilizada por outro avaliador.
  Também resolve a colisão de e-mail da T4, caso não seja possível dar e-mails distintos.
- **Consequência:** as datas dos históricos são recalculadas a partir do momento do reset. Avaliadores anteriores e
  posteriores a um reset veem o mesmo perfil, mas com datas diferentes no gráfico de evolução. Isso não invalida
  nada, mas se um achado sobre o gráfico aparecer só em parte dos relatos, confira em `avaliacao-registro.log`
  quais avaliadores ficaram de cada lado do reset.
- **Não rode** com um avaliador no meio de uma sessão: o simulado dele é apagado.

Para só completar contas e históricos que faltam, sem apagar nada: `scripts/avaliacao.sh seed`.

## 7. Logs

```bash
scripts/avaliacao.sh logs            # todos os serviços
scripts/avaliacao.sh logs backend    # só a API (erros, logins, reset)
scripts/avaliacao.sh logs frontend   # nginx: acessos (com acesso restrito, inclui os barrados com 403)
scripts/avaliacao.sh logs tailscale  # publicação (use tunnel-quick no plano B)
scripts/avaliacao.sh ps              # estado e tempo no ar de cada serviço
```

## 8. Se algo cair

Comece sempre por `scripts/avaliacao.sh status`, que aponta a camada com problema.

| Sintoma | O que fazer |
|---|---|
| `/health` responde `Unhealthy` (503) | A API está no ar, mas não alcança o banco. Veja `scripts/avaliacao.sh logs postgres` e rode `scripts/avaliacao.sh up`. |
| Algum serviço "não está rodando" | `scripts/avaliacao.sh up`. Se ele cair de novo, veja `scripts/avaliacao.sh logs <serviço>`. |
| Local OK, URL pública sem resposta | O Funnel caiu. Confira a internet da máquina e veja `scripts/avaliacao.sh logs tailscale`. Para reiniciar só a publicação: `docker restart studyenem-avaliacao-tailscale-1`. |
| `status` diz "sem URL pública" ou os logs do Tailscale pedem login | O login expirou ou a máquina foi removida no painel. Gere uma chave nova (seção 1.3, passo 1), coloque em `TS_AUTHKEY`, rode `docker volume rm studyenem-avaliacao_tailscale_state` com o container parado e depois `scripts/avaliacao.sh up`. |
| Logs do Tailscale falam em Funnel não permitido | Falta o atributo `funnel` na política ou o HTTPS no tailnet (seção 1.3, passos 3 e 4). |
| Funnel fora do ar por muito tempo | Plano B (seção 2): `TUNNEL=quick` no `.env`, `scripts/avaliacao.sh up`, `scripts/avaliacao.sh convites` e reenvie os links. Volte a `TUNNEL=funnel` quando normalizar e reenvie os links originais. |
| Avaliador vê "Acesso restrito" (só com `ACESSO_RESTRITO=true`) | Abriu a URL sem o código, usou outro navegador ou o cookie expirou (30 dias). Ele deve abrir o link de convite de novo. Se continuar, confira se o código dele ainda está em `avaliacao/convites.txt`. |
| A máquina reiniciou | Nada a fazer se o Docker sobe no boot: os containers voltam sozinhos. Confirme com `status`. |
| A máquina desligou (bateria acabou) | Ligue, entre na sessão e rode `status`. O que ficou fora do ar é perdido: avise os avaliadores afetados se o intervalo foi longo. |
| Avaliador diz que "a página não carrega" e o `status` está OK | Peça o horário e um print. Confira `logs frontend` e `logs backend` nesse horário. Também pode ser a rede dele bloqueando `ts.net`. |
| Avaliador com dados estranhos na conta | Outro avaliador usou a mesma conta demoNN. Separe as contas e, se precisar, rode o `reset` (seção 6). |

## 9. Checklist pré-coleta

- [ ] **Projeto aprovado pelo CEP-IFSP e CAAE preenchido no TCLE do formulário.** Sem isso, nada abaixo importa:
      nenhum convite sai.
- [ ] `systemctl is-enabled docker` → `enabled`
- [ ] Suspensão automática na tomada desligada (`preflight` sem falha)
- [ ] Tampa aberta, ou `HandleLidSwitchExternalPower=ignore` aplicado com reboot
- [ ] Notebook na tomada, conectado por cabo se possível
- [ ] `.env` com `POSTGRES_PASSWORD`, `JWT_KEY`, `DEMO_PASSWORD` e `TUNNEL=funnel`
- [ ] Tailscale: máquina autenticada, nome do tailnet definitivo, HTTPS ligado, Funnel permitido, *key expiry*
      desligado
- [ ] Modo de acesso decidido (`ACESSO_RESTRITO`, seção 1.4) e conferido no `preflight`
- [ ] Teste em janela anônima e de outra rede: URL (ou link `organizador`, se restrito) → login
      demo01 → painel e plano com o perfil esperado
- [ ] `https://studyenem.<tailnet>.ts.net/health` mostra `Healthy` numa janela anônima
- [ ] Login com Google: decidido se fica ligado na avaliação (seção 1.5). Se ficar, a origem `.ts.net` está
      cadastrada e o botão funciona pela URL pública
- [ ] `scripts/avaliacao.sh reset` rodado logo antes dos convites (registrado em `avaliacao-registro.log`)
- [ ] Credenciais do formulário e do roteiro atualizadas: conta demoNN (e link de convite, se restrito) por avaliador, e-mail da
      T4 distinto
- [ ] Monitor externo em `/health` (keyword `Healthy`) criado e alerta de teste recebido
- [ ] `scripts/avaliacao.sh preflight` termina em "Tudo no ar."

## 10. Encerrar a coleta

```bash
# guarda o banco final (simulados e cadastros feitos pelos avaliadores), útil para a análise
docker compose -f docker-compose.yml -f docker-compose.avaliacao.yml exec -T postgres \
  pg_dump -U studyenem studyenem > avaliacao-final.sql
scripts/avaliacao.sh down
```

Depois:
- no painel do Tailscale, remova a máquina `studyenem` (Machines → `…` → Remove). Para apagar também o login
  local: `docker volume rm studyenem-avaliacao_tailscale_state`;
- desfaça a configuração de suspensão (seção 1.1);
- não versione o `avaliacao-final.sql`, porque ele contém os e-mails usados na T4.
