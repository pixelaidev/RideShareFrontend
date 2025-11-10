# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore NuGets (caches layers for faster builds)
COPY *.csproj .
RUN dotnet restore

# Copy source and publish Release
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose port (Render uses $PORT env var, but default 8080 for .NET)
EXPOSE 8080

# Health check (optional, for Render monitoring)
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 CMD curl --fail http://localhost:8080/health || exit 1

# Run the app (uses your RideShareFrontend.dll)
ENTRYPOINT ["dotnet", "RideShareFrontend.dll"]