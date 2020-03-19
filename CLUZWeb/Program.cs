using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;


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
                //.ConfigureLogging(logging =>
                //{
                //    logging.ClearProviders();
                //    logging.AddConsole();
                //})
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //webBuilder.ConfigureKestrel(serverOptions =>
                    //        {
                    //            serverOptions.Listen(System.Net.IPAddress.Any, 8000, listenOptions =>
                    //            {
                    //                listenOptions.UseHttps("pfx.pfx", "password");
                    //                listenOptions.UseConnectionLogging();
                    //            });
                    //        });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
