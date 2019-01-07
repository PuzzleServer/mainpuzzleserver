using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerCore.Areas.Identity.UserAuthorizationPolicy;
using ServerCore.DataModel;

namespace ServerCore
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
            services.AddMvc()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Events");
                    options.Conventions.AuthorizeFolder("/Puzzles");
                    options.Conventions.AuthorizeFolder("/Shared");
                    options.Conventions.AuthorizeFolder("/Teams");
                });

            services.AddDbContext<PuzzleServerContext>
                (options => options.UseLazyLoadingProxies()
                    .UseSqlServer(Configuration.GetConnectionString("PuzzleServerContext")));

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
            services.AddScoped<IAuthorizationHandler, IsRegisteredForEventHandler_Admin>();
            services.AddScoped<IAuthorizationHandler, IsRegisteredForEventHandler_Author>();
            services.AddScoped<IAuthorizationHandler, IsRegisteredForEventHandler_Player>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                PuzzleServerContext.UpdateDatabase(app);
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
