namespace Infrastructure.Messaging
{
    public class RabbitMQConfig
    {
        public string Host { get; set; }
        public int Port { get; set; } = 5672;
        public string Username { get; set; }
        public string Password { get; set; }
        public string VHost { get; set; } = "/";
    }
}
