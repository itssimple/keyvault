using KeyVault.DatabaseScripts;
using KeyVault.DataLayer;
using KeyVault.Services;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace KeyVault
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(x => new Database("Data source=storage.db"));
            services.AddScoped<StorageLayer>();

            services.AddSingleton<ClientCertificateValidationService>();

            services.AddCertificateForwarding(options =>
            {
                options.CertificateHeader = "X-SSL-CERT";
                options.HeaderConverter = (headerValue) =>
                {
                    X509Certificate2 clientCertificate = null;

                    if (!string.IsNullOrWhiteSpace(headerValue))
                    {
                        byte[] bytes = StringToByteArray(WebUtility.UrlDecode(headerValue));
                        clientCertificate = new X509Certificate2(bytes);
                    }

                    return clientCertificate;
                };
            });

            services
                .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate(options =>
                {
                    options.AllowedCertificateTypes = CertificateTypes.All;
                    options.Events = new CertificateAuthenticationEvents
                    {
                        OnAuthenticationFailed = x =>
                        {
                            return Task.CompletedTask;
                        },
                        OnCertificateValidated = context =>
                        {
                            var validationService =
                                context.HttpContext.RequestServices
                                    .GetService<ClientCertificateValidationService>();

                            if (string.IsNullOrWhiteSpace(context.Request.Query["clientId"]))
                            {
                                context.Fail("ClientId missing from request (query parameter)");
                            }
                            else
                            {
                                if (validationService.ValidateCertificate(context.Request.Query["clientId"], context.ClientCertificate))
                                {
                                    var claims = new[]
                                    {
                                    new Claim(
                                        ClaimTypes.NameIdentifier,
                                        context.ClientCertificate.Subject,
                                        ClaimValueTypes.String,
                                        context.Options.ClaimsIssuer),
                                    new Claim(
                                        ClaimTypes.Name,
                                        context.ClientCertificate.Subject,
                                        ClaimValueTypes.String,
                                        context.Options.ClaimsIssuer)
                                };

                                    context.Principal = new ClaimsPrincipal(
                                        new ClaimsIdentity(claims, context.Scheme.Name));
                                    context.Success();
                                }
                                else
                                {
                                    context.Fail("Invalid certificate");
                                }
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All,
                RequireHeaderSymmetry = false,
                ForwardLimit = null
            });

            app.UseCertificateForwarding();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static byte[] StringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];

            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }
    }
}
