# См. статью https://aka.ms/customizecontainer

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["kufar-to-telegram/kufar-to-telegram.csproj", "kufar-to-telegram/"]
RUN dotnet restore "kufar-to-telegram/kufar-to-telegram.csproj"
COPY ./kufar-to-telegram ./kufar-to-telegram
WORKDIR "/src/kufar-to-telegram"
RUN dotnet build "kufar-to-telegram.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "kufar-to-telegram.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "kufar-to-telegram.dll"]
