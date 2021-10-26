﻿using Serilog;

public static class ProgramExtensions
{
    public static void AddCustomSerilog(this WebApplicationBuilder builder)
    {
        var seqServerUrl = builder.Configuration["SeqServerUrl"];

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.Console()
            .WriteTo.Seq(seqServerUrl)
            .Enrich.WithProperty("ApplicationName", "Identity.API")
            .CreateLogger();
    }

    public static void AddCustomMvc(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllersWithViews();
    }

    public static void AddCustomDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(builder.Configuration["SqlConnectionString"]));
    }

    public static void AddCustomIdentity(this WebApplicationBuilder builder)
    {
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
    }

    public static void AddCustomIdentityServer(this WebApplicationBuilder builder)
    {
        var identityServerBuilder = builder.Services.AddIdentityServer(options =>
        {
            options.IssuerUri = "null";
            options.Authentication.CookieLifetime = TimeSpan.FromHours(2);

            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;
        })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryClients(Config.GetClients(builder.Configuration))
                .AddAspNetIdentity<ApplicationUser>();

        // not recommended for production - you need to store your key material somewhere secure
        identityServerBuilder.AddDeveloperSigningCredential();
    }

    public static void AddCustomAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication();
    }

    public static void AddCustomHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddSqlServer(builder.Configuration["SqlConnectionString"],
                    name: "IdentityDB-check",
                    tags: new string[] { "IdentityDB" });
    }

    public static void AddCustomApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IProfileService, ProfileService>();
    }
}