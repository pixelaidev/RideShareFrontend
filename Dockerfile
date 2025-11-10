# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Clean NuGet cache to remove Windows paths
RUN dotnet nuget locals all --clear

# Copy csproj and restore (no cache, no fallback, ignore failed sources)
COPY *.csproj .
RUN dotnet restore --no-cache /p:RestoreFallbackFolders="" /p:RestoreIgnoreFailedSources=true

# Copy source (exclude local NuGet.Config if present to avoid Windows paths)
COPY . .
RUN rm -f NuGet.Config  # Remove any copied NuGet.Config with Windows paths

# Publish Release (no restore, no fallback)
RUN dotnet publish -c Release -o /app/publish --no-restore /p:RestoreFallbackFolders="" /p:RestoreIgnoreFailedSources=true

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose port (Render sets $PORT)
EXPOSE $PORT

# Health check (optional)
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 CMD curl --fail http://localhost:$PORT || exit 1

# Run the app
ENTRYPOINT ["dotnet", "RideShareFrontend.dll"]