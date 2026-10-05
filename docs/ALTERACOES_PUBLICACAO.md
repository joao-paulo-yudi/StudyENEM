# Alterações para a publicação da avaliação remota

Resumo do que mudou para publicar o StudyENEM desta máquina durante a avaliação heurística remota. Está
organizado por repositório, porque `backend/` e `frontend/` são submódulos com commits próprios. O passo a passo
de operação está em [AVALIACAO_REMOTA.md](AVALIACAO_REMOTA.md), e o do login com Google em
[LOGIN_GOOGLE.md](LOGIN_GOOGLE.md).

## Repositório raiz

| Arquivo | Alteração |
|---|---|
| `docker-compose.avaliacao.yml` (novo) | Override do ambiente de avaliação, com nome de projeto próprio (containers e banco separados do desenvolvimento). Detalhes abaixo da tabela. |
| `docker-compose.yml` | Senha do PostgreSQL lida de `POSTGRES_PASSWORD`, com o valor de desenvolvimento como reserva. |
| `.env.example` (novo) | Modelo versionado do `.env`: senhas, chave JWT, `GOOGLE_CLIENT_ID`, contas demo, `ACESSO_RESTRITO`, porta, `TUNNEL`, `TS_AUTHKEY`. Comentários explicam onde obter cada valor. |
| `scripts/avaliacao.sh` (novo) | Operação do ambiente. Comandos listados abaixo da tabela. Cada `up`, `reset`, `down` e geração de convites fica registrado em `avaliacao-registro.log`. |
| `avaliacao/nginx/server/convite.conf` (novo) | Portão de convite opcional e `robots.txt`. Detalhes abaixo da tabela. |
| `avaliacao/tailscale/serve.json` (novo) | Configuração do Funnel: porta 443 → `http://frontend:80`. |
| `docs/AVALIACAO_REMOTA.md` (novo) | Runbook: preparação da máquina, Tailscale, convites, abertura da coleta (com o CEP como pré-requisito), credenciais por avaliador, verificação diária, monitor, reset, logs, problemas comuns, checklist e encerramento. |
| `docs/LOGIN_GOOGLE.md` (novo) | Fluxo do login com Google, origens a cadastrar, user-secrets para o `dotnet run` e regras de vínculo de contas. |
| `docs/ALTERACOES_PUBLICACAO.md` (novo) | Este arquivo. |
| `README.md` | Seções de Google e de avaliação remota apontam para `docs/`. O Swagger é descrito só no desenvolvimento, e o botão da Microsoft deixa de aparecer na descrição do RF01. |
| `.gitignore` | Ignora `avaliacao-registro.log`, `avaliacao-final.sql`, `avaliacao/convites.txt`, `avaliacao/nginx/convites.conf` e `avaliacao/credenciais-teste.md`. |

**`docker-compose.avaliacao.yml`:**
- nenhuma porta da API no host;
- nginx só em `127.0.0.1:8088`;
- `restart: unless-stopped` em todos os serviços;
- `POSTGRES_PASSWORD`, `JWT_KEY` e `DEMO_PASSWORD` obrigatórios;
- contas `demo01..demoNN`;
- Google com o mesmo client ID, podendo ser desligado só na avaliação por `AVALIACAO_GOOGLE_CLIENT_ID=`;
- portão de convite montado no nginx, desligado por padrão (`ACESSO_RESTRITO=false`: acesso aberto);
- serviços de publicação `tailscale` (Funnel, URL estável) e `tunnel-quick` (Cloudflare, plano B).

**`scripts/avaliacao.sh`:** `up`, `status` (verificação diária), `preflight` (checklist), `convites`, `url`, `reset`, `seed`, `logs`, `ps`, `down`.

**`avaliacao/nginx/server/convite.conf`:**
- `robots.txt` que pede aos buscadores para não indexar a URL de teste;
- `/convite/<código>` grava um cookie de 30 dias e leva ao login;
- só com `ACESSO_RESTRITO=true`: sem convite válido, toda a aplicação responde 403 com a página "Acesso restrito",
  e o `/health` fica livre para o monitor. No padrão, o acesso é aberto a qualquer pessoa.

## Backend (submódulo `backend/`)

| Arquivo | Alteração |
|---|---|
| `Program.cs` | Detalhes abaixo da tabela. |
| `Infrastructure/ForwardedHeadersSetup.cs` (novo) | Detalhes abaixo da tabela. |
| `Infrastructure/DatabaseHealthCheck.cs` (novo) | `/health` só responde `Healthy` quando o banco aceita conexões. |
| `Properties/launchSettings.json` (novo) | `dotnet run` e `dotnet watch` sobem em `Development` em `http://localhost:8080`, a porta que o frontend espera. |
| `StudyENEM.API.csproj` | `UserSecretsId`, para o client ID do Google no `dotnet run` sem segredo no repositório. |
| `Controllers/AuthController.cs`, `DTOs/AuthDto.cs` | `GET /api/auth/config` informa `providers` (só os provedores configurados) além do `googleClientId`. |
| `Data/DemoSeed.cs` | Detalhes abaixo da tabela. |
| `Data/DatabaseInitializer.cs` | Repassa a configuração das contas demo ao seed. |
| `Services/PerformanceCalculator.cs`, `Services/DashboardService.cs` | Desempate determinístico (nome, depois id) na ordem de conteúdos e disciplinas: o painel e o plano listam sempre os mesmos itens nas mesmas posições. |
| `StudyENEM.Tests/ForwardedHeadersSetupTests.cs` (novo) | 3 testes: esquema e IP pelo túnel, IPv4 mapeado do Kestrel, cabeçalhos de fora da rede do Docker ignorados. |
| `StudyENEM.Tests/PerformanceCalculatorTests.cs` (novo) | Ordem dos conteúdos empatados é a mesma com as respostas embaralhadas. |

