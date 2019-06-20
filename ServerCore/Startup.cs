﻿using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerCore.Areas.Deployment;
using ServerCore.Areas.Identity.UserAuthorizationPolicy;
using ServerCore.DataModel;

namespace ServerCore
{
    public class Startup
    {
        private IHostingEnvironment hostingEnvironment;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            hostingEnvironment = env;
            MailHelper.Initialize(Configuration, env.IsDevelopment());
        }

        public Startup(IHostingEnvironment env)
        {
            // Set up to use Azure settings
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = configBuilder.Build();
            MailHelper.Initialize(Configuration, env.IsDevelopment());

            hostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                                 .RequireAuthenticatedUser()
                                 .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddMvc()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Pages");
                    options.Conventions.AuthorizeFolder("/ModelBases");
                });

            DeploymentConfiguration.ConfigureDatabase(Configuration, services, hostingEnvironment);
            FileManager.ConnectionString = Configuration.GetConnectionString("AzureStorageConnectionString");

            services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = Configuration["Authentication-Microsoft-ApplicationId"];
                microsoftOptions.ClientSecret = Configuration["Authentication-Microsoft-Password"];
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsEventAuthor", policy => policy.Requirements.Add(new IsAuthorInEventRequirement()));
                options.AddPolicy("IsEventAdmin", policy => policy.Requirements.Add(new IsAdminInEventRequirement()));
                options.AddPolicy("IsGlobalAdmin", policy => policy.Requirements.Add(new IsGlobalAdminRequirement()));
                options.AddPolicy("IsPlayer", policy => policy.Requirements.Add(new IsPlayerInEventRequirement()));
                options.AddPolicy("PlayerCanSeePuzzle", policy => policy.Requirements.Add(new PlayerCanSeePuzzleRequirement()));
                options.AddPolicy("PlayerIsOnTeam", policy => policy.Requirements.Add(new PlayerIsOnTeamRequirement()));
                options.AddPolicy("IsAuthorOfPuzzle", policy => policy.Requirements.Add(new IsAuthorOfPuzzleRequirement()));
                options.AddPolicy("IsEventAdminOrEventAuthor", policy => policy.Requirements.Add(new IsEventAdminOrEventAuthorRequirement()));
                options.AddPolicy("IsEventAdminOrPlayerOnTeam", policy => policy.Requirements.Add(new IsEventAdminOrPlayerOnTeamRequirement()));
                options.AddPolicy("IsEventAdminOrAuthorOfPuzzle", policy => policy.Requirements.Add(new IsEventAdminOrAuthorOfPuzzleRequirement()));
                options.AddPolicy("IsRegisteredForEvent", policy => policy.Requirements.Add(new IsRegisteredForEventRequirement()));
            });

            services.AddScoped<IAuthorizationHandler, IsAuthorInEventHandler>();
            services.AddScoped<IAuthorizationHandler, IsAdminInEventHandler>();
            services.AddScoped<IAuthorizationHandler, IsGlobalAdminHandler>();
            services.AddScoped<IAuthorizationHandler, IsPlayerInEventHandler>();
            services.AddScoped<IAuthorizationHandler, PlayerCanSeePuzzleHandler>();
            services.AddScoped<IAuthorizationHandler, PlayerIsOnTeamHandler>();
            services.AddScoped<IAuthorizationHandler, IsAuthorOfPuzzleHandler>();

            services.AddScoped<IAuthorizationHandler, IsEventAdminOrAuthorOfPuzzleHandler_Admin>();
            services.AddScoped<IAuthorizationHandler, IsEventAdminOrAuthorOfPuzzleHandler_Author>();
            services.AddScoped<IAuthorizationHandler, IsEventAdminOrEventAuthorHandler_Admin>();
            services.AddScoped<IAuthorizationHandler, IsEventAdminOrEventAuthorHandler_Author>();
            services.AddScoped<IAuthorizationHandler, IsEventAdminOrPlayerOnTeamHandler_Admin>();
            services.AddScoped<IAuthorizationHandler, IsEventAdminOrPlayerOnTeamHandler_Play>();
            services.AddScoped<IAuthorizationHandler, IsRegisteredForEventHandler_Admin>();
            services.AddScoped<IAuthorizationHandler, IsRegisteredForEventHandler_Author>();
            services.AddScoped<IAuthorizationHandler, IsRegisteredForEventHandler_Player>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment() && String.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME")))
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                PuzzleServerContext.UpdateDatabase(app);
            }
            else if (env.IsStaging() && Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") == "PuzzleServerTestDeploy")
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                PuzzleServerContext.UpdateDatabase(app);
            }
            else if (env.IsProduction() && (Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") == "puzzlehunt" || Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") == "puzzleday"))
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // According to the Identity Scaffolding readme the order of the following calls matters
            // Must be UseStaticFiles, UseAuthentication, UseMvc
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
