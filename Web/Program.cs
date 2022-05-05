using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Presentation;
using Services.Abstractions;
using Services;
using Web.Middleware;
using Microsoft.ApplicationInsights;
using Serilog;
using Web.LoggerExtentions;

var builder = WebApplication.CreateBuilder(args);
TelemetryClient? client = null;
Log.Logger = new LoggerConfiguration().AddLoggingProviders(builder.Configuration, ref client).CreateLogger();
try
{
    builder.Host.UseSerilog();
    // Add services to the container.
    builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration);
    builder.Services.AddControllersWithViews(opts =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            opts.Filters.Add(new AuthorizeFilter(policy));
        })
        .AddMicrosoftIdentityUI()
        .AddApplicationPart(typeof(PresentationReference).Assembly);
    builder.Services.AddAuthorization(policies =>
    {
        policies.AddPolicy("Admin",
            p =>
            {
                p.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Admin");
            });
    });
    builder.Services.AddScoped<IUserService, UserService>();
    var app = builder.Build();
    app.UseExceptionHandler("/Home/Error");
    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.AddExceptionHandling();
    app.UseStaticFiles();

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Failed to launch the host");
}
finally
{
    Log.CloseAndFlush();
    client?.Flush();
    Task.Delay(1000);
}
