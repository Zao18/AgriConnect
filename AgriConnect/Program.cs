using AgriConnect.Models;
using AgriConnect.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// ? Add Cookie Authentication only (no Identity)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

builder.Services.AddAuthorization();

// Optional: Add session support
builder.Services.AddSession();

// Register Azure TableStorageService
builder.Services.AddSingleton<ITableStorageService<AgriConnect.Models.FarmerEntity>>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    return new TableStorageService<AgriConnect.Models.FarmerEntity>(config, "Farmers");
});

builder.Services.AddSingleton<ITableStorageService<AgriConnect.Models.ProductEntity>>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    return new TableStorageService<AgriConnect.Models.ProductEntity>(config, "Products");
});

builder.Services.AddScoped<ITableStorageService<ApplicationUser>>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    return new TableStorageService<ApplicationUser>(config, "Users");
});

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication(); // ? Enables cookie auth
app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
