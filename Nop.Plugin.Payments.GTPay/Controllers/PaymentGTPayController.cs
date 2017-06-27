using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.GTPay.Models;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace Nop.Plugin.Payments.GTPay.Controllers
{
    public class PaymentGTPayController : BasePaymentController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly IEncryptionService _encryptionService;

        #endregion

        #region Ctor

        public PaymentGTPayController(IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILogger logger,
            PaymentSettings paymentSettings,
            ILocalizationService localizationService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IStoreContext storeContext,
            IEncryptionService encryptionService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._logger = logger;
            this._paymentSettings = paymentSettings;
            this._localizationService = localizationService;
            this._currencyService = currencyService;
            this._customerService = customerService;
            this._storeContext = storeContext;
            this._encryptionService = encryptionService;
        }

        #endregion

        #region Utils

        private void LoadAvailableGateWays(ConfigurationModel model)
        {
            model.AvailableGateways.Clear();
            model.AvailableGateways.Add(new SelectListItem() { Text = "", Value = "" });
            model.AvailableGateways.Add(new SelectListItem() { Text = "WebPay", Value = "webpay" });
            model.AvailableGateways.Add(new SelectListItem() { Text = "Mastercard International Gateway", Value = "migs" });
        }

        #endregion

        #region Methods

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var gtPayPaymentSettings = _settingService.LoadSetting<GTPayPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.SkipPaymentInfo = gtPayPaymentSettings.SkipPaymentInfo;
            model.DescriptionText = gtPayPaymentSettings.DescriptionText;
            model.UseSandbox = gtPayPaymentSettings.UseSandbox;
            model.ShowGTPayPage = gtPayPaymentSettings.ShowGTPayPage;
            model.MerchantId = gtPayPaymentSettings.MerchantId;
            model.MerchantHashKey = gtPayPaymentSettings.MerchantHashKey;
            model.PreferredGateway = gtPayPaymentSettings.PreferredGateway;
            model.ShowCustomerName = gtPayPaymentSettings.ShowCustomerName;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.SkipPaymentInfo_OverrideForStore = _settingService.SettingExists(gtPayPaymentSettings, x => x.SkipPaymentInfo, storeScope);
                model.DescriptionText_OverrideForStore = _settingService.SettingExists(gtPayPaymentSettings, x => x.DescriptionText, storeScope);
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(gtPayPaymentSettings, x => x.UseSandbox, storeScope);
                model.ShowGTPayPage_OverrideForStore = _settingService.SettingExists(gtPayPaymentSettings, x => x.ShowGTPayPage, storeScope);
                model.MerchantId_OverrideForStore = _settingService.SettingExists(gtPayPaymentSettings, x => x.MerchantId, storeScope);
                model.MerchantHashKey_OverrideForStore = _settingService.SettingExists(gtPayPaymentSettings, x => x.MerchantHashKey, storeScope);
                model.PreferredGateway_OverrideForStore = _settingService.SettingExists(gtPayPaymentSettings, x => x.PreferredGateway, storeScope);
                model.ShowCustomerName_OverrideForStore = _settingService.SettingExists(gtPayPaymentSettings, x => x.ShowCustomerName, storeScope);
            }

            LoadAvailableGateWays(model);

            return View("~/Plugins/Payments.GTPay/Views/PaymentGTPay/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var gtPayPaymentSettings = _settingService.LoadSetting<GTPayPaymentSettings>(storeScope);

            //save settings
            gtPayPaymentSettings.SkipPaymentInfo = model.SkipPaymentInfo;
            gtPayPaymentSettings.DescriptionText = model.DescriptionText;
            gtPayPaymentSettings.UseSandbox = model.UseSandbox;
            gtPayPaymentSettings.ShowGTPayPage = model.ShowGTPayPage;
            gtPayPaymentSettings.MerchantId = model.MerchantId;
            gtPayPaymentSettings.MerchantHashKey = model.MerchantHashKey;
            gtPayPaymentSettings.PreferredGateway = model.PreferredGateway;
            gtPayPaymentSettings.ShowCustomerName = model.ShowCustomerName;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(gtPayPaymentSettings, x => x.SkipPaymentInfo, model.SkipPaymentInfo_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(gtPayPaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(gtPayPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(gtPayPaymentSettings, x => x.ShowGTPayPage, model.ShowGTPayPage_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(gtPayPaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(gtPayPaymentSettings, x => x.MerchantHashKey, model.MerchantHashKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(gtPayPaymentSettings, x => x.PreferredGateway, model.PreferredGateway_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(gtPayPaymentSettings, x => x.ShowCustomerName, model.ShowCustomerName_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var gtPayPaymentSettings = _settingService.LoadSetting<GTPayPaymentSettings>(storeScope);

            var model = new PaymentInfoModel();
            model.DescriptionText = gtPayPaymentSettings.DescriptionText;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.DescriptionText_OverrideForStore = _settingService.SettingExists(gtPayPaymentSettings, x => x.DescriptionText, storeScope);
            }

            return View("~/Plugins/Payments.GTPay/Views/PaymentGTPay/PaymentInfo.cshtml", model);
        }

        public ActionResult SubmitPaymentInfo(int orderId)
        {
            Order order = _orderService.GetOrderById(orderId);
            var sb = new StringBuilder();
            var now = DateTime.UtcNow;

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var gtPayPaymentSettings = _settingService.LoadSetting<GTPayPaymentSettings>(storeScope);
            var currencySettings = _settingService.LoadSetting<CurrencySettings>(storeScope);
            var currency = _currencyService.GetCurrencyById(currencySettings.PrimaryStoreCurrencyId);
            var test_mert_id = GTPayHelper.GetGTPayTestMerchantIdentifier();

            string tranx_id = $"{now.Ticks}/{order.OrderGuid}";
            string tranx_amt = (order.OrderTotal * 100).ToString("F0");//Convert to kobo or cents
            string cust_id = order.CustomerId.ToString();
            string tranx_noti_url = Url.Action("ReturnPaymentInfo", "PaymentGTPay", null, Request.Url.Scheme, null);
            string tranx_curr = GTPayHelper.GetCurrencyISOCode(currency.CurrencyCode);
            string mert_id = !gtPayPaymentSettings.UseSandbox ? gtPayPaymentSettings.MerchantId : test_mert_id;

            string tranRequestUrl = GTPayHelper.GetGTPayTransactionRequestUrl();
            if (gtPayPaymentSettings.UseSandbox)
                tranRequestUrl = GTPayHelper.GetTestGTPayTransactionRequestUrl();

            sb.AppendLine("<html>");
            sb.AppendLine("<body onload=\"document.submit2gtpay_form.submit()\">");
            sb.AppendLine($"<form name=\"submit2gtpay_form\" action=\"{tranRequestUrl}\" target=\"_self\" method=\"post\">");
            sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_mert_id\" value=\"{mert_id}\" />");
            sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_tranx_id\" value=\"{tranx_id}\" />");
            sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_tranx_amt\" value=\"{tranx_amt}\" />");
            sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_tranx_curr\" value=\"{tranx_curr}\" />");
            sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_cust_id\" value=\"{cust_id}\" />");
            if (gtPayPaymentSettings.ShowCustomerName)
            {
                var customer = _customerService.GetCustomerById(order.CustomerId);
                sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_cust_name\" value=\"{customer.GetFullName()}\" />");
            }
            sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_tranx_memo\" value=\"Purchase from {_storeContext.CurrentStore.Name}.\" />");
            sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_tranx_noti_url\" value=\"{tranx_noti_url}\" />");
            if (!string.IsNullOrWhiteSpace(gtPayPaymentSettings.PreferredGateway))
            {
                sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_gway_name\" value=\"{gtPayPaymentSettings.PreferredGateway}\" />");
            }
            sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_no_show_gtbank\" value=\"yes\" />");
            sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_echo_data\" value=\"orderId:{order.Id},orderGuid:{order.OrderGuid},customerId:{order.CustomerId}\" />");
            if (gtPayPaymentSettings.ShowGTPayPage && !string.IsNullOrWhiteSpace(gtPayPaymentSettings.PreferredGateway))
            {
                sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_gway_first\" value=\"yes\" />");
            }
            if (!gtPayPaymentSettings.UseSandbox)
            {
                //compute gtpay_hash here
                string hash = _encryptionService.CreatePasswordHash(
                    password: $"{mert_id}{tranx_id}{tranx_amt}{tranx_curr}{cust_id}{tranx_noti_url}",
                    saltkey: gtPayPaymentSettings.MerchantHashKey,
                    passwordFormat: "SHA512");

                sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_hash\" value=\"{hash}\" />");
            }

            sb.AppendLine("</form>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            string content = sb.ToString();
            _logger.Information($"GTPay Request: {content}");
            return Content(content);
        }

        public ActionResult ReturnPaymentInfo(FormCollection form)
        {
            string tranx_id = form["gtpay_tranx_id"];
            string tranx_status_code = form["gtpay_tranx_status_code"];
            string tranx_status_msg = form["gtpay_tranx_status_msg"];

            var orderGuid = Guid.Parse(tranx_id?.Split('/')[1]);
            Order order = _orderService.GetOrderByGuid(orderGuid);

            if (!string.Equals(tranx_status_code, "00", StringComparison.InvariantCultureIgnoreCase))
            {
                var model = new ReturnPaymentInfoModel();
                model.DescriptionText = "Your transaction was unsuccessful.";
                model.OrderId = order.Id;
                model.StatusCode = tranx_status_code;
                model.StatusMessage = tranx_status_msg;

                return View("~/Plugins/Payments.GTPay/Views/PaymentGTPay/ReturnPaymentInfo.cshtml", model);
            }

            order.PaymentStatus = PaymentStatus.Paid;
            _orderService.UpdateOrder(order);

            return RedirectToAction("Completed", "Checkout");
        }


        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        #endregion
    }
}