using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReadingListPlus.Common;
using ReadingListPlus.DataAccess;
using ReadingListPlus.Repositories;
using ReadingListPlus.Services;
using ReadingListPlus.Services.ArticleExtractorService;

namespace ReadingListPlus.Web.Core
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
            AddSettings(services);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    Constants.BackupPolicy,
                    policy => policy.RequireClaim(Constants.BackupClaim, Constants.BackupClaim));

                options.AddPolicy(
                    Constants.FixPolicy,
                    policy => policy.RequireClaim(Constants.FixClaim, Constants.FixClaim));                
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddScoped<IApplicationContext, ApplicationContext>();
            AddRepositories(services);
            AddServices(services);

            services.AddSingleton<IHttpClientWrapper>(new HttpClientWrapper());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ApplicationContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            context.Database.Migrate();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void AddSettings(IServiceCollection services)
        {
            var settings = new Settings();
            Configuration.GetSection("Settings").Bind(settings);

            services.AddSingleton<ISettings>(settings);
        }

        private static void AddServices(IServiceCollection services)
        {
            services.AddTransient<IDeckService, DeckService>();
            services.AddTransient<ICardService, CardService>();

            services.AddTransient<ILocalExtractorService, LocalExtractorService>();
            services.AddTransient<IRemoteExtractorService, RemoteExtractorService>();
            services.AddTransient<IArticleExtractorService, CombinedExtractorService>();

            services.AddTransient<ISchedulerService, SchedulerService>();
            services.AddTransient<ITextConverterService, TextConverterService>();
            services.AddTransient<IRepetitionCardService, RepetitionCardService>();
        }

        private static void AddRepositories(IServiceCollection services)
        {
            services.AddTransient<IDeckRepository, DeckRepository>();
            services.AddTransient<ICardRepository, CardRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
        }
    }
}
