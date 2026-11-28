# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore con el cache de capas: solo se invalida si cambian los .csproj.
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Web/Web.csproj src/Web/
RUN dotnet restore src/Web/Web.csproj

COPY . .
RUN dotnet publish src/Web/Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Data Protection necesita un HOME escribible cuando corre como usuario no-root.
RUN mkdir -p /app/.aspnet/DataProtection-Keys \
    && chown -R app:app /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
ENV HOME=/app

EXPOSE 8080
USER app

ENTRYPOINT ["dotnet", "Web.dll"]
