# StudyENEM

Plataforma de apoio ao planejamento de estudos para o ENEM baseada em Learning Analytics
(TCC — Bacharelado em Ciência da Computação, IFSP Campus Salto).

## Stack

- **Backend**: C# / ASP.NET Core 8 (API REST) + Entity Framework Core 8 (migrations) + **PostgreSQL 16**, autenticação por **JWT**
- **Frontend**: Angular 17 (standalone components) + Chart.js
- **Infra**: Docker + Docker Compose (nginx servindo o SPA e encaminhando `/api` e `/midia` ao backend)

## Funcionalidades

| Requisito | Implementação |
|---|---|
| RF01 | Cadastro e login por e-mail/senha com token JWT e **login com a conta Google** (veja [Login com Google](#login-com-google)). O botão da Microsoft está na tela, mas a integração ainda não foi feita |
| RF02 | Simulado **geral** no formato do ENEM (até a prova completa de 180 questões, na ordem do caderno) ou **focado** em uma área ou conteúdo; escolha da língua estrangeira |
| RF03 / RF09 | Banco com a **prova completa do ENEM 2022** (185 questões com inglês e espanhol), organizado por área, disciplina, conteúdo, habilidade e ano, com gabarito oficial e dificuldade TRI |
| RF04 | Correção automática com desempenho por área e por conteúdo (tabela `resultado`) |
| RF05 | Dashboard: taxa de acerto por área, evolução temporal geral e por área, comparação com o simulado anterior |
| RF06 | Plano de estudos priorizado pelo índice de dificuldade de cada conteúdo, com botão para treinar o conteúdo |
| RF08 | Histórico completo; cada simulado abre o resultado detalhado |
| RF10 | Cronômetro com tempo limite (3 min por questão), alerta no fim e envio automático |
| RNF03 | Endpoints protegidos por JWT; cada estudante só acessa os próprios dados |

**Métricas de desempenho** (Seção 3.1.2 do relatório):

1. taxa de acerto por área;
2. taxa de acerto por conteúdo;
3. evolução temporal;
4. índice de dificuldade por conteúdo;
5. tempo médio por questão.

Além delas, há a **nota estimada pela TRI**. Ela é calculada como no ENEM (modelo logístico de 3 parâmetros, EAP,
parâmetros oficiais dos itens) e foi validada contra notas oficiais dos microdados do INEP.

A origem dos dados, a metodologia de classificação e a validação da TRI estão em
[backend/tools/enem-import/README.md](backend/tools/enem-import/README.md).

## Clonando o projeto

> ⚠️ O backend e o frontend são **submódulos** Git. Clone com `--recurse-submodules`:

```bash
git clone --recurse-submodules https://github.com/joao-paulo-yudi/StudyENEM.git
```

Se você já clonou sem os submódulos:

```bash
git submodule update --init --recursive
```

Repositórios dos submódulos:

- Backend: https://github.com/joao-paulo-yudi/StudyENEM-backend
- Frontend: https://github.com/joao-paulo-yudi/StudyENEM-frontend

## Subir com Docker (recomendado)

```bash
docker compose up --build
```

- Frontend: http://localhost:4200
- API: http://localhost:8080/api
- Swagger: http://localhost:8080/swagger (use **Authorize** com o token de `POST /api/auth/login`)
- Conta de demonstração: `joao@studyenem.com` / `1234`. O histórico dela é gerado pelo próprio modelo da TRI, só para demonstrar os dashboards.

Na primeira subida, o backend aplica as migrations: cria as tabelas e carrega o banco de questões (veja [Banco de dados](#banco-de-dados)).

> ⚠️ **Banco criado por versão anterior:** o banco agora é criado por migrations, com as tabelas do DER (`usuario`,
> `simulado`, `resposta`, `questao`, `alternativa`, `resultado`, `area`, `disciplina`, `conteudo`), além de `habilidade`
> e `escala_tri`. Se o volume do PostgreSQL foi criado por uma versão anterior, sem migrations, o backend encerra com uma
> mensagem pedindo para recriá-lo:
>
> ```bash
> docker compose down -v && docker compose up --build
> ```

> ⚠️ **Projeto dentro do OneDrive:** o BuildKit não consegue ler os arquivos do contexto de build quando eles estão em
> uma pasta sincronizada do OneDrive. Mesmo baixados, eles continuam sendo *pontos de nova análise*, e o build falha com
> `invalid file request <arquivo>`. Para contornar, crie um arquivo `.env` na raiz com:
>
> ```
> DOCKER_BUILDKIT=0
> ```
>
> O compose passa a usar o builder clássico, que lê esses arquivos sem problema. Como esse builder é legado, a solução
> definitiva é manter o repositório fora do OneDrive ou desligar os Arquivos Sob Demanda.

Fora do ambiente de desenvolvimento, defina a chave dos tokens em um arquivo `.env` na raiz: `JWT_KEY=<ao menos 32 caracteres>`.

## Login com Google

O botão **Continuar com Google** usa o [Google Identity Services](https://developers.google.com/identity/gsi/web): o
navegador obtém um *ID token* e o backend o valida em `POST /api/auth/google` (assinatura, emissor, validade e ID do
cliente). O login é opcional — sem configuração, a tela mostra o botão decorativo e o acesso continua por e-mail e senha.

1. No [Google Cloud Console](https://console.cloud.google.com/apis/credentials), crie uma credencial
   **ID do cliente OAuth** do tipo *Aplicativo da Web*.
2. Em **Origens JavaScript autorizadas**, informe as origens do frontend — `http://localhost:4200` (Docker e
   `npm start`). Não é preciso configurar URIs de redirecionamento: o fluxo do GSI não usa redirect.
3. Adicione o ID do cliente ao `.env` na raiz:

   ```
   GOOGLE_CLIENT_ID=<id>.apps.googleusercontent.com
   ```

   Em desenvolvimento local (`dotnet run`), a mesma configuração vale como variável de ambiente
   `Google__ClientId` ou em `appsettings.json` (`{ "Google": { "ClientId": "..." } }`).

O frontend descobre o ID do cliente em `GET /api/auth/config`, então trocá-lo não exige recompilar o Angular.

Na primeira entrada, a conta Google cria o cadastro do estudante (sem senha local). Se já existir uma conta com o mesmo
e-mail, ela é vinculada ao Google e passa a aceitar os dois modos de login.

## Banco de dados

O esquema e o banco de questões são criados por **migrations do EF Core** (`backend/StudyENEM.API/Data/Migrations/`),
aplicadas automaticamente quando a API sobe:

| Migration | Conteúdo |
|---|---|
| `CriacaoInicial` | Tabelas do DER, além de `habilidade` e `escala_tri` |
| `CargaEnem2022` | Executa `Sql/enem_2022.sql`: 4 áreas, 120 habilidades, 15 disciplinas, 58 conteúdos, escalas da TRI, 185 questões e 925 alternativas |

As questões ficam no banco, carregadas por esse script. As **imagens são arquivos** em
`backend/StudyENEM.API/wwwroot/midia/`, servidos pelo backend em `/midia`, que é o caminho usado no Markdown das
questões. Tanto o script quanto as imagens são gerados pelo importador a partir de dados públicos, principalmente do
INEP (veja [backend/tools/enem-import/README.md](backend/tools/enem-import/README.md)).
O código C# só cria a conta de demonstração e o histórico dela (`DemoSeed`), que não fazem parte do banco de questões.

Comandos úteis, a partir de `backend/`:

```bash
dotnet tool restore                                   # instala o dotnet-ef na versão fixada no repositório
dotnet ef database update --project StudyENEM.API     # aplica as migrations sem subir a API
dotnet ef migrations add <Nome> --project StudyENEM.API --output-dir Data/Migrations   # após alterar o modelo
```

## Desenvolvimento local

### Backend

```bash
cd backend/StudyENEM.API
dotnet run            # requer PostgreSQL em localhost:5432 (usuário/senha/banco: studyenem); aplica as migrations na subida
```

### Testes

```bash
cd backend
dotnet test           # inclui a validação da TRI contra 48 notas oficiais do ENEM 2022
```

### Frontend

```bash
cd frontend
npm install
npm start
```

> Em desenvolvimento, o frontend acessa `http://localhost:8080/api` e as imagens em `http://localhost:8080/midia`.

### Regenerar o banco de questões

```bash
cd backend
python tools/enem-import/importar_enem.py   # reescreve os scripts de carga em Data/Migrations/Sql/
```

> Uma migration já aplicada não roda de novo. Depois de regenerar o script, recrie o banco (`docker compose down -v`).

## Estrutura

```
StudyENEM/
├── backend/
│   ├── StudyENEM.API/
│   │   ├── Controllers/        # Auth, Questions, Attempts, Dashboard
│   │   ├── Data/               # AppDbContext (mapeamento para o DER), DatabaseInitializer, DemoSeed
│   │   │   └── Migrations/     # CriacaoInicial, CargaEnem2022 (+ Sql/enem_2022.sql) e LoginComGoogle
│   │   ├── DTOs/
│   │   ├── Infrastructure/     # filtro de exceções, claims do JWT
│   │   ├── Models/             # entidades
│   │   ├── Services/           # TriScorer, PerformanceCalculator, ExamService, DashboardService, GoogleAuthService, ...
│   │   └── wwwroot/midia/      # imagens das questões
│   ├── StudyENEM.Tests/        # xUnit: validação da TRI e seleção de questões
│   └── tools/enem-import/      # importador dos dados do ENEM (Python) que gera o SQL da carga
├── frontend/
│   └── src/app/
│       ├── core/               # ApiService, sessão JWT, interceptor, login com Google, gráficos, formatação
│       ├── shared/             # chart, sidebar, pipe de Markdown
│       └── features/           # login, home, simulado, resultado, desempenho, plano, questoes
└── docker-compose.yml
