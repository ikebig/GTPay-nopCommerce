using Nop.Services.Security;

namespace Nop.Plugin.Payments.GTPay.Services
{
    public static class EncryptionServiceExtensions
    {
        public static string GenerateGTPayHash(this IEncryptionService encryptionService, GTPayPaymentSettings gtPayPaymentSettings,
            string transactionId, string transactionAmount, string currencyISOCode, string customerId)
        {
            string password = gtPayPaymentSettings.MerchantId + 
                transactionId + 
                transactionAmount + 
                currencyISOCode +
                customerId + 
                GTPayHelper.GetGTPayTransactionRequestUrl() +
                gtPayPaymentSettings.MerchantHashKey;

            return encryptionService.CreatePasswordHash(password, gtPayPaymentSettings.MerchantHashKey, "SHA512");
        }
    }
}
