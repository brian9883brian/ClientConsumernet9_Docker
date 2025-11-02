namespace ClientConsumerOrder.Events
{
    public class OrderDeletedEvent : OrderBaseEvent
    {
        public override string EventType => "OrderDeleted";

        public string Reason { get; set; } = string.Empty;
        public DateTime DeletedDate { get; set; } = DateTime.UtcNow;
        public string DeletedBy { get; set; } = string.Empty;
    }
}
