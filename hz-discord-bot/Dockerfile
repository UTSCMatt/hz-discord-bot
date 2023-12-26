#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS dectalk
WORKDIR /dectalkSrc
RUN apt-get update && apt-get -y install build-essential libasound2-dev libpulse-dev libgtk2.0-dev unzip git
RUN git clone https://github.com/dectalk/dectalk.git
WORKDIR ./dectalk/src
RUN autoreconf -si
RUN ./configure
RUN make -j

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["hz-discord-bot/hz-discord-bot.csproj", "hz-discord-bot/"]
COPY ["DectalkNET/DectalkNET.csproj", "DectalkNET/"]
RUN dotnet restore "hz-discord-bot/hz-discord-bot.csproj"
COPY . .
WORKDIR "/src/hz-discord-bot"
RUN dotnet build "hz-discord-bot.csproj" -c Release -o /app/build /p:AllowUnsafeBlocks=true

FROM build AS publish
RUN dotnet publish "hz-discord-bot.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:AllowUnsafeBlocks=true

FROM base AS final
WORKDIR /app
RUN apt-get update && apt-get -y install libasound2-dev libpulse-dev
COPY --from=publish /app/publish .
COPY --from=dectalk /dectalkSrc/dectalk/dist/lib/libtts_us.so ./dectalk.dll
COPY --from=dectalk /dectalkSrc/dectalk/dist/dic/dtalk_us.dic ./dtalk_us.dic

STOPSIGNAL SIGINT

ENTRYPOINT ["dotnet", "hz-discord-bot.dll"]