# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY CreativeBudgeting/*.csproj CreativeBudgeting/
RUN dotnet restore CreativeBudgeting/CreativeBudgeting.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/CreativeBudgeting
RUN dotnet build "CreativeBudgeting.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "CreativeBudgeting.csproj" -c Release -o /app/publish

# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy the published application
COPY --from=publish /app/publish .

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Expose port
EXPOSE 10000

# Set environment variables
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:10000/api/budget/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "CreativeBudgeting.dll"]