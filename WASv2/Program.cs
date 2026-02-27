using Microsoft.AspNetCore.Authentication.Cookies;

namespace WASv2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Authentication must be registered before building the app
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/SignIn";
                    //options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapControllerRoute(
                name: "PurchasingOfficer",
                pattern: "{controller=PurchasingOfficer}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapControllerRoute(
                name: "DepartmentHead",
                pattern: "{controller=DepartmentHead}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapControllerRoute(
                name: "DepartmentAdminStaff",
                pattern: "{controller=DepartmentAdminStaff}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapControllerRoute(
                name: "Supplier",
                pattern: "{controller=Supplier}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapControllerRoute(
                name: "PamStaff",
                pattern: "{controller=PamStaff}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapControllerRoute(
                name: "TopManagement",
                pattern: "{controller=TopManagement}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapControllerRoute(
                name: "Manager/Director",
                pattern: "{controller=Manager/Director}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapControllerRoute(
                name: "Authentication",
                pattern: "{controller=Auth}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
