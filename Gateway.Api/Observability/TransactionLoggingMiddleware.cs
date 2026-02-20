// Gateway.Api/Observability/TransactionLoggingMiddleware.cs
using Gateway.Domain.Auditing;
using Gateway.Infrastructure.Database.Repositories;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Yarp.ReverseProxy.Model;

namespace Gateway.Api.Observability;

public sealed class TransactionLoggingMiddleware : IMiddleware {
    private readonly IUnitOfWork _uow;
    private readonly TransactionLoggingOptions _opt;
    private const string RequestIdItemKey = "Gateway.RequestId";

    public TransactionLoggingMiddleware(IUnitOfWork uow, IOptions<TransactionLoggingOptions> opt) {
        _uow = uow;
        _opt = opt.Value;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
        if (!_opt.Enabled) {
            await next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        GatewayTransactionLog log = new();

        try {
            // Captura request temprano
            PopulateRequest(context, log);

            await next(context);

            // Captura response + routing (si existe)
            PopulateResponseAndRouting(context, log, sw, error: null);
        }
        catch (Exception ex) {
            // En excepción, intenta igualmente persistir log
            PopulateResponseAndRouting(context, log, sw, error: ex);
            throw;
        }
        finally {
            // IMPORTANTE: no loggear el body de /auth/token (o de nada, por ahora)
            // Persistimos el registro
            try {
                await _uow.Repository<GatewayTransactionLog>().AddAsync(log);
                await _uow.SaveChangesAsync();
            }
            catch {
                // No “rompas” el request si falla el logging
                // (si quieres, aquí podrías escribir a un logger)
            }
        }
    }

    private void PopulateRequest(HttpContext ctx, GatewayTransactionLog log) {
        log.TimestampUtc = DateTime.UtcNow;

        // TraceId: ideal si hay Activity (distributed tracing). Fallback a TraceIdentifier.
        log.TraceId = Activity.Current?.TraceId.ToString() ?? ctx.TraceIdentifier;

        // RequestId: único por request (no dependas de TraceIdentifier)
        var requestId = GetOrCreateRequestId(ctx);
        log.RequestId = requestId;

        // CorrelationId: prioriza x-correlation-id, luego x-request-id, luego RequestId
        if (ctx.Request.Headers.TryGetValue("x-correlation-id", out var corr) && !string.IsNullOrWhiteSpace(corr))
            log.CorrelationId = corr.ToString();
        else if (ctx.Request.Headers.TryGetValue("x-request-id", out var reqid) && !string.IsNullOrWhiteSpace(reqid))
            log.CorrelationId = reqid.ToString();
        else
            log.CorrelationId = requestId;

        // Usuario (ASP.NET Core Identity)
        if (ctx.User?.Identity?.IsAuthenticated == true) {
            // UserId (Guid) viene normalmente como ClaimTypes.NameIdentifier
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId))
                log.UserId = userId;

            // Username viene normalmente como ClaimTypes.Name
            log.Username = ctx.User.Identity?.Name
                           ?? ctx.User.FindFirstValue(ClaimTypes.Name)
                           ?? ctx.User.FindFirstValue("preferred_username");
        }

        log.HttpMethod = ctx.Request.Method;
        log.InboundPath = ctx.Request.Path.Value ?? "/";
        log.InboundQuery = MaskQuery(ctx.Request.QueryString.Value);

        log.InboundIp = ctx.Connection.RemoteIpAddress?.ToString();
        log.UserAgent = ctx.Request.Headers.UserAgent.ToString();

