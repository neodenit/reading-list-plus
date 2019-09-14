using System;
using System.ComponentModel.DataAnnotations;
using ReadingListPlus.Repositories;

namespace ReadingListPlus.Services.Attributes
{
    public class CardFoundAttribute : ValidationAttribute
    {
        private readonly ValidationResult FailedValidationResult = new ValidationResult("NotFound");
        private readonly ValidationResult DefaultValidationResult = ValidationResult.Success;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || !(value is Guid))
            {
                return DefaultValidationResult;
            }

            var cardRepository = validationContext.GetService(typeof(ICardRepository)) as ICardRepository;

            var cardId = (Guid)value;

            var card = cardRepository.GetCard(cardId);

            var validationResult = card == null ? FailedValidationResult : ValidationResult.Success;

            return validationResult;
        }
    }
}
