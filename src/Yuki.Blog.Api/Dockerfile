# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Yuki.Blog.sln", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["src/Yuki.Blog.Domain/Yuki.Blog.Domain.csproj", "src/Yuki.Blog.Domain/"]
COPY ["src/Yuki.Blog.Application/Yuki.Blog.Application.csproj", "src/Yuki.Blog.Application/"]
COPY ["src/Yuki.Blog.Infrastructure/Yuki.Blog.Infrastructure.csproj", "src/Yuki.Blog.Infrastructure/"]
COPY ["src/Yuki.Blog.Api/Yuki.Blog.Api.csproj", "src/Yuki.Blog.Api/"]

# Restore dependencies
RUN dotnet restore "src/Yuki.Blog.Api/Yuki.Blog.Api.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/src/Yuki.Blog.Api"
RUN dotnet publish "Yuki.Blog.Api.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application from build stage
COPY --from=build /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "Yuki.Blog.Api.dll"]
