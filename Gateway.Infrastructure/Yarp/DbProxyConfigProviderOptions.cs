namespace Gateway.Infrastructure.Yarp;

public sealed class DbProxyConfigProviderOptions {
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(10);
}