namespace Gateway.Domain.ProxyConfig;

public sealed class RouteTransform {
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Guardamos el transform como key/value genérico para soportar el set completo de YARP
    /// (PathPattern, RequestHeadersCopy, etc) sin rehacer el modelo cada vez.
    /// </summary>
    public string Key { get; private set; } = default!;
    public string Value { get; private set; } = default!;

    private RouteTransform() { }

    public RouteTransform(string key, string value) {
        Key = key;
        Value = value;
    }
}