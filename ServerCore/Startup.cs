using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                microsoftOptions.ClientId = Configuration["Authentication:Microsoft:ApplicationId"];
                microsoftOptions.ClientSecret = Configuration["Authentication:Microsoft:Password"];
            });
            
            services.AddAuthorization(options =>
            {
               options.AddPolicy("IsAuthor", policy => policy.Requirements.Add(new IsAuthorForEventRequirement(context.Event)));
               options.AddPolicy("IsAdmin", policy => policy.Requirements.Add(new IsAdminForEventRequirement(context.Event)));
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
