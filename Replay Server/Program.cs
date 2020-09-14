using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Replay_Server
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
                    webBuilder
                    .UseUrls("http://0.0.0.0:3030")
                    .UseStartup<Startup>();
                });
    }
}
