using System.ComponentModel.DataAnnotations;

using DTPortal.Web.ViewModel.Reports;

namespace DTPortal.Web.CustomValidations
{
    public class StartDateNotGreaterThanEndDateAttribute : ValidationAttribute //, IClientModelValidator
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ReportsSearchViewModel reportViewModel = (ReportsSearchViewModel)validationContext.ObjectInstance;

            if (reportViewModel.StartDate > reportViewModel.EndDate)
            {
                return new ValidationResult("Start date must not be greater than End Date");
            }

            return ValidationResult.Success;
        }

        //public void AddValidation(ClientModelValidationContext context)
        //{
        //    context.Attributes.Add("data-val", "true");
        //    context.Attributes.Add("data-val-ratecardvalidity", "Valid From must not be greater than Valid To");
        //}
    }
}
