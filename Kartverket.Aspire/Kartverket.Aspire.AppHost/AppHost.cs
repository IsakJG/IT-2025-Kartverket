using Kartverket.Aspire.AppHost.MariaDb;

// Initializes the Aspire distributed application builder.
var builder = DistributedApplication.CreateBuilder(args);

// Adds the MariaDB container resource to the application model.
var mariaDbServer = builder.AddMariaDb("mariadb")
                   // Essential environment variables for MariaDB setup:
                   .WithEnvironment("MYSQL_ROOT_PASSWORD", "kart")
                   .WithEnvironment("MYSQL_USER", "kartverket_user")
                   .WithEnvironment("MYSQL_PASSWORD", "KartverketBruker1234")
                   .WithEnvironment("MYSQL_DATABASE", "kartverketdb")
                   
                   // Configures a volume to ensure data persists across container restarts.
                   .WithVolume("mariadb_data", "/var/lib/mysql")
                   
                   // Sets the container lifecycle to Persistent, ensuring the container
                   // instance and its data volume are not removed when the application stops.
                   .WithLifetime(ContainerLifetime.Persistent); 

// Registers the specific database instance for use by other application components.
var mariaDb = mariaDbServer.AddDatabase("kartverketdb");


// Adds the Kartverket.Web application, built from a Dockerfile.
builder.AddDockerfile("kartverket-web", "../../", "Kartverket.Web/Dockerfile")
                       // Exposes all configured HTTP endpoints externally.
                       .WithExternalHttpEndpoints()
                       // Establishes a connection reference to the MariaDB database.
                       .WithReference(mariaDb)
                       // Ensures the web application waits for the database to be ready before starting.
                       .WaitFor(mariaDb)
                       // Sets the ASP.NET Core hosting environment for the web application.
                       .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
                       // Configures the HTTP endpoint name, port, and target port.
                       .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "kartverket-web");
                        

// Builds the distributed application model and runs the configured services.
builder.Build().Run();