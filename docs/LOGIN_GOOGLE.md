# Login com Google

O botão **Continuar com Google** é opcional. Ele aparece na tela de login somente quando o backend tem um ID de cliente
OAuth configurado. Sem ele, o acesso continua por e-mail e senha e o botão não é exibido.

## Como funciona

1. A tela de login consulta `GET /api/auth/config`. Quando o Google está configurado, a resposta é
   `{"providers":["google"],"googleClientId":"<id>"}`.
2. O frontend carrega o script do Google Identity Services (`https://accounts.google.com/gsi/client`) e desenha o
   botão oficial. O login abre num popup.
3. O Google devolve ao navegador um **ID token** (JWT), que o frontend envia para `POST /api/auth/google`.
4. O backend valida o token com `GoogleJsonWebSignature`, que confere a assinatura, o emissor, a validade e se o token
   foi emitido para o nosso ID de cliente. Depois disso, cria a conta ou vincula a conta existente (veja
   [Contas e vínculo](#contas-e-vínculo)).

O fluxo **não usa client secret nem URI de redirecionamento**. O Google só confere de qual **origem JavaScript** o
botão foi aberto. O ID do cliente não é segredo, porque o navegador o recebe, mas fica fora do repositório como o resto
da configuração.

## Onde o ID do cliente é configurado

| Ambiente | Endereço no navegador | Como o backend recebe o ID |
|---|---|---|
| Desenvolvimento: `dotnet run` + `npm start` | http://localhost:4200 | user-secrets (abaixo) |
| Docker de desenvolvimento: `docker compose up` | http://localhost:4200 | `GOOGLE_CLIENT_ID` no `.env` |
| Avaliação: `docker-compose.avaliacao.yml` | https://studyenem.tail8c88d4.ts.net | o mesmo `GOOGLE_CLIENT_ID` do `.env` |

Para desligar o Google **só** na avaliação, defina `AVALIACAO_GOOGLE_CLIENT_ID=` (vazio) no `.env` e rode
`scripts/avaliacao.sh up`. Pontos a considerar com ele ligado:
- uma conta Google nova entra **vazia**, sem o histórico das contas demoNN;
- enquanto o app OAuth estiver *Em teste*, só os usuários de teste cadastrados no Console conseguem entrar. Para
  os demais, o popup do Google mostra *Erro 403: access_denied*;
- o `reset` da avaliação apaga as contas criadas pelo Google, que são recriadas no próximo login.

## Passo a passo no Google Cloud Console

Os nomes dos menus são os da interface atual, chamada *Google Auth Platform*. Em versões anteriores do console, os
mesmos itens ficavam em **APIs e serviços → Tela de consentimento OAuth / Credenciais**.

1. Em https://console.cloud.google.com, crie um projeto (por exemplo, `StudyENEM`) ou selecione um existente.
2. **Google Auth Platform → Branding:** se o console pedir, configure a tela de consentimento com o nome do app
   (`StudyENEM`), o e-mail de suporte e o e-mail de contato do desenvolvedor.
3. **Google Auth Platform → Audience (Público-alvo):**
   - Tipo de usuário: **Externo**.
   - Enquanto o status de publicação for **Em teste**, só as contas listadas em **Usuários de teste** conseguem entrar.
     Adicione a sua conta Google ali. Sem isso, o popup mostra *Erro 403: access_denied*.
4. **Google Auth Platform → Clients (Clientes) → Criar cliente:**
   - Tipo de aplicativo: **Aplicativo da Web**.
   - Nome: por exemplo, `StudyENEM web`.
   - **Origens JavaScript autorizadas:** veja a lista abaixo.
   - **URIs de redirecionamento autorizados:** deixe vazio.
5. Copie o **ID do cliente** (`<números>-<letras>.apps.googleusercontent.com`). O console também gera uma *chave
   secreta do cliente*: ela não é usada e não deve ser copiada para nenhum lugar.

As mudanças de origem podem levar de alguns minutos a algumas horas para valer no Google.

### Origens JavaScript autorizadas

Cadastre exatamente estas, sem barra no final e sem caminho:

```
http://localhost
http://localhost:4200
```

- `http://localhost` sem porta é exigido pelo Google Identity Services para testes locais, junto com a origem que
  inclui a porta.
- `http://localhost:4200` é a mesma origem para o `npm start` e para o `docker compose up` de desenvolvimento.
- `http://127.0.0.1:4200` é **outra origem**. Use sempre `localhost` no navegador.

Para a URL pública da avaliação (Tailscale Funnel), cadastre também:

```
https://studyenem.tail8c88d4.ts.net
```

`http://localhost:8088`, o acesso local da avaliação, não precisa ser cadastrado: teste o Google pela URL pública.
Nunca cadastre a URL de um quick tunnel (`*.trycloudflare.com`), porque ela muda a cada reinício.

## Configurar o ID do cliente

**Docker** (`docker compose up`): no `.env` da raiz, criado a partir do `.env.example`:

```
GOOGLE_CLIENT_ID=<id>.apps.googleusercontent.com
```

Depois recrie o backend para ele ler o valor novo: `docker compose up -d backend`.

**`dotnet run` / `dotnet watch`:** a API não lê o `.env`. Grave o ID nos user-secrets do .NET, que ficam fora do
repositório (em `~/.microsoft/usersecrets/`) e são carregados no ambiente `Development`, o mesmo definido pelo
`launchSettings.json`:

```bash
cd backend/StudyENEM.API
dotnet user-secrets set "Google:ClientId" "<id>.apps.googleusercontent.com"
dotnet user-secrets list      # confere
dotnet user-secrets remove "Google:ClientId"   # desliga de novo
```

Para conferir, `curl http://localhost:8080/api/auth/config` deve listar `"google"` em `providers`. Isso vale tanto para o
`dotnet run` quanto para o Docker de desenvolvimento, que também publica a API na porta 8080.

## Contas e vínculo

O backend procura a conta primeiro pelo identificador do Google (claim `sub`) e depois pelo e-mail:

| Situação | Resultado |
|---|---|
| Nenhuma conta com esse Google nem com esse e-mail | Cria a conta, sem senha local. O nome vem do Google (ou da parte local do e-mail). |
| Conta criada por e-mail e senha com o mesmo e-mail | **Vincula** o Google à conta existente. O histórico é preservado e a senha continua funcionando. |
| Conta já vinculada a este Google | Entra normalmente, mesmo que o e-mail da conta Google tenha mudado. |
| E-mail já vinculado a **outra** conta Google | Recusa: "Este e-mail já está vinculado a outra conta Google." |
| Conta Google com e-mail não verificado | Recusa: "A conta Google precisa ter um e-mail verificado." |

Uma conta criada pelo Google não tem senha. Por isso:

- o login por e-mail e senha recusa essa conta com a mensagem genérica "E-mail ou senha inválidos.";
- o cadastro com o mesmo e-mail responde "Este e-mail já está vinculado a uma conta Google".

Nunca existem duas contas com o mesmo e-mail: o banco tem índice único em `email` e em `google_id`.

## Problemas comuns

| Sintoma | Causa provável |
|---|---|
| O botão não aparece | O backend não recebeu o ID. Confira `/api/auth/config` e, no Docker, se o backend foi recriado depois de editar o `.env`. |
| Botão aparece, mas o console do navegador mostra `The given origin is not allowed for the given client ID` | A origem não foi cadastrada (ou ainda não propagou), ou o navegador está em `127.0.0.1` em vez de `localhost`. |
| Popup mostra *Erro 403: access_denied* | App em modo de teste e a conta não está em **Usuários de teste**. |
| Popup abre e fecha sem entrar | Bloqueador de popups ou de cookies de terceiros para `accounts.google.com`. |
| "Não foi possível validar sua conta Google" | O ID do cliente do backend é diferente do usado pelo botão. Os dois vêm da mesma configuração, então reinicie o backend. |
