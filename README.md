# StudyENEM

Plataforma de apoio ao planejamento de estudos para o ENEM, baseada em Learning Analytics.

## Stack

- **Backend**: C# .NET 8 + SQLite (via EF Core)
- **Frontend**: Angular 17 (standalone components)
- **Infra**: Docker + Docker Compose

## Clonando o projeto

> ⚠️ O backend e o frontend são **submódulos** Git. Clone com `--recurse-submodules` para trazer o código deles:

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
- Swagger: http://localhost:8080/swagger

## Desenvolvimento local

### Backend

```bash
cd backend/StudyENEM.API
dotnet run
```

### Frontend

```bash
cd frontend
npm install
npm start
```

> Em desenvolvimento o frontend aponta para `http://localhost:8080/api` diretamente.

## Funcionalidades (v0.1)

- Banco de questões ENEM (por área e ano)
- Simulado configurável (filtro por ano/área)
- Resultado detalhado por questão e por área
- Dashboard com histórico e indicadores de desempenho por matéria

## Estrutura

```
StudyENEM/
├── backend/
│   └── StudyENEM.API/
│       ├── Controllers/   # Questions, Attempts, Dashboard
│       ├── Data/          # DbContext + Seed
│       ├── DTOs/
│       ├── Models/
│       └── Services/
├── frontend/
│   └── src/app/
│       ├── core/          # ApiService, StudentService
│       └── features/
│           ├── home/
│           ├── exam/
│           ├── result/
│           └── dashboard/
└── docker-compose.yml
```
