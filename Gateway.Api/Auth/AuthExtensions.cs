// Auth/AuthExtensions.cs
using System.Text;
using Gateway.Api.Auth.Dtos;
using Gateway.Api.Auth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Gateway.Api.Auth;

public static class AuthExtensions {
    public const string ProxyPolicyName = "ProxyPolicy";

    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config) {
        services.Configure<JwtOptions>(config.GetSection("Jwt"));
        services.AddSingleton<JwtTokenService>();

        var jwt = config.GetSection("Jwt").Get<JwtOptions>()
                  ?? throw new InvalidOperationException("Missing Jwt configuration");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => {
                options.RequireHttpsMetadata = false; // MVP (en prod: true)
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization(o => {
            // Política que usará YARP por defecto (puedes agregar más)
            o.AddPolicy(ProxyPolicyName, policy => {
                policy.RequireAuthenticatedUser();
            });
        });

        return services;
    }
}
