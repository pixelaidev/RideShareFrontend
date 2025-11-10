# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore (no cache, no fallback folders)
COPY *.csproj .
RUN dotnet restore --no-cache /p:RestoreFallbackFolders=""

# Copy source and publish Release (no restore, disable fallback)
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore /p:RestoreFallbackFolders="" /p:RestoreDisableParallel=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose port (Render sets $PORT, default 8080)
EXPOSE 8080

# Health check (optional)
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 CMD curl --fail http://localhost:8080 || exit 1

# Run the app
ENTRYPOINT ["dotnet", "RideShareFrontend.dll"]