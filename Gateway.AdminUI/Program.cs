using Gateway.AdminUI.Services;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Session para guardar el JWT (server-side)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});

// HttpClient hacia Gateway.Api
builder.Services.AddHttpClient("GatewayApi", client => {
    var baseUrl = builder.Configuration["GatewayApi:BaseUrl"]
        ?? throw new InvalidOperationException("Missing GatewayApi:BaseUrl");

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<GatewayApiClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage(); // 👈 clave para ver por qué “no se ven”
}
else {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

// Opcional: si algún día usas [Authorize] en AdminUI
// app.UseAuthentication();
// app.UseAuthorization();

// MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// 👇 fallback útil si te quedas sin rutas (diagnóstico)
app.MapFallbackToController("Login", "Auth");

app.Run();