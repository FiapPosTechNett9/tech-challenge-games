# FIAP Cloud Games — Microserviço Games (.NET 8)

Estrutura em camadas (igual ao microserviço **Users**):

- FIAP.CloudGames.Games.API
- FIAP.CloudGames.Games.Application
- FIAP.CloudGames.Games.Domain
- FIAP.CloudGames.Games.Infrastructure

## Rotas

O serviço usa `PathBase`:

- Base: `/games`
- Swagger (DEV): `/games/swagger`

## Banco (PostgreSQL / AWS RDS)

Ajuste a connection string em `src/FIAP.CloudGames.Games.API/appsettings.json`:

- `ConnectionStrings:DefaultConnection`

## Migrations

```bash
dotnet ef migrations add InitialGames       --project src/FIAP.CloudGames.Games.Infrastructure       --startup-project src/FIAP.CloudGames.Games.API

dotnet ef database update       --project src/FIAP.CloudGames.Games.Infrastructure       --startup-project src/FIAP.CloudGames.Games.API
```

## JWT

O serviço **apenas valida** JWT (Bearer).  
Configure `JwtSettings` para ser compatível com o microserviço **Users**.
