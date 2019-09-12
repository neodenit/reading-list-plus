using System;
using System.ComponentModel.DataAnnotations;
using ReadingListPlus.DataAccess;

namespace ReadingListPlus.Web.Core.Attributes
{
    public class DeckFoundAttribute : ValidationAttribute
    {
        private readonly ValidationResult FailedValidationResult = new ValidationResult("NotFound");
        private readonly ValidationResult DefaultValidationResult = ValidationResult.Success;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || !(value is Guid))
            {
                return DefaultValidationResult;
            }

            var dbContext = validationContext.GetService(typeof(IApplicationContext)) as IApplicationContext;

            var deckId = (Guid)value;

            var deck = dbContext.GetDeck(deckId);

            var validationResult = deck == null ? FailedValidationResult : ValidationResult.Success;

            return validationResult;
        }
    }
}
