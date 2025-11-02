namespace ClientConsumerOrder.Events
{
    public class OrderUpdatedEvent : OrderBaseEvent
    {
        public override string EventType => "OrderUpdated";

        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public string UpdatedBy { get; set; } = string.Empty;

        // Campos actualizables
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmailAddress { get; set; }
        public string? AddressLine { get; set; }
    }
}
