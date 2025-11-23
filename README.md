# Kartverket.web

ASP.NET + Razor + Aspire + Tailwind

## Usage

Docker desktop must be running

Run the file Kartverket.Aspire.AppHost

## Structure

The project is organized as follows:

- **Kartverket.Web**  
  - `Controllers/`  
    Contains controller classes for handling HTTP requests (e.g., `AuthController.cs` for authentication).
  - `Data/`  
    Entity Framework Core database context and configuration (e.g., `KartverketDbContext.cs`).
  - `Models/`  
    Data models and view models representing entities such as User, Report, Role, Organization, etc.
  - `Views/`  
    Razor Pages and shared layouts for the UI.
    - `Shared/`  
      Common layout files (e.g., `_Layout.cshtml`).
    - `AdminPart/`  
      Views for admin features (e.g., `User-management.cshtml`).
    - Other folders for specific features (e.g., authentication, reporting).
  - `wwwroot/`  
    Static files such as CSS, JavaScript, and images.
  - `Kartverket.Web.csproj`  
    Project file with dependencies and build configuration.

- **Kartverket.Aspire.AppHost**  
  - Used for running the application with Aspire and Docker support.

  ### Folder overview
    ```
    Kartverket.Aspire/
    ├── Kartverket.Aspire.AppHost/
    ├── Kartverket.Aspire.ServiceDefaults/
    Kartverket.Web/
    ├── Controllers/
    ├── Data/
    ├── Migrations/
    ├── Models/
    |   ├── Entities/
    ├── Services/
    ├── Views/
    |   ├── AdminPart/
    |   ├── Auth/
    |   ├── Home/
    |   ├── Obstacle/
    |   ├── Registrar/
    |   ├── Report/
    |   ├── Shared/
    ├── wwwroot/
    ├── Dockerfile
    ├── Program.cs
    Kartverket.Web.UnitTests/
    ├── Controllers/
    ```

## Libraries used
The project uses the following libraries that you may need to install:

### NuGet Packages
- Aspire.MySqlConnector
- Aspire.Pomelo.EntityFrameworkCore.MySql  
- Pomelo.EntityFrameworkCore.MySql  
- Microsoft.EntityFrameworkCore.Design  
- Microsoft.EntityFrameworkCore.Tools  
- Microsoft.VisualStudio.Azure.Containers.Tools.Targets
- Moq

### Client-side Libraries
- Tailwind CSS  
- jQuery  
- jQuery Validation  
- jQuery Validation Unobtrusive  
- Bootstrap  
- Leaflet  
- Leaflet.Draw  
- Leaflet.Draw  

## Unit Tests
The project includes a dedicated test project (Kartverket.Web.UnitTests) used to verify the logic of all main controllers.

# Test Status (all tests passing)

- AuthControllerUnitTests ✅

- ReportControllerUnitTests ✅

- HomeControllerUnitTests ✅

- ObstacleControllerUnitTests ✅

- AdminPartControllerUnitTests ✅

- RegistrarControllerUnitTests ✅

# How to run the tests
- dotnet test
