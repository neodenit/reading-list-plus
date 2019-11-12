using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ReadingListPlus.Repositories;

namespace ReadingListPlus.Services.Attributes
{
    public class CardOwnedAttribute : ValidationAttribute
    {
        private readonly ValidationResult FailedValidationResult = new ValidationResult("Unauthorized");
        private readonly ValidationResult DefaultValidationResult = ValidationResult.Success;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || !(value is Guid))
            {
                return DefaultValidationResult;
            }

            var cardRepository = validationContext.GetService(typeof(ICardRepository)) as ICardRepository;
            var httpContextAccessor = validationContext.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
            var userName = httpContextAccessor.HttpContext.User.Identity.Name;

            var cardId = (Guid)value;
            var card = cardRepository.GetCard(cardId);

            if (card == null)
            {
                return DefaultValidationResult;
            }

            var validationResult = card.OwnerID == userName ? ValidationResult.Success : FailedValidationResult;

            return validationResult;
        }
    }
}
