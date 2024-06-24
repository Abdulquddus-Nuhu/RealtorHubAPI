
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog.Events;
using Serilog;
using System.Data;
using System.Text;
using System;
using RealtorHubAPI.Data;
using RealtorHubAPI.Entities.Identity;
using System.Threading.RateLimiting;
using RealtorHubAPI.Services;
using RealtorHubAPI.Middlewares;
using RealtorHubAPI.SeedDatabase;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Npgsql;
using RealtorHubAPI.Configurations;
using RealtorHubAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http.Features;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
Log.Information($"Starting up RealTor Hub API Web Server!");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    bool InDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    if (InDocker)
    {
        builder.WebHost.UseUrls("http://*:4100");  // Use port 4100 for Docker
    }
    else if (builder.Environment.IsProduction())
    {
        builder.WebHost.UseUrls("http://localhost:4002");
    }
    else if (builder.Environment.IsStaging())
    {
        builder.WebHost.UseUrls("http://localhost:4001");
    }



    builder.Logging.ClearProviders();

    if (builder.Environment.IsDevelopment())
    {
        Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Debug()
           .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
           .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
           .Enrich.FromLogContext()
           .WriteTo.Console()
           //.WriteTo.File(outputTemplate:"", formatter: "Serilog.Formatting.Json.JsonFormatter, Serilog")
           .CreateLogger();
    }
    else if (builder.Environment.IsStaging())
    {
        Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Information()
           .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
           .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
           .Enrich.FromLogContext()
           .WriteTo.Console()
           .WriteTo.File(
                @"/logs/RealtorHubAPIStaging/logs.txt",
                fileSizeLimitBytes: 10485760,
                rollOnFileSizeLimit: true,
                shared: true,
                retainedFileCountLimit: null,
                flushToDiskInterval: TimeSpan.FromSeconds(1))
           //.WriteTo.ApplicationInsights(TelemetryConfiguration.CreateDefault(), TelemetryConverter.Traces)
           .CreateLogger();
    }
    else
    {
        Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Information()
           .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
           .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
           .Enrich.FromLogContext()
           .WriteTo.Console()
           .WriteTo.File(
                @"/logs/RealtorHubProduction/logs.txt",
                fileSizeLimitBytes: 10485760,
                rollOnFileSizeLimit: true,
                shared: true,
                retainedFileCountLimit: null,
                flushToDiskInterval: TimeSpan.FromSeconds(1))
        //.WriteTo.ApplicationInsights(TelemetryConfiguration.CreateDefault(), TelemetryConverter.Traces)
           .CreateLogger();
    }

    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        //options.UseNpgsql(connectionString, b => b.MigrationsAssembly("Infrastructure"));
        options.UseNpgsql(connectionString);
    });


    builder.Services.AddIdentity<User, Role>(
               options =>
               {
                   options.Password.RequireDigit = true;
                   options.Password.RequireNonAlphanumeric = true;
                   options.Password.RequireLowercase = true;
                   options.Password.RequireUppercase = true;
                   options.Password.RequiredLength = 8;
                   options.User.RequireUniqueEmail = true;
                   options.SignIn.RequireConfirmedEmail = true;
               })
               .AddEntityFrameworkStores<AppDbContext>()
               .AddDefaultTokenProviders();

    builder.Services.Configure<CloudinaryOptions>(builder.Configuration.GetSection("Cloudinary"));
    builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("Minio"));
    builder.Services.Configure<CoinbaseOptions>(builder.Configuration.GetSection("Coinbase"));
   

    builder.Services.AddScoped<TokenService>();
    builder.Services.AddScoped<OtpGenerator>();
    builder.Services.AddScoped<CloudinaryService>();
    builder.Services.AddScoped<MinioService>();
    builder.Services.AddHostedService<QueuedHostedService>();
    builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

    // Register the worker responsible of seeding the database.
    builder.Services.AddHostedService<SeedDb>();


    var key = Encoding.ASCII.GetBytes(builder.Configuration["JWT_SECRET_KEY"]);
    var tokenValidationParams = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.Zero
    };

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(config =>
    {
        config.RequireHttpsMetadata = false;
        config.SaveToken = true;
        config.TokenValidationParameters = tokenValidationParams;
    });
    //builder.Services.AddAuthorization(options =>
    //{
    //    options.AddPolicy(AuthConstants.Policies.ADMINS, policy => policy.RequireRole(AuthConstants.Roles.ADMIN, AuthConstants.Roles.SUPER_ADMIN));
    //});

    builder.Services.AddAuthorization();

    //Ensure all controllers use jwt token
    //builder.Services.AddControllers(options =>
    //{
    //    var policy = new AuthorizationPolicyBuilder()
    //        .RequireAuthenticatedUser()
    //        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
    //        .Build();
    //    options.Filters.Add(new AuthorizeFilter(policy));
    //});

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: "MyAllowSpecificOrigins",
                          builder =>
                          {
                              builder
                              .WithOrigins("https://localhost:3000", "http://localhost:3000")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                          });
    });


    //Swagger Authentication/Authorization
    builder.Services.AddSwaggerGen(c =>
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme. **Enter Bearer Token Only**",
            Reference = new OpenApiReference
            {
                Id = "Bearer",
                Type = ReferenceType.SecurityScheme
            }
        };

        c.EnableAnnotations();
        c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { securityScheme, Array.Empty<string>() }
        });
    });

    // Security and Production enhancements 
    if (!builder.Environment.IsDevelopment())
    {
        // Proxy Server Config
        builder.Services.Configure<ForwardedHeadersOptions>(
              options =>
              {
                  options.ForwardedHeaders =
                      ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
              });

        //persist keys to database
        builder.Services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();


        //Persist key
        //builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo("/var/keys"));
    }

    //Remove Server Header
    builder.WebHost.UseKestrel(options => options.AddServerHeader = false);


    builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4; // Maximum requests allowed
        options.Window = TimeSpan.FromSeconds(12); // Time window duration
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2; // Maximum queued requests
    }));

    builder.Services.AddResponseCaching();
    builder.Services.AddHttpClient();
    builder.Services.AddHttpClient("CloudinaryClient", client =>
    {
        client.BaseAddress = new Uri("https://api.cloudinary.com/v1_1/");
        client.Timeout = TimeSpan.FromMinutes(10);
    });


    //builder.Services.AddOpenTelemetry().WithTracing(b =>
    //{
    //    b
    //    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Environment.EnvironmentName))
    //    .AddAspNetCoreInstrumentation()
    //    .AddOtlpExporter(opt => { opt.Endpoint = new Uri("http://localhost:4317"); });
    //});

    // add prometheus exporter
    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing =>
        {
            tracing
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                //.AddRedisInstrumentation()
                .AddNpgsql()
                .AddOtlpExporter(opts =>
                {
                    opts.Endpoint = new Uri("http://otel-collector:4317");  // This should match the OTLP endpoint exposed by your collector
                });

            //tracing.AddOtlpExporter();
        })
        .WithMetrics(opt =>

            opt
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("OpenRemoteManage.GatewayAPI"))
                .AddMeter(builder.Configuration.GetValue<string>("OpenRemoteManageMeterName"))
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddMeter("System.Net.Http")
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddOtlpExporter(opts =>
                {
                    opts.Endpoint = new Uri(builder.Configuration["Otel:Endpoint"]);
                })
        );

    builder.Logging.AddOpenTelemetry(option =>
    {
        option.AddOtlpExporter();
    });


    var app = builder.Build();

    //Configure the HTTP request pipeline.
    if (!app.Environment.IsProduction())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRateLimiter();
    app.UseResponseCaching();

    //security
    //app.UseMiddleware<UserAgentValidationMiddleware>();
    //app.UseMiddleware<SecurityHeadersMiddleware>();
    //app.UseMiddleware<NotFoundRequestTrackingMiddleware>();


    app.UseHsts();
    app.UseHttpsRedirection();

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    app.UseCors("MyAllowSpecificOrigins");

    app.UseAuthentication();
    app.UseAuthorization();



    app.MapControllers();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occurred during bootstrapping the Server!");
}
finally
{
    Log.CloseAndFlush();
}

