namespace Backend.Features.Subscriptions
{
    public class SubscriptionUpgradePreviewDto
    {
        public decimal OriginalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public string? Message { get; set; }
        public bool CanUpgrade { get; set; }
    }
}
