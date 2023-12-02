#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["hz-discord-bot/hz-discord-bot.csproj", "hz-discord-bot/"]
RUN dotnet restore "hz-discord-bot/hz-discord-bot.csproj"
COPY . .
WORKDIR "/src/hz-discord-bot"
RUN dotnet build "hz-discord-bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "hz-discord-bot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "hz-discord-bot.dll"]