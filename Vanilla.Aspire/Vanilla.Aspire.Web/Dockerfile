ARG DOTNET_VERSION=9.0
ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
RUN dotnet dev-certs https
RUN dotnet dev-certs https --trust
WORKDIR /src
COPY ["Vanilla App/", "Vanilla App/"]
COPY ["Vanilla.Common/", "Vanilla.Common/"]
COPY ["Vanilla.Data/", "Vanilla.Data/"]
COPY ["Vanilla.OAuth/", "Vanilla.OAuth/"]
COPY ["Vanilla.TelegramBot/", "Vanilla.TelegramBot/"]
COPY ["Vanilla.Aspire/Vanilla.Aspire.ApiService/", "Vanilla.Aspire/Vanilla.Aspire.ApiService/"]
COPY ["Vanilla.Aspire/Vanilla.Aspire.Web/", "Vanilla.Aspire/Vanilla.Aspire.Web/"]
COPY ["Vanilla.Aspire/Vanilla.Aspire.ServiceDefaults/", "Vanilla.Aspire/Vanilla.Aspire.ServiceDefaults/"]

WORKDIR /src/Vanilla.Aspire/Vanilla.Aspire.Web
RUN dotnet restore "./Vanilla.Aspire.Web.csproj"
RUN dotnet build "./Vanilla.Aspire.Web.csproj" -c "$BUILD_CONFIGURATION" -o /app --property AssemblyVersion=1.2.3.4

FROM build AS publish
WORKDIR "/src/Vanilla.Aspire/Vanilla.Aspire.Web"
RUN find -name "appsettings*.json" -exec rm {} \;
RUN dotnet publish "Vanilla.Aspire.Web.csproj" -c "$BUILD_CONFIGURATION" -o /app --property AssemblyVersion=1.2.3.4

FROM base AS final
WORKDIR /app
COPY --from=publish /root/.dotnet/corefx/cryptography/x509stores/my/* /root/.dotnet/corefx/cryptography/x509stores/my/
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Vanilla.Aspire.Web.dll"]