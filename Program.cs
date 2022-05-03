using GroupManager.Application.Services;
using GroupManager.Common.Extensions;
using GroupManager.Common.Models;

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
    services.AddHostedService<UpdateService>();
});

host.InjectSerilog();

await host.Build().RunAsync();