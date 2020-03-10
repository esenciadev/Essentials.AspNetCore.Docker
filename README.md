# AspNetCore.Configuration.Docker

Docker configuration provider for Asp.net Core projects. At this time, it
has a docker secrets provider only, with a docker config provider in the works.

# How to use

In `Program.cs` call `AddDockerSecrets()` during webhost construction, like so:

```
public static IWebHostBuilder CreateWebHostBuilder(string[] args)
{
    WebHost.CreateDefaultBuilder(args)
           .AddDockerSecrets(false)
           .UseStartup<Startup>();
}   
```

If you only want to use Docker secrets in production, so that you can debug locally, call `.AddDockerSecretsExceptInDevelopment()` instead. 
