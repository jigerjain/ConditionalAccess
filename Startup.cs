using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Okta.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace ConditionalAccess
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
            services.AddRazorPages(opts =>
            {
                // we don't care about antiforgery in the demo
                opts.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
            }).AddNewtonsoftJson(); // the FIDO2 library requires Json.NET

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            })
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOktaMvc(new OktaMvcOptions
            {
               // Replace these values with your Okta configuration
               OktaDomain = Configuration.GetValue<string>("Okta:OktaDomain"),
               AuthorizationServerId = Configuration.GetValue<string>("Okta:AuthorizationServerId"),
               ClientId = Configuration.GetValue<string>("Okta:ClientId"),
               ClientSecret = Configuration.GetValue<string>("Okta:ClientSecret"),
               Scope = new List<string> { "openid", "profile", "email" },
            });

            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(2);
                options.Cookie.HttpOnly = true;
                // Strict SameSite mode is required because the default mode used
                // by ASP.NET Core 3 isn't understood by the Conformance Tool
                // and breaks conformance testing
                options.Cookie.SameSite = SameSiteMode.Unspecified;
            });
            services.AddFido2(options =>
            {
                options.ServerDomain = Configuration["fido2:serverDomain"];
                options.ServerName = "CCDashboard";
                options.Origin = Configuration["fido2:origin"];
                options.TimestampDriftTolerance = Configuration.GetValue<int>("fido2:timestampDriftTolerance");
                options.MDSAccessKey = Configuration["fido2:MDSAccessKey"];
                options.MDSCacheDirPath = Configuration["fido2:MDSCacheDirPath"];
            })
            .AddCachedMetadataService(config =>
            {
            //They'll be used in a "first match wins" way in the order registered
             if (!string.IsNullOrWhiteSpace(Configuration["fido2:MDSAccessKey"]))
                {
                    config.AddFidoMetadataRepository(Configuration["fido2:MDSAccessKey"]);
                }
                config.AddStaticMetadataRepository();
            });
            services.AddControllersWithViews();
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseSession();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
