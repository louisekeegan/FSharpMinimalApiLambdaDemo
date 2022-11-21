namespace App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Serilog

type Startup private () =
    let handleGet logInfo =
        logInfo $"Request received"
        [|"value1"; "value2"|]
 
    let handleCreate (payload: Request.Payload) logInfo =
        logInfo $"Payload received: {payload}"
        ()
            
    new (configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration
                  
    member this.GetLogger =
        let loggerConfiguration = LoggerConfiguration()
        let logger = loggerConfiguration.WriteTo.Console().CreateLogger()
        logger
        
    // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices(services: IServiceCollection) =
        // To add AWS services to the ASP.NET Core dependency injection add
        // the AWSSDK.Extensions.NETCore.Setup NuGet package. Then
        // use the "AddAWSService" method to add AWS service clients.
        // services.AddAWSService<Amazon.S3.IAmazonS3>() |> ignore

        // Add framework services.
        services.AddControllers() |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        let logger = this.GetLogger
        let logInfo = logger.Information
        
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore

        app.UseHttpsRedirection() |> ignore
        app.UseRouting() |> ignore

        app.UseAuthorization() |> ignore

        app.UseEndpoints(fun endpoints -> 
            endpoints.MapPost("api/values", Action<Request.Payload>(fun payload -> handleCreate payload logInfo)) |> ignore
            endpoints.MapGet("api/values", Func<string[]>(fun () -> handleGet logInfo)) |> ignore
                ) |> ignore

    member val Configuration : IConfiguration = null with get, set