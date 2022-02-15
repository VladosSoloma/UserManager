using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Presentation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var config = builder.Configuration;
var initialScopes = config.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');
builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddMicrosoftGraph(
        config["DownstreamApi:BaseUrl"],
        config.GetValue<string>("DownstreamApi:Scopes"))
    .AddInMemoryTokenCaches();
builder.Services.AddControllersWithViews(opts =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    opts.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI().AddApplicationPart(typeof(PresentationReference).Assembly);

var app = builder.Build();

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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
