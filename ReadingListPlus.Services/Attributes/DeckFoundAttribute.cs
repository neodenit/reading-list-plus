using System;
using System.ComponentModel.DataAnnotations;
using ReadingListPlus.Repositories;

namespace ReadingListPlus.Services.Attributes
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

            var deckRepository = validationContext.GetService(typeof(IDeckRepository)) as IDeckRepository;

            var deckId = (Guid)value;

            var deck = deckRepository.GetDeck(deckId);

            var validationResult = deck == null ? FailedValidationResult : ValidationResult.Success;

            return validationResult;
        }
    }
}
