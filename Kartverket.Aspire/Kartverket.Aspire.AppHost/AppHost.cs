using Kartverket.Aspire.AppHost.MariaDb;

var builder = DistributedApplication.CreateBuilder(args);


var mariaDbServer = builder.AddMariaDb("mariadb")
                   .WithEnvironment("MYSQL_ROOT_PASSWORD", "kart")
                   .WithEnvironment("MYSQL_USER", "kartverket_user")
                   .WithEnvironment("MYSQL_PASSWORD", "KartverketBruker1234")
                   .WithEnvironment("MYSQL_DATABASE", "kartverketdb")
                   .WithLifetime(ContainerLifetime.Session);


var mariaDb = mariaDbServer.AddDatabase("kartverketdb");


//Variant dockerfile
builder.AddDockerfile("kartverket-web", "../../", "Kartverket.Web/Dockerfile")
                       .WithExternalHttpEndpoints()
                       .WithReference(mariaDb)
                       .WaitFor(mariaDb)
                       .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "kartverket-web");

builder.Build().Run();
