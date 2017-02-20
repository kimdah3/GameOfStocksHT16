using System;
using System.Globalization;
using System.Threading;
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
using Microsoft.AspNetCore.Identity;

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

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            var cultureInfo = new CultureInfo("sv-SE") { NumberFormat = { CurrencySymbol = "kr" } };


            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            _downloadStocksTimer = new Timer(stockService.SaveStocksOnStartup, null, 20 * 1000, 60 * 10 * 1000);
            _completeStockTransTimer = new Timer(stockService.CompleteStockTransactions, null, 30 * 1000, /*Timeout.Infinite*/30 * 1000);
            _saveUsersTotalWorthPerDay = new Timer(stockService.SaveUsersTotalWorthPerDay, null, GetMillisecondsToMidnight(), TimeSpan.FromDays(1).Milliseconds);

            if (env.IsDevelopment())
            {
                if (!stockService.DailyUsersTotalWorthExists())
                    stockService.SaveUsersTotalWorthPerDay(this);
            }


            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Entities.StockTransaction, Models.StockTransationDto>();
            });

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

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
