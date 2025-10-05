# NRL Object registration

ASP.Net + Razor + Aspire

## Liberaries
- Microsoft.AspNetCore.Mvc
- Microsoft.Extensions.Logging
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Diagnostics.HealthChecks
- Microsoft.Extensions.ServiceDiscovery
- OpenTelemetry, OpenTelemetry.Metrics, OpenTelemetry.Trace
- NSubstitute
- Leaflet

## The system follows the MVC pattern:
- Controller (ObstacleController): Handles GET/POST requests from the user.
- Model (ObstacleData): Stores obstacle information and validation rules.
- View (DataForm + Overview): The user fills out a form with name, height, and location.
- The map (Leaflet) allows selecting coordinates, which are shown on a second page.

## Overview of structure
This web-application uses a MVC srtucutre and docker
```
Kartverket.Web/
├── Controllers/
├── Models/
├── Views/
│   ├── Shared/
│   ├── Obstacle/
│   ├── Map/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/
├── Program.cs
└── README.md
```
## Usage
Docker must be  running for application to work

Start, manage and terminate application in Aspire
