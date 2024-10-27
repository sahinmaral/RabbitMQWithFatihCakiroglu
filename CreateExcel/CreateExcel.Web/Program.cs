using CreateExcel.Web.Hubs;
using CreateExcel.Web.Models;
using CreateExcel.Web.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using RabbitMQ.Client;

namespace CreateExcel.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("SQLServer"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure() 
                );
            });

            builder.Services.AddSingleton(sp => new ConnectionFactory() { Uri = new Uri(builder.Configuration["RabbitMQ:Uri"]), DispatchConsumersAsync = true });
            builder.Services.AddSingleton<RabbitMQClientService>();
            builder.Services.AddSingleton<RabbitMQPublisher>();

            builder.Services.AddSignalR();

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
            }).AddEntityFrameworkStores<AppDbContext>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
            });

            var app = builder.Build();

            using(var scope = app.Services.CreateScope())
            {
                var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                appDbContext.Database.Migrate();

                if (!appDbContext.Users.Any())
                {
                    userManager.CreateAsync(new IdentityUser()
                    {
                        UserName = "deneme",
                        Email = "deneme@outlook.com"
                    }, password: "Abc1234.").Wait();

                    userManager.CreateAsync(new IdentityUser()
                    {
                        UserName = "deneme2",
                        Email = "deneme2@outlook.com"
                    }, password: "Abc1234.").Wait();
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<MyHub>("/MyHub");

            app.Run();
        }
    }
}