        // Headers allowlist como JSON
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var h in _opt.AllowedHeaders) {
            if (ctx.Request.Headers.TryGetValue(h, out var v))
                headers[h] = v.ToString();
        }
        log.InboundHeaders = headers.Count == 0 ? null : JsonSerializer.Serialize(headers);

        // Tamaño body si viene
        if (ctx.Request.ContentLength.HasValue)
            log.RequestBodySizeBytes = ctx.Request.ContentLength.Value;
    }

    private static string GetOrCreateRequestId(HttpContext ctx) {
        // Si el cliente/env te manda uno, úsalo
        if (ctx.Request.Headers.TryGetValue("x-request-id", out var rid) && !string.IsNullOrWhiteSpace(rid))
            return rid.ToString();

        // Si ya lo creamos en este request, reutilízalo
        if (ctx.Items.TryGetValue(RequestIdItemKey, out var existing) && existing is string s && !string.IsNullOrWhiteSpace(s))
            return s;

        // Crear uno único
        var created = Guid.NewGuid().ToString("N");
        ctx.Items[RequestIdItemKey] = created;

        // (Opcional) exponerlo al cliente para troubleshooting
        ctx.Response.Headers["x-request-id"] = created;

        return created;
    }

    private void PopulateResponseAndRouting(HttpContext ctx, GatewayTransactionLog log, Stopwatch sw, Exception? error) {
        sw.Stop();
        log.DurationMs = sw.ElapsedMilliseconds;

        if (error is null) {
            log.StatusCode = ctx.Response.StatusCode;
        }
        else {
            // Si el proxy lanzó excepción, lo tratamos como 500 (puedes mapear algunos tipos a 502/504 si quieres)
            log.StatusCode = 500;
            log.ErrorCode = error.GetType().Name;
            log.ErrorMessage = Truncate(error.Message, 2000);
        }

        log.ResponseBodySizeBytes = null;

        // Path "final" (al menos path+query que salió del gateway)
        // Nota: esto es lo que el gateway vio, no necesariamente el path reescrito por transforms.
        log.ProxiedPath = ctx.Request.GetEncodedPathAndQuery();

        var proxyFeature = ctx.Features.Get<IReverseProxyFeature>();
        if (proxyFeature is not null) {
            // ✅ IDs estables
            log.RouteId = proxyFeature.Route?.Config?.RouteId;
            log.ClusterId = proxyFeature.Cluster?.Config?.ClusterId;

            // Destination: idealmente guardar Address (URL base real), no solo DestinationId
            // YARP expone modelos/config dependiendo de versión; intentamos sacar Address.
            string? destinationId = proxyFeature.ProxiedDestination?.DestinationId;

            // Best-effort: Address real
            string? destinationAddress = null;

            // Algunas versiones: proxyFeature.ProxiedDestination.Model.Config.Address
            destinationAddress ??= proxyFeature.ProxiedDestination?.Model?.Config?.Address;

            // Si no encontramos Address, al menos guardamos el DestinationId
            log.DestinationAddress = destinationAddress ?? destinationId;

            // Si quieres guardar el URL final "aproximado" (Address + path+query)
            // Esto NO garantiza incluir transforms exactos si tu ProxiedPath es inbound,
            // pero ayuda para troubleshooting.
            if (!string.IsNullOrWhiteSpace(destinationAddress)) {
                // Normaliza slash y concatena
                var baseUri = destinationAddress!.TrimEnd('/');
                var pq = ctx.Request.GetEncodedPathAndQuery();
                // pq empieza con /...  (o vacío)
                log.DestinationAddress = $"{baseUri}{pq}";
            }
        }

        // Código amigable para errores comunes
        if (log.StatusCode is 502)
            log.ErrorCode ??= "BAD_GATEWAY";
        else if (log.StatusCode is 504)
            log.ErrorCode ??= "GATEWAY_TIMEOUT";

        // Opcional: si no hubo excepción pero sí error status, agrega un código genérico
        if (error is null && log.StatusCode >= 500)
            log.ErrorCode ??= "UPSTREAM_ERROR";
    }
    private string? MaskQuery(string? qs) {
        if (string.IsNullOrWhiteSpace(qs)) return qs;

        // Muy simple: si quieres masking real por key, lo implementamos más adelante.
        // Por ahora, solo evita loggear “password” explícito.
        if (_opt.MaskQueryKeys is null || _opt.MaskQueryKeys.Length == 0) return qs;

        var url = new Uri("http://local" + qs); // hack para parsear query
        var q = System.Web.HttpUtility.ParseQueryString(url.Query);

        foreach (var key in _opt.MaskQueryKeys) {
            if (q[key] is not null)
                q[key] = "***";
        }

        var masked = q.ToString();
        return string.IsNullOrEmpty(masked) ? null : "?" + masked;
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s.Substring(0, max);
}
