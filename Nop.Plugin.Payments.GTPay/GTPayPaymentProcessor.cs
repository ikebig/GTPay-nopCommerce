using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.GTPay.Controllers;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Nop.Plugin.Payments.GTPay
{
    /// <summary>
    /// GTPay payment processor
    /// </summary>
    public class GTPayPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly GTPayPaymentSettings _gtPayPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public GTPayPaymentProcessor(GTPayPaymentSettings gtPayPaymentSettings,
            ISettingService settingService,
            IGenericAttributeService genericAttributeService,
            ICurrencyService currencyService, ICustomerService customerService,
            CurrencySettings currencySettings, IWebHelper webHelper,
            IOrderTotalCalculationService orderTotalCalculationService,
            IStoreContext storeContext)
        {
            this._gtPayPaymentSettings = gtPayPaymentSettings;
            this._settingService = settingService;
            this._genericAttributeService = genericAttributeService;
            this._currencyService = currencyService;
            this._customerService = customerService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._storeContext = storeContext;
        }

        #endregion

        #region Utilities




        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            int orderId = postProcessPaymentRequest.Order.Id;

            string url = $"~/Plugins/PaymentGTPay/SubmitPaymentInfo?orderId={orderId}";
            HttpContext.Current.Response.Redirect(url);
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {

            //load settings for a chosen store scope
            var storeScope = _storeContext.CurrentStore.Id;
            var currencySettings = _settingService.LoadSetting<CurrencySettings>(storeScope);

            var supportedCurrencyCodes = GTPayHelper.GetSupportedCurrencyCodes();
            var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (primaryStoreCurrency == null || !supportedCurrencyCodes.Contains(primaryStoreCurrency.CurrencyCode))
                return true;

            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return 0;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported.");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported.");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported.");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported.");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported.");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //it's a redirection payment method. So we always return true
            return true;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentGTPay";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.GTPay.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentGTPay";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.GTPay.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentGTPayController);
        }

        public override void Install()
        {
            //settings
            var settings = new GTPayPaymentSettings
            {
                UseSandbox = true,
                DescriptionText = "<p><b>GTPAY accepts both locally and internationally issued cards including Interswitch, MasterCard and VISA.</b><br /></p>",
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_DescriptionText, "Description");
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_DescriptionText_Hint, "Enter info that will be shown to customers during checkout");
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_DescriptionText_Required, "Description is required.");
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_UseSandbox, "Use sandbox");
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_UseSandbox_Hint, "Check to enable Sandbox (testing environment).");
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_MerchantId, "Merchant identifier");
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_MerchantId_Hint, "Specify merchant identifier.");
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_MerchantId_Required, "Merchant identifier is required.");
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_MerchantHashKey, "Merchant hash key");
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_MerchantHashKey_Hint, "Specify merchant hash key provided by GTPay on merchant setup.");
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_MerchantHashKey_Required, "Merchant hash key is required.");
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_ShowGTPayPage, "Show GTPay page");
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_ShowGTPayPage_Hint, "Check to show GTPay's own first page, from where the customer will click Continue to go to the gateway.");
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_PreferredGateway, "Preferred gateway");
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_PreferredGateway_Hint, "If specified, then customer cannot choose what gateway to use for the transaction.");
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_ShowCustomerName, "Show customer name");
            this.AddOrUpdatePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_ShowCustomerName_Hint, "Check to display customer name on the payment page for the customer.");

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<GTPayPaymentSettings>();

            //locales
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_DescriptionText);
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_DescriptionText_Hint);
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_DescriptionText_Required);
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_UseSandbox);
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_UseSandbox_Hint);
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_MerchantId);
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_MerchantId_Hint);
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_MerchantId_Required);
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_MerchantHashKey);
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_MerchantHashKey_Hint);
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_MerchantHashKey_Required);
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_ShowGTPayPage);
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_ShowGTPayPage_Hint);
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_PreferredGateway);
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_PreferredGateway_Hint);
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_ShowCustomerName);
            this.DeletePluginLocaleResource(Constants.LocaleResources.GTPay_Fields_ShowCustomerName_Hint);

            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get
            {
                return false;
            }
        }

        #endregion
    }
}