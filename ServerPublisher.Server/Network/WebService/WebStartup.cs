using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using ServerPublisher.Server.Network.PublisherClient;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ServerPublisher.Server.Network.WebService
{
    public class WebStartup
    {
        private readonly IConfiguration configuration;

        public WebStartup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            PublisherNetworkServer.Instance.Load();

            services.AddSingleton(x => StaticInstances.ServiceManager);
            services.AddSingleton(x => StaticInstances.SessionManager);
            services.AddSingleton(x => StaticInstances.PatchManager);
            services.AddSingleton(x => StaticInstances.ProjectsManager);
            services.AddSingleton(x => StaticInstances.Server);
            services.AddSingleton(x => StaticInstances.ServerLogger);
            services.AddSingleton(x => StaticInstances.ServerConfiguration);
            services.AddSingleton(x => StaticInstances.ExplorerManager);

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(c =>
                {
                    c.LoginPath = "/Login";
                });

            services.AddRazorPages(c =>
            {
                c.RootDirectory = "/Network/WebService/Pages";
            })
            .AddRazorRuntimeCompilation(c => {
                c.FileProviders.Add(new EmbeddedFileProvider(Assembly.GetExecutingAssembly()));

                c.AdditionalReferencePaths.Add("/Network/WebService");
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }

        public static void Run() =>
            CreateWebHostBuilder(Environment.GetCommandLineArgs())
            .Build()
            .Run();

        private static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<WebStartup>();
                });
    }
}
