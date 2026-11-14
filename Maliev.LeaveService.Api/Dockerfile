# syntax=docker/dockerfile:1.4
FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
ENV GITHUB_ACTIONS=false
RUN apt-get update && apt-get install -y --no-install-recommends git ca-certificates \
    && rm -rf /var/lib/apt/lists/*
RUN git clone --filter=blob:none --no-checkout https://github.com/MALIEV-Co-Ltd/Maliev.Aspire.git /Maliev.Aspire \
    && git -C /Maliev.Aspire checkout --detach d79f954b2e7224b48bec75af4826bb38dd5f2a20 \
    && test "$(git -C /Maliev.Aspire rev-parse HEAD)" = d79f954b2e7224b48bec75af4826bb38dd5f2a20 \
    && git clone --filter=blob:none --no-checkout https://github.com/MALIEV-Co-Ltd/Maliev.MessagingContracts.git /Maliev.MessagingContracts \
    && git -C /Maliev.MessagingContracts checkout --detach 2347023e8aa17dfd7071317c2ee5c4436e1d945a \
    && test "$(git -C /Maliev.MessagingContracts rev-parse HEAD)" = 2347023e8aa17dfd7071317c2ee5c4436e1d945a

COPY ["nuget.config", "."]
COPY ["Directory.Build.props", "."]
COPY ["Maliev.LeaveService.Api/Maliev.LeaveService.Api.csproj", "Maliev.LeaveService.Api/"]
COPY ["Maliev.LeaveService.Application/Maliev.LeaveService.Application.csproj", "Maliev.LeaveService.Application/"]
COPY ["Maliev.LeaveService.Domain/Maliev.LeaveService.Domain.csproj", "Maliev.LeaveService.Domain/"]
COPY ["Maliev.LeaveService.Infrastructure/Maliev.LeaveService.Infrastructure.csproj", "Maliev.LeaveService.Infrastructure/"]

RUN --mount=type=secret,id=nuget_username \
    --mount=type=secret,id=nuget_password \
    NUGET_USERNAME=$(cat /run/secrets/nuget_username) \
    NUGET_PASSWORD=$(cat /run/secrets/nuget_password) \
    dotnet restore "./Maliev.LeaveService.Api/Maliev.LeaveService.Api.csproj"

COPY . .
WORKDIR "/src/Maliev.LeaveService.Api"
RUN dotnet build "./Maliev.LeaveService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Maliev.LeaveService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN chown -R app:app /app
USER app
COPY --chown=app:app --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/leave/liveness || exit 1

ENTRYPOINT ["dotnet", "Maliev.LeaveService.Api.dll"]
