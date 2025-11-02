namespace ClientConsumerOrder.Models
{
    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public int PaymentMethod { get; set; }
        public string Status { get; set; } = "Created";
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
