.PHONY: help build run test e2e e2e-smoke e2e-critical docker-up docker-down docker-db docker-seq docker-logs docker-clean info

# Default target
.DEFAULT_GOAL := help

# Colors for output
BLUE := \033[0;34m
GREEN := \033[0;32m
YELLOW := \033[1;33m
RED := \033[0;31m
NC := \033[0m # No Color

##@ Help

help: ## Display this help message
	@echo "$(BLUE)Yuki Blog - Makefile Commands$(NC)"
	@echo ""
	@awk 'BEGIN {FS = ":.*##"; printf "Usage:\n  make $(GREEN)<target>$(NC)\n"} /^[[:alnum:]_-]+:.*##/ { printf "  $(GREEN)%-20s$(NC) %s\n", $$1, $$2 } /^##@/ { printf "\n$(YELLOW)%s$(NC)\n", substr($$0, 5) } ' $(MAKEFILE_LIST)

##@ Build

build: ## Build the solution
	@echo "$(BLUE)Building solution...$(NC)"
	dotnet build Yuki.Blog.sln

##@ Run

run: docker-db docker-seq ## Run the API locally (requires database and Seq)
	@echo "$(BLUE)Stopping any running API instances...$(NC)"
	@-pgrep -f "Yuki.Blog.Api.dll" | xargs -r kill 2>/dev/null || true
	@echo "$(YELLOW)Waiting for services to be ready...$(NC)"
	@sleep 1 2>/dev/null || ping -n 2 127.0.0.1 > nul 2>&1 || true
	@echo "$(BLUE)Starting API...$(NC)"
	@echo "$(YELLOW)API will be available at http://localhost:5000$(NC)"
	@echo "$(YELLOW)Seq logging dashboard at http://localhost:5341$(NC)"
	cd src/Yuki.Blog.Api && dotnet run

##@ Testing

test: ## Run all tests (unit + E2E) with coverage
	@echo "$(BLUE)Running all tests with coverage...$(NC)"
	@dotnet test tests/Yuki.Blog.Domain.UnitTests/Yuki.Blog.Domain.UnitTests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover --verbosity quiet --nologo
	@dotnet test tests/Yuki.Blog.Application.UnitTests/Yuki.Blog.Application.UnitTests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover --verbosity quiet --nologo
	@dotnet test tests/Yuki.Blog.Infrastructure.UnitTests/Yuki.Blog.Infrastructure.UnitTests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover --verbosity quiet --nologo
	@dotnet test tests/Yuki.Blog.Sdk.UnitTests/Yuki.Blog.Sdk.UnitTests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover --verbosity quiet --nologo
	@echo "$(YELLOW)Running E2E tests (with TestContainers)...$(NC)"
	@dotnet test tests/Yuki.Blog.Api.E2ETests/Yuki.Blog.Api.E2ETests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover --verbosity quiet --nologo
	@echo "$(GREEN)All tests completed! Coverage reports generated in test project directories$(NC)"

e2e: ## Run all E2E tests (SpecFlow BDD)
	@echo "$(BLUE)Running all E2E tests with SpecFlow...$(NC)"
	cd tests/Yuki.Blog.Api.E2ETests && dotnet test --logger "console;verbosity=normal"
	@echo "$(GREEN)E2E tests completed!$(NC)"

e2e-smoke: ## Run smoke tests only (critical path)
	@echo "$(BLUE)Running smoke tests...$(NC)"
	cd tests/Yuki.Blog.Api.E2ETests && dotnet test --filter "Category=smoke" --logger "console;verbosity=normal"
	@echo "$(GREEN)Smoke tests completed!$(NC)"

e2e-critical: ## Run all critical tests
	@echo "$(BLUE)Running critical tests...$(NC)"
	cd tests/Yuki.Blog.Api.E2ETests && dotnet test --filter "Category=critical" --logger "console;verbosity=normal"
	@echo "$(GREEN)Critical tests completed!$(NC)"

##@ Docker

docker-up: ## Start all services (database + API)
	@echo "$(BLUE)Starting all services with docker-compose...$(NC)"
	docker compose up -d
	@echo "$(GREEN)Services started. API available at http://localhost:8080$(NC)"

docker-down: ## Stop all services
	@echo "$(BLUE)Stopping all services...$(NC)"
	docker compose down

docker-db: ## Start only the database
	@echo "$(BLUE)Starting database service...$(NC)"
	docker compose up -d postgres
	@echo "$(GREEN)Database started at localhost:5432$(NC)"
	@echo "$(YELLOW)Waiting for database to be ready...$(NC)"
	@sleep 2 2>/dev/null || ping -n 3 127.0.0.1 > nul 2>&1 || true

docker-seq: ## Start only the Seq logging service
	@echo "$(BLUE)Starting Seq logging service...$(NC)"
	docker compose up -d seq
	@echo "$(GREEN)Seq started at http://localhost:5341$(NC)"
	@echo "$(YELLOW)Waiting for Seq to be ready...$(NC)"
	@sleep 2 2>/dev/null || ping -n 3 127.0.0.1 > nul 2>&1 || true

docker-logs: ## View docker-compose logs
	docker compose logs -f

docker-clean: docker-down ## Remove all containers and volumes
	@echo "$(RED)Cleaning up all docker resources...$(NC)"
	docker compose down -v
	@echo "$(YELLOW)Note: This removes the database volume. All data will be lost!$(NC)"

##@ Info

info: ## Show project information
	@echo "$(BLUE)=== Yuki Blog Project Information ===$(NC)"
	@echo "$(GREEN)Solution:$(NC) Yuki.Blog.sln"
	@echo "$(GREEN)Framework:$(NC) .NET 8.0"
	@echo ""
	@echo "$(YELLOW)Source Projects:$(NC)"
	@echo "  - Yuki.Blog.Api"
	@echo "  - Yuki.Blog.Application"
	@echo "  - Yuki.Blog.Domain"
	@echo "  - Yuki.Blog.Infrastructure"
	@echo "  - Yuki.Blog.Sdk"
	@echo ""
	@echo "$(YELLOW)Test Projects:$(NC)"
	@echo "  - Yuki.Blog.Domain.UnitTests"
	@echo "  - Yuki.Blog.Application.UnitTests"
	@echo "  - Yuki.Blog.Infrastructure.UnitTests"
	@echo "  - Yuki.Blog.Sdk.UnitTests"
	@echo "  - Yuki.Blog.Api.E2ETests"
	@echo ""
	@echo "$(YELLOW)Ports:$(NC)"
	@echo "  - API (local): http://localhost:5000/swagger"
	@echo "  - API (docker): http://localhost:8080/swagger"
	@echo "  - PostgreSQL: Host=localhost;Database=yukiblog;Username=yukiuser;Password=yukipass;Port=5432"
	@echo "  - Seq Logging: http://localhost:5341"
	@echo ""
	@echo "$(YELLOW)Key Features:$(NC)"
	@echo "  - Clean Architecture + DDD + CQRS"
	@echo "  - Result Pattern for error handling"
	@echo "  - Strongly-Typed IDs"
	@echo "  - Header-Based API Versioning (X-API-Version: 1.0)"
	@echo "  - Correlation ID for distributed tracing (X-Correlation-ID)"
	@echo "  - Structured Logging with Serilog + Seq"
	@echo "  - Rate Limiting (10/min on POST) + Response Caching"
	@echo "  - Health Checks + TestContainers for E2E tests"
