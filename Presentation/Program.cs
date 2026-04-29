using Infrastructure;
using Serilog;
using Serilog.Sinks.MSSqlServer;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with access to configuration
builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

    loggerConfig
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/health-insurance-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
        .WriteTo.MSSqlServer(
            connectionString: connectionString!,
            sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
            {
                TableName = "Logs",
                AutoCreateSqlTable = true,
                BatchPostingLimit = 50,
                BatchPeriod = TimeSpan.FromSeconds(5)
            },
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information);
});

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

