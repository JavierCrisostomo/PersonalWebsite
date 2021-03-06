﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PersonalWebsite.Models;
using PersonalWebsite.Providers;
using PersonalWebsite.Repositories;
using PersonalWebsite.Services;

namespace PersonalWebsite
{
    public class Startup
    {
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

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var sqlContextConfigurator = new DbContextConfigurator(Configuration);
            services.AddEntityFramework()
                .AddDbContext<AuthDbContext>(sqlContextConfigurator.Configure)
                .AddDbContext<DataDbContext>(sqlContextConfigurator.Configure);

            // Add Identity services to the services container.
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(
                // Configure login page path
                x => x.Cookies.ApplicationCookie.LoginPath = new PathString("/Private/Account/Login")
            );

            // Add MVC services to the services container.
            services.AddMvc()
                    .AddViewLocalization();

            services.AddTransient<IContentRepository, ContentRepository>();
            services.AddTransient<IApplicationUserRepository, ApplicationUserRepository>();
            services.AddTransient<IHumanReadableContentRetrievalService, HumanReadableContentRetrievalService>();
            services.AddTransient<IRequiredDataRepository, RequiredDataRepository>();
            services.AddTransient<IInternalContentRepository, InternalContentRepository>();

            services.AddTransient<IContentEditorRepository, ContentEditorRepository>();

            services.AddSingleton<IRoutesBuilder, RoutesBuilder>();
            services.AddSingleton<ILanguageManipulationService, LanguageManipulationService>();
            services.AddSingleton<IPageConfiguration, PageConfiguration>();
            services.AddSingleton<IConfiguration>(Configuration);
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IPageConfiguration pageConfiguration,
            ILanguageManipulationService languageManipulationService,
            IRoutesBuilder routesBuilder)
        {
            loggerFactory.AddConsole();

            // Configure the HTTP request pipeline.

            // Add the following to the request pipeline only in development environment.
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();

                loggerFactory.AddDebug(LogLevel.Debug);
            }
            else
            {
                loggerFactory.AddDebug(LogLevel.Critical);
            }

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            // Add cookie-based authentication to the request pipeline.
            app.UseIdentity();
            

            var defaultCulture = languageManipulationService
                                   .LanguageDefinitionToCultureInfo(
                                      pageConfiguration.DefaultLanguage
                                   );

            app.UseRequestLocalization(new RequestLocalizationOptions
                {
                    SupportedCultures = languageManipulationService.SupportedCultures,
                    SupportedUICultures = languageManipulationService.SupportedCultures,
                    RequestCultureProviders = new[] { new CustomUrlStringCultureProvider(languageManipulationService) },
                    DefaultRequestCulture = new RequestCulture(defaultCulture, defaultCulture)
                }
            );

            app.UseMvc(routesBuilder.Build);

            using (var dataInitializer = new DataInitializer(app.ApplicationServices))
            {
                dataInitializer.EnsureRequiredContentsAvailable();
                dataInitializer.EnsureInitialUserAvaialble();
            }
        }
    }
}
