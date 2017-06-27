using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.GTPay
{
    public class GTPayPaymentSettings : ISettings
    {
        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store.
        /// </summary>
        public string DescriptionText { get; set; }

        /// <summary>
        ///  Gets a value indicating whether we should display a payment information page for this plugin.
        /// </summary>
        public bool SkipPaymentInfo { get; set; }

        public bool UseSandbox { get; set; }
        public bool ShowGTPayPage { get; set; }
        public string MerchantId { get; set; }
        public string MerchantHashKey { get; set; }
        public string PreferredGateway { get; set; }
        public bool ShowCustomerName { get; set; }

         
    }
}
