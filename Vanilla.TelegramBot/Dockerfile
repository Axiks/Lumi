ARG DOTNET_VERSION=9.0
ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Vanilla App/", "Vanilla App/"]
COPY ["Vanilla.Common/", "Vanilla.Common/"]
COPY ["Vanilla.Data/", "Vanilla.Data/"]
COPY ["Vanilla.OAuth/", "Vanilla.OAuth/"]
COPY ["Vanilla.TelegramBot/", "Vanilla.TelegramBot/"]
COPY ["Vanilla.Aspire/Vanilla.Aspire.ServiceDefaults/", "Vanilla.Aspire/Vanilla.Aspire.ServiceDefaults/"]

WORKDIR /src/Vanilla.TelegramBot
RUN dotnet restore "./Vanilla.TelegramBot.csproj"
RUN dotnet build "./Vanilla.TelegramBot.csproj" -c "$BUILD_CONFIGURATION" -o /app --property AssemblyVersion=1.2.3.4

FROM build AS publish
WORKDIR "/src/Vanilla.TelegramBot"
RUN dotnet publish "Vanilla.TelegramBot.csproj" -c "$BUILD_CONFIGURATION" -o /app --property AssemblyVersion=1.2.3.4

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Vanilla.TelegramBot.dll"]