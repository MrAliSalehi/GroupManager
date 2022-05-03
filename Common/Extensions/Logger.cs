using Serilog;

namespace GroupManager.Common.Extensions
{
    public static class Logger
    {
        public static IHostBuilder InjectSerilog(this IHostBuilder builder)
        {
            if (!File.Exists("logs.txt"))
                File.Create("logs.txt").Close();

            builder.UseSerilog((_, lc) =>
            {
                lc.WriteTo.File(new FileInfo("logs.txt").FullName);
                lc.WriteTo.Console();
            });
            return builder;
        }
    }
}