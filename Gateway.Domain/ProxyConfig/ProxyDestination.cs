namespace Gateway.Domain.ProxyConfig;

public sealed class ProxyDestination {
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string DestinationId { get; private set; } = default!; // ej: d1
    public string Address { get; private set; } = default!;       // ej: http://172.20.1.21:9533/
    public bool IsActive { get; private set; } = true;

    private ProxyDestination() { }

    public ProxyDestination(string destinationId, string address) {
        DestinationId = destinationId;
        Address = address;
    }

    public void Disable() => IsActive = false;
}