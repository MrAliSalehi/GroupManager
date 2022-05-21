using GroupManager.Application.Contracts;
using GroupManager.Application.Services;
using GroupManager.Common.Attributes;
using GroupManager.Common.Models;
using GroupManager.DataLayer.Context;
using Hangfire;
using Hangfire.Logging.LogProviders;
using Hangfire.Storage.SQLite;
using Telegram.Bot;

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
    using var db = new ManagerContext();
    var created = db.Database.EnsureCreated();
    Log.Information("Db Created:{x}", created);

    services.AddSingleton<ITelegramBotClient, TelegramBotClient>(_ => new TelegramBotClient(Globals.BotConfigs.Token));

    Globals.Configuration = context.Configuration;
    Globals.ServiceProvider = services.BuildServiceProvider();
    GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 3 });
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
    services.AddHostedService<AntiFloodService>();
    services.AddHostedService<MediaLimitService>();
    services.AddHostedService<LanguageService>();
    services.AddHostedService<AntiLinkIdTagService>();
});

var describers = typeof(IDescriber)
    .GetImplementedClasses()
    .GetDescribeAttribute<DescriberAttribute>()
    .Map();

Globals.Describers.AddRange(describers);


host.InjectSerilog();

await host.Build().RunAsync();