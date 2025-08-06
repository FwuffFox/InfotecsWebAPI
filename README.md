# InfotecsWebAPI

A .NET 9 Web API for processing CSV timescale data with PostgreSQL storage and OpenTelemetry observability.
Made as an appliance for the Infotecs internship program.

- [Project Structure](#project-structure)
- [Quick Start](#quick-start)
- [Development](#development)
- [Testing](#testing)
- [Configuration](#configuration)

## Project Structure

```
InfotecsWebAPI/
├── compose.yaml                    # Docker Compose configuration
├── InfotecsWebAPI.sln             # Solution file
├── InfotecsWebAPI/                 # Main Web API project
│   ├── Controllers/                # API controllers
│   │   ├── CsvController.cs        # CSV file upload and processing
│   │   ├── ResultsController.cs    # Query processed results
│   │   └── ValuesController.cs     # Raw value data endpoints
│   ├── Data/
│   │   └── TimescaleDbContext.cs   # Entity Framework DbContext
│   ├── Models/                     # Data models and DTOs
│   │   ├── ValueDto.cs             # CSV data transfer object
│   │   ├── ValueEntity.cs          # Database entity for values
│   │   └── ResultEntity.cs         # Database entity for results
│   ├── Services/                   # Business logic services
│   │   ├── CsvProcessingService.cs # CSV file processing logic
│   │   ├── ResultService.cs        # Results data service
│   │   └── ValueService.cs         # Values data service
│   ├── Mappers/                    # Object mapping utilities
│   ├── Migrations/                 # Entity Framework migrations
│   └── Program.cs                  # Application entry point
├── InfotecsWebAPI.Tests/           # Unit tests
│   ├── Controllers/                # Controller tests
│   └── Services/                   # Service tests
└── test_datasets/                  # Sample CSV files for testing
```

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/FwuffFox/InfotecsWebAPI
cd InfotecsWebAPI
```

### 2. Start the Application

```bash
# Start all services with Docker Compose
docker-compose up -d

# Or build and start
docker-compose up --build
```

### 3. Access the Application

| Service               | URL                                   | Description                                              |
|-----------------------|---------------------------------------|----------------------------------------------------------|
| **Swagger**           | http://localhost:8080                 | Main API endpoints                                       |
| **pgAdmin**           | http://localhost:5050                 | Database administration                                  |
| **Aspire Dashboard**  | http://localhost:18888                | OpenTelemetry dashboard (Access token in container logs) |

##  Development

### Local Development Setup

```bash
# Restore dependencies
dotnet restore

# Start PostgreSQL only
docker-compose up db -d

# Update connection string in appsettings.Development.json
# Run the application locally
dotnet run --project InfotecsWebAPI

# Or with hot reload
dotnet watch --project InfotecsWebAPI
```

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add <MigrationName> --project InfotecsWebAPI

# Update database
dotnet ef database update --project InfotecsWebAPI
```

## Testing

Most of important functionality is covered by unit tests.

### Run All Tests

```bash
dotnet test
```

### Test Data

Sample CSV files are available in the `test_datasets/` directory:

- `valid.csv` - Valid data set
- `valid_10.csv` - Valid data with 10 records
- `invalid_type.csv` - Invalid data types
- `missing_value.csv` - Missing values
- `future_date.csv` - Future dates
- `too_old_date.csv` - Too old dates
- `negative_value.csv` - Negative values
- `negative_execution.csv` - Negative execution times

## Configuration
Check the `compose.yaml` file for service configurations, including environment variables and ports.