**`Program.cs`:**
- `UseForwardedHeaders` no início do pipeline;
- CORS só em Development, restrito a `localhost:4200`, no lugar do `AllowAnyOrigin`;
- Swagger só em Development;
- recusa subir com chave JWT de menos de 32 caracteres;
- health check com banco;
- configuração `Demo`;
- comandos `demo seed` e `demo reset`, que rodam e encerram sem subir o servidor.

**`Infrastructure/ForwardedHeadersSetup.cs`:**
- `X-Forwarded-For` e `X-Forwarded-Proto` vindos das redes privadas do Docker;
- dois saltos confiáveis (túnel e nginx).

**`Data/DemoSeed.cs`:**
- contas `demo01..demoNN` (`Demo__Accounts`, `Demo__Password`) só na avaliação, e `joao@` só no desenvolvimento;
- histórico **por conta**, com semente fixa e questões ordenadas: todas as contas são idênticas;
- MT é claramente a área mais fraca, e Eletricidade e magnetismo (CN) é o conteúdo prioritário isolado;
- `Reset` apaga simulados e contas fora da lista demo, mantendo os ids das demos, e se recusa a rodar no desenvolvimento.

Total: 71 → 72 testes, todos passando.

## Frontend (submódulo `frontend/`)

| Arquivo | Alteração |
|---|---|
| `nginx.conf` | Detalhes abaixo da tabela. |
| `src/app/features/login/*` | Detalhes abaixo da tabela. |
| `src/app/core/api.service.ts` | Tipo `SocialProvider` e campo `providers` em `AuthConfigDto`. |
| `src/environments/environment*.ts` | `loginPrefill`: a conta `joao@` é preenchida só em desenvolvimento, e em produção os campos abrem vazios. |
| `src/styles.css` | Restaurada a regra que esconde o painel da marca abaixo de 880px. Ela tinha sido removida por engano no commit `30b8939`. |
| `src/favicon.svg` (novo), `src/index.html`, `angular.json` | Logo do app como ícone da página, no lugar do `favicon.ico`, que não existia. |

**`nginx.conf`:**
- repassa `X-Forwarded-For`, `X-Forwarded-Proto` e `X-Forwarded-Host`;
- gzip;
- cache imutável dos arquivos com hash e `no-cache` no `index.html`;
- DNS interno do Docker com reresolução, para que recriar a API não cause 502;
- `/health` repassado à API;
- `include` das regras da avaliação (`robots.txt` e portão opcional), que fica vazio no desenvolvimento.

**`src/app/features/login/*`:**
- cada botão social só aparece se o servidor listar o provedor;
- o Microsoft não aparece, porque não há integração;
- removidos o botão falso do Google e a mensagem "disponível em breve";
- espaçamento sem divisor quando não há provedores.

## Fora dos repositórios (configuração desta máquina e de contas)

- `.env`: segredos gerados (`POSTGRES_PASSWORD`, `JWT_KEY`), `DEMO_PASSWORD`, `GOOGLE_CLIENT_ID`, `TUNNEL=funnel`,
  `ACESSO_RESTRITO=false` e `DOCKER_CONTEXT_AVALIACAO=default`.
- Docker: usuário no grupo `docker`. A avaliação roda no Docker Engine do sistema, que sobe no boot sem login, e não
  no Docker Desktop. O login do Tailscale foi copiado do volume do Desktop, mantendo a URL. O Docker Desktop inicia
  no login (`systemctl --user enable docker-desktop`), para o ambiente de desenvolvimento.
- Tailscale:
  - máquina `studyenem` no tailnet `tail8c88d4.ts.net`;
  - HTTPS ligado;
  - atributo `funnel` na política;
  - chave de uso único já consumida e apagada do `.env`.
- Google Cloud Console: origem `https://studyenem.tail8c88d4.ts.net` cadastrada no cliente OAuth.
- GNOME: suspensão automática na tomada desligada. Para desfazer:
  `gsettings reset org.gnome.settings-daemon.plugins.power sleep-inactive-ac-type`.

## Pendências conhecidas (não alteradas de propósito)

- O índice de dificuldade é só a proporção de erros, sem considerar o tamanho da amostra. É um achado legítimo para
  os avaliadores e um tema para o TCC.
- LC tem percentual maior que CH, mas TRI menor: é o comportamento real da TRI.
- O placeholder `••••••••` no campo de senha pode parecer uma senha já preenchida. É uma decisão de design, dentro
  do escopo da avaliação.
