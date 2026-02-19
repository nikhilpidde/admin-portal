using System.ComponentModel.DataAnnotations;

using DTPortal.Web.ViewModel.RateCard;

//namespace DTPortal.Web.CustomValidations
//{
//    public class RateCardValidityAttribute : ValidationAttribute //, IClientModelValidator
//    {
//        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
//        {
//            RateCardAddViewModel rateCard = (RateCardAddViewModel)validationContext.ObjectInstance;

//            if (rateCard.RateEffectiveFrom > rateCard.ValidTo)
//            {
//                return new ValidationResult("Valid From must not be greater than Valid To");
//            }

//            return ValidationResult.Success;
//        }

//        //public void AddValidation(ClientModelValidationContext context)
//        //{
//        //    context.Attributes.Add("data-val", "true");
//        //    context.Attributes.Add("data-val-ratecardvalidity", "Valid From must not be greater than Valid To");
//        //}
//    }
//}
