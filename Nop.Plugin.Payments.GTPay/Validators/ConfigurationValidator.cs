using FluentValidation;
using Nop.Plugin.Payments.GTPay.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.GTPay.Validators
{
    public partial class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
    {
        public ConfigurationValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.DescriptionText).NotEmpty()
               .WithMessage(localizationService.GetResource(Constants.LocaleResources.GTPay_Fields_DescriptionText_Required));
            RuleFor(x => x.MerchantId).NotEmpty()
                .When(x => !x.UseSandbox)
                .WithMessage(localizationService.GetResource(Constants.LocaleResources.GTPay_Fields_MerchantId_Required));
            RuleFor(x => x.MerchantHashKey).NotEmpty()
                .When(x => !x.UseSandbox)
                .WithMessage(localizationService.GetResource(Constants.LocaleResources.GTPay_Fields_MerchantHashKey_Required));
        }
    }
}

