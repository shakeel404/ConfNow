using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrontEnd.Filters;
using FrontEnd.HealthCheck;
using FrontEnd.Models;
using FrontEnd.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FrontEnd
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
            services.AddTransient<RequireLoginFilter>();
            services.AddSingleton<IAdminService, AdminService>();

            services.AddRazorPages();
            services.AddMvc(opt => 
            { 
                opt.EnableEndpointRouting = false;
                opt.Filters.AddService<RequireLoginFilter>();
            })
                .AddRazorPagesOptions(options =>
            {
                options.Conventions.AuthorizeFolder("/Admin", "Admin");
            });
            services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new Uri(Configuration["ServiceUrl"]);
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                {
                    policy.RequireAuthenticatedUser()
                          .RequireIsAdminClaim();
                });
            });

             
            services.AddHealthChecks()
        .AddCheck<BackendHealthCheck>("backend")
       .AddDbContextCheck<IdentityDbContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

          //  app.UseAuthorization();
            app.UseAuthentication();

            app.UseMvc();
            app.UseHealthChecks("/health");
        }
    }
}
