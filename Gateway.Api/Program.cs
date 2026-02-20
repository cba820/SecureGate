using Gateway.Api.Auth;
using Gateway.Api.Endpoints;
using Gateway.Api.Observability;
using Gateway.Api.Proxy;
using Gateway.Infrastructure.Database;
using Gateway.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Config separada de YARP
builder.Configuration.AddJsonFile("reverseproxy.json", optional: false, reloadOnChange: true);

// JWT
builder.Services.AddJwtAuth(builder.Configuration);

// YARP
builder.Services.AddGatewayProxy(builder.Configuration);

//DependencyInjection de la base de datos (DbContext, repositorios, etc.)
builder.Services.AddGatewayInfrastructure(builder.Configuration);

// Logging middleware
builder.Services.AddGatewayTransactionLogging(builder.Configuration);

var app = builder.Build();

// Swagger (yo lo dejo habilitado siempre para MVP; en prod puedes condicionarlo)
app.UseSwagger();
app.UseSwaggerUI();

// Si quieres forzar HTTPS, mantenlo (en algunos escenarios de dev puede molestar)
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseGatewayTransactionLogging();

app.MapAuthEndpoints();
app.MapGatewayProxyEndpoints();

using (var scope = app.Services.CreateScope()) {
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    await IdentitySeed.SeedAsync(userManager, roleManager);
}

app.Run();
