using ItServiceApp.Business.MapperProfiles;
using ItServiceApp.Business.Services.Email;
using ItServiceApp.Business.Services.Payment;
using ItServiceApp.Core.Identity;
using ItServiceApp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ItServiceApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MyContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("SqlConnection"));
            });
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                //Password Settings.
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 5;

                //Lockout Settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.AllowedForNewUsers = true;

                //User Settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
                /*options.SignIn.RequireConfirmedEmail = true;*/ // Emailden gelen do�rulamay� onaylamayan ki�ileri sisteme giremez.  
            }).AddEntityFrameworkStores<MyContext>().AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                //Cookie Settings.
                options.ExpireTimeSpan = TimeSpan.FromMinutes(120);

                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });
            services.AddTransient<IEmailSender, EmailSender>();  // Loose coupling -- Gev�ek Ba�
            services.AddScoped<IPaymentService, IyzicoPaymentService>();  // Loose coupling -- Gev�ek Ba�
            services.AddAutoMapper(options =>
            {
                options.AddProfile<PaymentProfile>();
                //options.AddProfile(typeof(PaymentProfile)); // 2. yaz�m �ekli.
                options.AddProfile(typeof(EntityProfile));
            });
            services.AddControllersWithViews()
                .AddNewtonsoftJson(
                options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"node_modules")),
                RequestPath = new PathString("/vendor")
            }); // Node_modules klas�r�n� static yapmak i�in.(wwwroot klas�r�ne atmak i�in).

            app.UseRouting();

            app.UseAuthentication(); //Login Logout kullanabilmek i�in.
            app.UseAuthorization(); //Authorization attiribute kullanabilmek i�in.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapAreaControllerRoute(
                    areaName: "Admin",
                    name: "Admin",
                    pattern: "Admin/{controller=Manage}/{action=Index}/{id?}"
                    );
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}"
                    );

            });
        }
    }
}
