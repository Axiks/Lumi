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
COPY ["Vanilla.Aspire/Vanilla.Aspire.ApiService/", "Vanilla.Aspire/Vanilla.Aspire.ApiService/"]
COPY ["Vanilla.Aspire/Vanilla.Aspire.ServiceDefaults/", "Vanilla.Aspire/Vanilla.Aspire.ServiceDefaults/"]

WORKDIR /src/Vanilla.Aspire/Vanilla.Aspire.ApiService
RUN dotnet restore "./Vanilla.Aspire.ApiService.csproj"
RUN dotnet build "./Vanilla.Aspire.ApiService.csproj" -c "$BUILD_CONFIGURATION" -o /app --property AssemblyVersion=1.2.3.4

FROM build AS publish
WORKDIR "/src/Vanilla.Aspire/Vanilla.Aspire.ApiService"
RUN dotnet publish "Vanilla.Aspire.ApiService.csproj" -c "$BUILD_CONFIGURATION" -o /app --property AssemblyVersion=1.2.3.4

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Vanilla.Aspire.ApiService.dll"]