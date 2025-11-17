using Kartverket.Aspire.AppHost.MariaDb;

var builder = DistributedApplication.CreateBuilder(args);


var mariaDbServer = builder.AddMariaDb("mariadb")
                   .WithEnvironment("MYSQL_ROOT_PASSWORD", "kart")
                   .WithEnvironment("MYSQL_USER", "kartverket_user")
                   .WithEnvironment("MYSQL_PASSWORD", "KartverketBruker1234")
                   .WithEnvironment("MYSQL_DATABASE", "kartverketdb")
                   .WithLifetime(ContainerLifetime.Session)
                   // ❗ 1. Legg til persistent volume (super viktig)
                     .WithVolume("mariadb_data", "/var/lib/mysql")

                    // ❗ 2. Endre Lifetime slik at containeren ikke slettes på stopp
                    .WithLifetime(ContainerLifetime.Persistent);





var mariaDb = mariaDbServer.AddDatabase("kartverketdb");


//Variant dockerfile
builder.AddDockerfile("kartverket-web", "../../", "Kartverket.Web/Dockerfile")
                       .WithExternalHttpEndpoints()
                       .WithReference(mariaDb)
                       .WaitFor(mariaDb)
                        .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
                       .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "kartverket-web");
                        

builder.Build().Run();
