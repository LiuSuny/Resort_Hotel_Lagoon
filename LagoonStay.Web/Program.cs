using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LagoonStay.Application.Common.Interfaces;
using LagoonStay.Domain.Entities;
using LagoonStay.Infrastructure.Data;
using LagoonStay.Infrastructure.Repository;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(option =>
option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser,  IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

//configure the cookie settings for authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LoginPath = "/Account/Login";
});

//configure the identity options for password, lockout and user settings
builder.Services.Configure<IdentityOptions>(option =>
{
    option.Password.RequiredLength = 6;
    //option.Password.RequireDigit = true;
    //option.Password.RequireLowercase = true;
    //option.Password.RequireNonAlphanumeric = true;
    //option.Password.RequireUppercase = true;
    //option.Password.RequiredUniqueChars = 1;
    //option.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    //option.Lockout.MaxFailedAccessAttempts = 5;
    //option.Lockout.AllowedForNewUsers = true;
    
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();
//configure the Stripe API key & Need to uncommented 
//StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:StripeSec").Get<string>(); //This line retrieves the Stripe secret key from the configuration and sets it for use in the application.

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
