using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;

namespace KeyVault
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
                    webBuilder.UseStartup<Startup>()
                    .ConfigureKestrel(options =>
                    {
                        options.ConfigureHttpsDefaults(o =>
                        {
                            o.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
                            o.AllowAnyClientCertificate();
                        });
                    })
                    .UseUrls("http://localhost:51337");
                });
    }
}
