# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the entire source
COPY . .

# Publish in Release mode
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published app
COPY --from=build /app/publish .

# Render automatically sets $PORT — we must listen on it.
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

EXPOSE 7777

ENTRYPOINT ["dotnet", "RideShareFrontend.dll"]
