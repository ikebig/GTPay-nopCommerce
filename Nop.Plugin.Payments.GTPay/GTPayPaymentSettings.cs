using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.GTPay
{
    public class GTPayPaymentSettings : ISettings
    {
        public string DescriptionText { get; set; }
        public bool UseSandbox { get; set; }
        public bool ShowGTPayPage { get; set; }
        public string MerchantId { get; set; }
        public string MerchantHashKey { get; set; }
        public string PreferredGateway { get; set; }
        public bool ShowCustomerName { get; set; }
    }
}
