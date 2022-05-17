using GroupManager.Application.Services;
using GroupManager.Common.Models;
using Hangfire;
using Hangfire.Logging.LogProviders;
using Hangfire.Storage.SQLite;

var host = Host.CreateDefaultBuilder(args);


host.ConfigureAppConfiguration((context, builder) =>
{
    builder
   .SetBasePath(context.HostingEnvironment.ContentRootPath)
   .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", false, true)
   .Build();

});

host.ConfigureServices((context, services) =>
{
    var config = context.Configuration.GetSection("BotConfigs");
    services.Configure<BotConfigs>(config);

    context.Configuration.Bind(config.Key, Globals.BotConfigs);

    Globals.Configuration = context.Configuration;

    services.AddHangfire(conf =>
    {
        var connString = Globals.ConnectionString();
        conf.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseDefaultTypeSerializer()
            .UseSQLiteStorage(connString)
            .UseColouredConsoleLogProvider()
            .UseLogProvider(new SerilogLogProvider());
    });
    services.AddHangfireServer();

    services.AddHostedService<UpdateService>();

});


host.InjectSerilog();

await host.Build().RunAsync();