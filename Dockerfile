FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FIAP.CloudGames.Games.sln ./
COPY src/FIAP.CloudGames.Games.API/FIAP.CloudGames.Games.API.csproj src/FIAP.CloudGames.Games.API/
COPY src/FIAP.CloudGames.Games.Application/FIAP.CloudGames.Games.Application.csproj src/FIAP.CloudGames.Games.Application/
COPY src/FIAP.CloudGames.Games.Domain/FIAP.CloudGames.Games.Domain.csproj src/FIAP.CloudGames.Games.Domain/
COPY src/FIAP.CloudGames.Games.Infrastructure/FIAP.CloudGames.Games.Infrastructure.csproj src/FIAP.CloudGames.Games.Infrastructure/
COPY . .

RUN dotnet restore src/FIAP.CloudGames.Games.API/FIAP.CloudGames.Games.API.csproj

RUN dotnet publish src/FIAP.CloudGames.Games.API/FIAP.CloudGames.Games.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "FIAP.CloudGames.Games.API.dll"]