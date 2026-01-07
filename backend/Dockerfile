# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files (in dependency order for efficient layer caching)
COPY src/eSamadhaan.Domain/eSamadhaan.Domain.csproj src/eSamadhaan.Domain/
COPY src/eSamadhaan.Application/eSamadhaan.Application.csproj src/eSamadhaan.Application/
COPY src/eSamadhaan.Infrastructure/eSamadhaan.Infrastructure.csproj src/eSamadhaan.Infrastructure/
COPY src/eSamadhaan.API/eSamadhaan.API.csproj src/eSamadhaan.API/

# Restore dependencies (restore in dependency order)
RUN dotnet restore src/eSamadhaan.Domain/eSamadhaan.Domain.csproj && \
    dotnet restore src/eSamadhaan.Application/eSamadhaan.Application.csproj && \
    dotnet restore src/eSamadhaan.Infrastructure/eSamadhaan.Infrastructure.csproj && \
    dotnet restore src/eSamadhaan.API/eSamadhaan.API.csproj

# Copy all source files
COPY src/ src/

# Build the application
WORKDIR /src/src/eSamadhaan.API
RUN dotnet build -c Release -o /app/build --no-restore

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Copy entrypoint script
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Use entrypoint script
ENTRYPOINT ["/app/entrypoint.sh"]

