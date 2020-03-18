using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CLUZWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                            {
                                serverOptions.Listen(System.Net.IPAddress.Any, 8000, listenOptions =>
                                {
                                    listenOptions.UseHttps("pfx.pfx", "password");
                                    listenOptions.UseConnectionLogging();
                                });
                            });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
