var builder = DistributedApplication.CreateBuilder(args);

// All resources are added here in the AppHost, and then referenced by the API project. This is a key pattern in Aspire, where the AppHost is the main composition root for all your services and dependencies.
var sql = builder.AddSqlServer("SqlServer")
                 .AddDatabase("MusicDb");

// var cosmos = builder.AddAzureCosmosDB("Cosmos")
//                     .RunAsEmulator(emulator =>
//                     {
//                            // Access the underlying container resource of the emulator
//                            emulator.WithLifetime(ContainerLifetime.Persistent);
//                     })
//                     .AddDatabase("MusicDb");

// var appConfig = builder.AddAzureAppConfiguration("AppConfig");

var storage = builder.AddAzureStorage("Storage").RunAsEmulator(container =>
                     {
                            container.WithImageTag("latest"); ;
                     })
                     .AddBlobs("Blobs");

builder.AddProject<Projects.MusicApp_Api>("MusicApp-Api")
       .WithReference(sql)
       .WithReference(storage)
       // .WithReference(cosmos)
       // .WithReference(appConfig)
       .WaitFor(sql)
       .WaitFor(storage);

builder.Build().Run();