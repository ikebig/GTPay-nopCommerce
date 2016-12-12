using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.GTPay.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }
        public string DescriptionText { get; set; }
        public bool DescriptionText_OverrideForStore { get; set; }
    }
}