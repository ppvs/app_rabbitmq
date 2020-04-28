using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.LayoutRenderers;
using NLog.Layouts;
using NLog.Web;

namespace service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
            LogManager.Flush();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => {
                    builder.SetBasePath(Path.Combine(Environment.CurrentDirectory, "config"))
                        .AddJsonFile("appsettings.json")
                        .AddEnvironmentVariables();
                })
                .UseNLog()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .UseUrls("http://0.0.0.0:5005")
                .UseStartup<Startup>();
    }
}
