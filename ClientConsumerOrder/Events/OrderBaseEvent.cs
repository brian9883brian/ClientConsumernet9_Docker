namespace ClientConsumerOrder.Events
{
    public abstract class OrderBaseEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
        public abstract string EventType { get; }

        public Guid OrderId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
