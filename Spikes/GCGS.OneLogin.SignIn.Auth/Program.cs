using GCGS.OneLogin.SignIn.Auth.Models;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;

namespace GCGS.OneLogin.SignIn.Auth;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var oneLoginSettings = builder.Configuration.GetSection("OneLogin").Get<OneLoginSettings>();

        builder.Services.AddAuthentication(o =>
         {
             o.DefaultScheme = "cookie";
             o.DefaultChallengeScheme = "oidc";
         })
            .AddCookie("cookie", o =>
            {
                o.ExpireTimeSpan = TimeSpan.FromHours(1);
                o.SlidingExpiration = true;
                o.AccessDeniedPath = "/Home/AccessDenied";
            })
            .AddOpenIdConnect("oidc", o =>
            {
                o.Authority = oneLoginSettings!.AuthorityUrl;
                o.ClientId = oneLoginSettings.ClientId;
                o.ClientSecret = oneLoginSettings.ClientSecret;

                o.UsePkce = true;
                o.ResponseType = "code";
                o.ResponseMode = "query";
                o.SaveTokens = true;
                o.SignedOutCallbackPath = oneLoginSettings.CallbackPath;

                foreach (var scope in oneLoginSettings?.Scopes ?? Enumerable.Empty<string>())
                {
                    o.Scope.Add(scope);
                }

                o.GetClaimsFromUserInfoEndpoint = true;

                o.ClaimActions.Add(new JsonKeyClaimAction(JwtClaimTypes.Role, JwtClaimTypes.Role, JwtClaimTypes.Role));

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.Name,
                    RoleClaimType = JwtClaimTypes.Role
                };

                o.Events.OnRemoteFailure = async context =>
                {
                    context.Response.Redirect("/");
                    context.HandleResponse();
                    await Task.CompletedTask;
                };
            });

        builder.Services.AddAuthorization(o =>
        {
            o.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        builder.Services.AddControllersWithViews(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });
        builder.Services.AddRazorPages()
            .AddMicrosoftIdentityUI();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
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
        app.MapRazorPages();

        app.Run();
    }
}
