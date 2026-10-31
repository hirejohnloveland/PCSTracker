using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace PartsTrackerErp.GoldenPattern.Api.Registration;

internal static class AuthenticationRegistration
{
    internal static IHostApplicationBuilder AddApiAuthentication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        return builder;
    }
}
