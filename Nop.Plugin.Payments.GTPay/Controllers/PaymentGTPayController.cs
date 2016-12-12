using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.GTPay.Models;
using Nop.Plugin.Payments.GTPay.Services;
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

        #region Utils

        private void LoadAvailableGateWays(ConfigurationModel model)
        {
            model.AvailableGateways.Clear();
            model.AvailableGateways.Add(new SelectListItem() { Text = "", Value = "" });
            model.AvailableGateways.Add(new SelectListItem() { Text = "WebPay", Value = "webpay" });
            model.AvailableGateways.Add(new SelectListItem() { Text = "Mastercard International Gateway", Value = "migs" });
        }

        #endregion

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var gtPayPaymentSettings = _settingService.LoadSetting<GTPayPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
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

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var gtPayPaymentSettings = _settingService.LoadSetting<GTPayPaymentSettings>(storeScope);
            var currencySettings = _settingService.LoadSetting<CurrencySettings>(storeScope);

            string merchantId = gtPayPaymentSettings.MerchantId;
            if (gtPayPaymentSettings.UseSandbox)
                merchantId = GTPayHelper.GetGTPayTestMerchantIdentifier();

            string transactionId = order.OrderGuid.ToString();
            string transactionAmount = (order.OrderTotal * (decimal)100).ToString();
            string customerId = order.CustomerId.ToString();           


            var pryCurrency = _currencyService.GetCurrencyById(currencySettings.PrimaryStoreCurrencyId);
            string currencyISOCode = GTPayHelper.GetCurrencyISOCode(pryCurrency.CurrencyCode);
            string gtPayCallbackUrl = Url.Action("ReturnPaymentInfo", "PaymentGTPay", null, Request.Url.Scheme, null);

            sb.AppendLine("<html>");
            sb.AppendLine("<body onload=\"document.submit2gtpay_form.submit()\">");
            sb.AppendLine($"<form name=\"submit2gtpay_form\" action=\"{GTPayHelper.GetGTPayTransactionRequestUrl()}\" target=\"_self\" method=\"post\">");
            sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_mert_id\" value=\"{merchantId}\" />");
            sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_tranx_id\" value=\"{transactionId}\" />");
            sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_tranx_amt\" value=\"{transactionAmount}\" />"); //Convert to kobo or cents
            sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_tranx_curr\" value=\"{currencyISOCode}\" />");
            sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_cust_id\" value=\"{order.CustomerId}\" />");
            if (gtPayPaymentSettings.ShowCustomerName)
            {
                var customer = _customerService.GetCustomerById(order.CustomerId);
                sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_cust_name\" value=\"{customer.GetFullName()}\" />");
            }
            sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_tranx_memo\" value=\"Purchase from {_storeContext.CurrentStore.Name}.\" />");
            sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_tranx_noti_url\" value=\"{gtPayCallbackUrl}\" />");
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
                //compute hash here
                string hashValue = _encryptionService.GenerateGTPayHash(gtPayPaymentSettings: gtPayPaymentSettings,
                    transactionId: transactionId,
                    transactionAmount: transactionAmount,
                    currencyISOCode: currencyISOCode,
                    customerId: customerId);

                sb.AppendLine($"<input type=\"hidden\" name=\"gtpay_tranx_hash\" value=\"{hashValue}\" />");
            }

            sb.AppendLine("</form>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            string content = sb.ToString();
            return Content(content);
        }

        public ActionResult ReturnPaymentInfo(FormCollection form)
        {
            string transactionId = form["gtpay_tranx_id"].ToString();
            string statusCode = form["gtpay_tranx_status_code"].ToString();
            string statusMessage = form["gtpay_tranx_status_msg"].ToString();

            Order order = _orderService.GetOrderByGuid(Guid.Parse(transactionId));

            if (!string.Equals(statusCode, "00", StringComparison.InvariantCultureIgnoreCase))
            {
                //order.PaymentStatus = Core.Domain.Payments.PaymentStatus.Voided;
                //order.OrderStatus = OrderStatus.Pending;
                //_orderService.UpdateOrder(order);

                var model = new ReturnPaymentInfoModel();
                model.DescriptionText = "Your transaction was unsuccessful.";
                model.OrderId = order.Id;
                model.StatusCode = statusCode;
                model.StatusMessage = statusMessage;

                return View("~/Plugins/Payments.GTPay/Views/PaymentGTPay/ReturnPaymentInfo.cshtml", model);
            }

            order.PaymentStatus = PaymentStatus.Paid;
            //if (order.OrderStatus == OrderStatus.Pending)
            //    order.OrderStatus = OrderStatus.Processing;
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


    }
}