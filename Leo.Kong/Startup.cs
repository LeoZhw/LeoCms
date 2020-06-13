using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Kong;
using Kong.Extensions;
using Kong.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Leo.Kong
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<KongClient>(fat =>
            {
                var options = new KongClientOptions(HttpClientFactory.Create(), this.Configuration["kong:host"]);
                var client = new KongClient(options);
                return client;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, KongClient kongClient)
        {
            UseKong(app, kongClient);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }

        public void UseKong(IApplicationBuilder app, KongClient kongClient)
        {
            var upStream = Configuration.GetSection("kong:upstream").Get<UpStream>();
            var target = Configuration.GetSection("kong:target").Get<TargetInfo>();
            var uri = new Uri("http://localhost:5000");
            target.Target = uri.Authority;
            app.UseKong(kongClient, upStream, target);
        }
    }
}
