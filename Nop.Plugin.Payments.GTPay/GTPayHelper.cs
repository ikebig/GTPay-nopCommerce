using System;
using System.Linq;

namespace Nop.Plugin.Payments.GTPay
{
    public static class GTPayHelper
    {
        public static string[] GetSupportedCurrencyCodes()
        {
            return new string[] { "NGN", "USD" };
        }
        public static string GetCurrencyISOCode(string currencyCode)
        {
            var supportedCurrencyCodes = GetSupportedCurrencyCodes();
            if (!supportedCurrencyCodes.Contains(currencyCode))
                throw new ArgumentException("Unsupported currency code.");

            string result = string.Empty;
            switch (currencyCode)
            {
                case "USD":
                    result = "826";
                    break;
                default:
                    result = "566";
                    break;
            }

            return result;
        }

        public static string GetGTPayTransactionRequestUrl()
        {
            return "https://ibank.gtbank.com/GTPay/Tranx.aspx";
        }

        public static string GetGTPayTestMerchantIdentifier()
        {
            return "GT-Merchant";
        }
    }
}
