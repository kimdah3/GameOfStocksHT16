using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Facebook;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GameOfStocksHT16.Data;
using GameOfStocksHT16.Entities;
using GameOfStocksHT16.Services;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;

namespace GameOfStocksHT16
{
    public class Startup
    {
        private Timer _downloadStocksTimer;
        private Timer _completeStockTransTimer;
        private Timer _saveUsersTotalWorthPerDay;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

        }

        public static IConfigurationRoot Configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.Password = new PasswordOptions
            {
                RequireDigit = true,
                RequiredLength = 6,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = false
            }).AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddScoped<IStockService, StockService>();
            services.AddSingleton<IGameOfStocksRepository, GameOfStocksRepository>();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IStockService stockService)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            var sslPort = 0;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();

                var builder = new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile(@"Properties/launchSettings.json", optional: false, reloadOnChange: true);
                var launchConfig = builder.Build();
                sslPort = launchConfig.GetValue<int>("iisSettings:iisExpress:sslPort");

            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            var supportedCultures = new List<CultureInfo>()
            {
                new CultureInfo("sv-SE") {NumberFormat = {CurrencySymbol = "kr"}}
            };
            
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("sv-SE"),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });

            app.UseStaticFiles();

            app.UseIdentity();

            _downloadStocksTimer = new Timer(stockService.SaveStocksOnStartup, null, 20 * 1000, 60 * 2 * 1000);
            _completeStockTransTimer = new Timer(stockService.CompleteStockTransactions, null, 30 * 1000, /*Timeout.Infinite*/30 * 1000);
            _saveUsersTotalWorthPerDay = new Timer(stockService.SaveUsersTotalEveryDay, null, GetMillisecondsToMidnight(), TimeSpan.FromDays(1).Milliseconds);
<<<<<<< HEAD
            
=======
>>>>>>> origin/master

            if (env.IsDevelopment())
            {
                //if (!stockService.DailyUsersTotalWorthExists()) 
                //stockService.SaveUsersTotalWorthPerDay(this);
            }
            
            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Entities.StockTransaction, Models.StockTransationDto>();
            });

            // FACEBOOK
            var facebookAuthenticatonOptions = new FacebookOptions
            {
                AppId = Configuration["Authentication:Facebook:AppId"],
                AppSecret = Configuration["Authentication:Facebook:AppSecret"],
                Events = new OAuthEvents
                {
                    OnCreatingTicket = context =>
                    {
                        var client = new FacebookClient(context.AccessToken);
                        dynamic info = client.Get("me", new { fields = "name,id,email,picture.width(300).height(300)" });
                        context.Identity.AddClaim(new Claim("pictureUrl", (string)info["picture"]["data"]["url"]));
                        context.Identity.AddClaim(new Claim(ClaimTypes.Email, info.email));
                        return Task.FromResult(0);
                    }
                }
            };
            facebookAuthenticatonOptions.Scope.Add("public_profile");
            facebookAuthenticatonOptions.Scope.Add("email");
            app.UseFacebookAuthentication(facebookAuthenticatonOptions);

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.Use(async (context, next) =>
            {
                if (context.Request.IsHttps)
                {
                    await next();
                }
                else
                {
                    var sslPortStr = sslPort == 0 || sslPort == 443 ? string.Empty : $":{sslPort}";
                    var httpsUrl = $"https://{context.Request.Host.Host}{sslPortStr}{context.Request.Path}";
                    context.Response.Redirect(httpsUrl);
                }
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private int GetMillisecondsToMidnight()
        {
            var openTime = DateTime.Today.AddHours(0.0);
            var now = DateTime.Now;

            if (now > openTime)
                openTime = openTime.AddDays(1);

            return (int)((openTime - now).TotalMilliseconds);
        }
    }
}
