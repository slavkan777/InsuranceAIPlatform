# syntax=docker/dockerfile:1
# =====================================================================
# InsuranceAIPlatform API (BFF / modular monolith) container image.
# Build context = ./server  (e.g. `docker build -f server/Dockerfile server`).
# Multi-stage: SDK builds + publishes; aspnet runtime runs as non-root on :8080.
# Matches infra/modules/container-apps.bicep (targetPort 8080, ASPNETCORE_URLS=http://+:8080).
# No secrets are baked in — DB connection string + AI keys are injected at runtime
# (env / Managed Identity / Key Vault). AI defaults to Mock.
# =====================================================================

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only the csproj files first so `dotnet restore` is cached unless a
# project file changes (the API + the projects it transitively references).
COPY InsuranceAIPlatform.Api/InsuranceAIPlatform.Api.csproj InsuranceAIPlatform.Api/
COPY InsuranceAIPlatform.BuildingBlocks/InsuranceAIPlatform.BuildingBlocks.csproj InsuranceAIPlatform.BuildingBlocks/
COPY InsuranceAIPlatform.Services.Claims/InsuranceAIPlatform.Services.Claims.csproj InsuranceAIPlatform.Services.Claims/
COPY InsuranceAIPlatform.Services.CustomersPolicies/InsuranceAIPlatform.Services.CustomersPolicies.csproj InsuranceAIPlatform.Services.CustomersPolicies/
COPY InsuranceAIPlatform.Services.Documents/InsuranceAIPlatform.Services.Documents.csproj InsuranceAIPlatform.Services.Documents/
COPY InsuranceAIPlatform.Services.AiAnalysis/InsuranceAIPlatform.Services.AiAnalysis.csproj InsuranceAIPlatform.Services.AiAnalysis/
COPY InsuranceAIPlatform.Services.Approval/InsuranceAIPlatform.Services.Approval.csproj InsuranceAIPlatform.Services.Approval/
COPY InsuranceAIPlatform.Services.AuditCost/InsuranceAIPlatform.Services.AuditCost.csproj InsuranceAIPlatform.Services.AuditCost/
RUN dotnet restore InsuranceAIPlatform.Api/InsuranceAIPlatform.Api.csproj

# Copy the remaining source and publish the API (no apphost → smaller image).
COPY . .
RUN dotnet publish InsuranceAIPlatform.Api/InsuranceAIPlatform.Api.csproj \
      -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Container Apps injects ASPNETCORE_URLS, but default it here for `docker run` parity.
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080

COPY --from=build /app/publish .

# Run as the non-root user shipped in the .NET runtime image (UID 1654).
USER $APP_UID

ENTRYPOINT ["dotnet", "InsuranceAIPlatform.Api.dll"]
