FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
WORKDIR "/src/Vanilla.TelegramBot"
RUN dotnet build "Vanilla.TelegramBot.csproj" -c Release -o /app

FROM build AS publish
WORKDIR "/src/Vanilla.TelegramBot"
RUN dotnet publish "Vanilla.TelegramBot.csproj" -c Release -o /app
    
FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Vanilla.TelegramBot.dll"]