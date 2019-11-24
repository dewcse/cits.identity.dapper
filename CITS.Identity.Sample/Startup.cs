using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CITS.Identity.Dapper;
using DbUp;
using CITS.Identity.Sample.Areas.Identity.Data;
using CITS.Identity.Dapper.Data;
using CITS.Identity.Dapper.Stores;

namespace CITS.Identity.Sample
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
            string connectionString = Configuration.GetConnectionString("DefaultConnection");

            //create Database if it doesn't exist
            EnsureDatabase.For.PostgresqlDatabase(connectionString);

            //start by running the scripts from library as base starting point
            var upgrader = DeployChanges.To.PostgresqlDatabase(connectionString)
                        .WithScriptsEmbeddedInAssembly(System.Reflection.Assembly.GetAssembly(typeof(ApplicationUser)))
                        .LogToConsole()
                        .Build();

            var result = upgrader.PerformUpgrade();


            //services.AddDbContext<ApplicationDbContext>(options =>
            //		options.UseSqlServer(
            //				Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<ApplicationUser, Dapper.IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                    //.AddEntityFrameworkStores<ApplicationDbContext>();
                    .AddUserManager<UserManager<ApplicationUser>>()
                    .AddRoleManager<RoleManager<Dapper.IdentityRole>>()
                    .AddSignInManager<SignInManager<ApplicationUser>>()
                //.AddDapperStores(connectionString)
                .AddDefaultTokenProviders();


            //// Identity Services, What it takes : -
            ///
            services.AddTransient<IUserStore<Dapper.IdentityUser>, UserStore>();
            services.AddTransient<IUserPasswordStore<Dapper.IdentityUser>, CustomUserStore>();
            services.AddTransient<IUserPhoneNumberStore<Dapper.IdentityUser>, CustomUserStore>();
            services.AddTransient<IUserTwoFactorStore<Dapper.IdentityUser>, CustomUserStore>();
            services.AddTransient<IUserEmailStore<Dapper.IdentityUser>, CustomUserStore>();
            services.AddTransient<IRoleStore<Dapper.IdentityRole>, CustomRoleStore>();

            /// Get error, but need to push ApplicationUSer instead of Identity User

            //services.AddTransient<IUserStore<ApplicationUser>, UserStore>();
            //services.AddTransient<IUserPasswordStore<ApplicationUser>, CustomUserStore>();
            //services.AddTransient<IUserPhoneNumberStore<ApplicationUser>, CustomUserStore>();
            //services.AddTransient<IUserTwoFactorStore<ApplicationUser>, CustomUserStore>();
            //services.AddTransient<IUserEmailStore<ApplicationUser>, CustomUserStore>();
            //services.AddTransient<IRoleStore<ApplicationUser>, CustomRoleStore>();


            //services.AddTransient<SqlConnection>(e => new SqlConnection(connectionString));
            services.AddTransient<IDatabaseConnectionFactory>(e => new ConnectionFactory(connectionString));
            services.AddTransient<DapperUsersTable>();

            services.AddControllersWithViews();
            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
            });
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
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
