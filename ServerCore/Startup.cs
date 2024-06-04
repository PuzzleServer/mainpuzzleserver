using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerCore.Areas.Deployment;
using ServerCore.Areas.Identity;
using ServerCore.Areas.Identity.UserAuthorizationPolicy;
using ServerCore.DataModel;
using ServerCore.ServerMessages;

namespace ServerCore
{
    public class Startup
    {
        IWebHostEnvironment _hostEnv;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _hostEnv = env;
            Configuration = configuration;
            MailHelper.Initialize(Configuration, env.IsDevelopment());
        }

        public Startup(IWebHostEnvironment env)
        {
            // Set up to use Azure settings
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = configBuilder.Build();
            MailHelper.Initialize(Configuration, env.IsDevelopment());
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Endpoint routing breaks most of our links because it doesn't include "ambient" route parameters
            // like eventId and eventRole in links -- they'd have to be manually specified everywhere
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            });

            services.AddRazorPages()
            .AddRazorPagesOptions(options =>
            {
                options.Conventions.AuthorizeFolder("/Pages");
                options.Conventions.AuthorizeFolder("/ModelBases");
            }).AddViewOptions(options =>
            {
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development)
                {
                    // Disables javascript validation of all fields when running in Development
                    // Primary use case is to work around a bug in the current url validation used by jquery
                    // Said bug causes false-positives with urls that are on localhost
                    // Those are usually used when setting the custom url field of a puzzle to a storage emulator link
                    options.HtmlHelperOptions.ClientValidationEnabled = false;
                }
            });

            services.AddServerSideBlazor();

            DeploymentConfiguration.ConfigureDatabase(Configuration, services, _hostEnv);
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
                options.AddPolicy("PlayerCanSeePuzzle", policy => policy.Requirements.Add(new PlayerCanSeePuzzleRequirement()));
                options.AddPolicy("PlayerIsOnTeam", policy => policy.Requirements.Add(new PlayerIsOnTeamRequirement()));
                options.AddPolicy("IsAuthorOfPuzzle", policy => policy.Requirements.Add(new IsAuthorOfPuzzleRequirement()));
                options.AddPolicy("IsEventAdminOrEventAuthor", policy => policy.Requirements.Add(new IsEventAdminOrEventAuthorRequirement()));
                options.AddPolicy("IsEventAdminOrPlayerOnTeam", policy => policy.Requirements.Add(new IsEventAdminOrPlayerOnTeamRequirement()));
                options.AddPolicy("IsEventAdminOrAuthorOfPuzzle", policy => policy.Requirements.Add(new IsEventAdminOrAuthorOfPuzzleRequirement()));
                options.AddPolicy("IsMicrosoftOrCommunity", policy => policy.Requirements.Add(new IsMicrosoftOrCommunityRequirement()));
                options.AddPolicy("IsRegisteredForEvent", policy => policy.Requirements.Add(new IsRegisteredForEventRequirement()));
            });

            services.AddCors(options =>
            {
                options.AddPolicy("PuzzleApi",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost", "http://127.0.0.1:10000", "http://puzzleserverteststore.blob.core.windows.net", "https://puzzleserverteststore.blob.core.windows.net")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    });
            });

            services.AddScoped<IAuthorizationHandler, IsAuthorInEventHandler>();
            services.AddScoped<IAuthorizationHandler, IsAdminInEventHandler>();
            services.AddScoped<IAuthorizationHandler, IsGlobalAdminHandler>();
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
            services.AddScoped<IAuthorizationHandler, IsMicrosoftOrCommunityHandler>();

            services.AddScoped<BackgroundFileUploader>();
            services.AddScoped<AuthorizationHelper>();

            var signalRBuilder = services.AddSignalR();

            // Azure SignalR free tier only allows 20 connections and each of the SignalR Hub and Blazor default to 5 connections per frontend.
            // This is too small to be practical, so rely on a setting to decide whether to use it.
            bool useAzureSignalR = Configuration.GetValue<bool>("UseAzureSignalR");
            if (useAzureSignalR)
            {
                // This automatically reads the connection string from the "Azure:SignalR:ConnectionString" setting.
                // To use this locally, add a connection string to your User Secrets file.
                signalRBuilder.AddAzureSignalR(options =>
                {
                    // Ensure Blazor connections get back to the frontend they initially connected to
                    options.ServerStickyMode = Microsoft.Azure.SignalR.ServerStickyMode.Required;

                    // Lowered for local debugging to not run out of connections on free tier
                    if (_hostEnv.IsDevelopment())
                    {
                        options.InitialHubServerConnectionCount = 1;
                    }
                });
            }
            services.AddSingleton<ServerMessageListener>();
            services.AddSingleton<PresenceStore>();
            services.AddSingleton<NotificationHelper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            // Allow cookies to be shared between localhost and the site name for local development
            if (env.IsDevelopment())
            {
                app.UseCookiePolicy(new CookiePolicyOptions()
                {
                    MinimumSameSitePolicy = SameSiteMode.Lax
                });
            }

            app.UseHttpsRedirection();

            // According to the Identity Scaffolding readme the order of the following calls matters
            // Must be UseStaticFiles, UseAuthentication, UseMvc
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapHub<ServerMessageHub>("/serverMessage");
            });

            app.UseMvc();

            app.UseCors();
        }
    }
}
