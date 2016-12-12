using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Nop.Plugin.Payments.GTPay.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            AvailableGateways = new List<SelectListItem>();
        }
        public int ActiveStoreScopeConfiguration { get; set; }

        [AllowHtml]
        [NopResourceDisplayName(Constants.LocaleResources.GTPay_Fields_DescriptionText)]
        public string DescriptionText { get; set; }
        public bool DescriptionText_OverrideForStore { get; set; }

        [NopResourceDisplayName(Constants.LocaleResources.GTPay_Fields_UseSandbox)]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName(Constants.LocaleResources.GTPay_Fields_MerchantId)]
        public string MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }

        [NopResourceDisplayName(Constants.LocaleResources.GTPay_Fields_MerchantHashKey)]
        public string MerchantHashKey { get; set; }
        public bool MerchantHashKey_OverrideForStore { get; set; }

        [NopResourceDisplayName(Constants.LocaleResources.GTPay_Fields_ShowGTPayPage)]
        public bool ShowGTPayPage { get; set; }
        public bool ShowGTPayPage_OverrideForStore { get; set; }

        [NopResourceDisplayName(Constants.LocaleResources.GTPay_Fields_ShowCustomerName)]
        public bool ShowCustomerName { get; set; }
        public bool ShowCustomerName_OverrideForStore { get; set; }

        [NopResourceDisplayName(Constants.LocaleResources.GTPay_Fields_PreferredGateway)]
        public string PreferredGateway { get; set; }
        public bool PreferredGateway_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableGateways { get; set; }
    }
}