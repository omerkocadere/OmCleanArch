namespace CleanArch.Infrastructure.Options;

public sealed class RabbitMQOptions
{
    public const string SectionName = "RabbitMQ";
    public bool Enabled { get; set; } = false;
    public string Host { get; set; } = "localhost";
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public int Port { get; set; } = 5672;
}
