using Nop.Web.Framework.Mvc.Routes;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nop.Plugin.Payments.GTPay
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.GTPay.Configure",
                "Plugins/PaymentGTPay/Configure",
                new { controller = "PaymentGTPay", action = "Configure" },
                new[] { "Nop.Plugin.Payments.GTPay.Controllers" }
           );

            routes.MapRoute("Plugin.Payments.GTPay.PaymentInfo",
                 "Plugins/PaymentGTPay/PaymentInfo",
                 new { controller = "PaymentGTPay", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.GTPay.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.GTPay.SubmitPaymentInfo",
                "Plugins/PaymentGTPay/SubmitPaymentInfo",
                new { controller = "PaymentGTPay", action = "SubmitPaymentInfo" },
                new[] { "Nop.Plugin.Payments.GTPay.Controllers" }
           );

            routes.MapRoute("Plugin.Payments.GTPay.ReturnPaymentInfo",
                "Plugins/PaymentGTPay/ReturnPaymentInfo",
                new { controller = "PaymentGTPay", action = "ReturnPaymentInfo" },
                new[] { "Nop.Plugin.Payments.GTPay.Controllers" }
           );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
