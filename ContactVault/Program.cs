using ContactVault.Data;
using ContactVault.Models;
using ContactVault.Services.Interfaces;
using ContactVault.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using ContactVault.Helpers;
using Npgsql;

namespace ContactVault
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Add services to the container.
            //var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            //var connectionString = builder.Configuration.GetSection("pgSettings")["pgConnection"] ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            //var connectionString = ConnectionHelper.GetConnectionString(builder.Configuration);
            

            var databaseURI = new Uri("postgres://dkswxjmctuetey:93b82f4f7ae88c573485a7ddb296226dd29a3624c2fe0cc9a71e4b66e30bfa99@ec2-54-234-13-16.compute-1.amazonaws.com:5432/ddd5isuicvjtvq");
            var userInfo = databaseURI.UserInfo.Split(':');

            string connectionString = new NpgsqlConnectionStringBuilder()
            {
                Host = databaseURI.Host,
                Port = databaseURI.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseURI.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            }.ToString();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();

            //Custom Services
            builder.Services.AddScoped<IimageService, ImageService>();
            builder.Services.AddScoped<IAddressBookService, AddressBookService>();
            builder.Services.AddScoped<IEmailSender, EmailService>();
            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

            var app = builder.Build();
            var scope = app.Services.CreateScope();
            // get the database update with the latest migrations
            await DataHelper.ManageDataAsync(scope.ServiceProvider); 



            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Home/HandleError/{0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}