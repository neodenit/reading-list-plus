using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ReadingListPlus.Repositories;

namespace ReadingListPlus.Services.Attributes
{
    public class DeckOwnedAttribute : ValidationAttribute
    {
        private readonly ValidationResult FailedValidationResult = new ValidationResult("Unauthorized");
        private readonly ValidationResult DefaultValidationResult = ValidationResult.Success;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || !(value is Guid))
            {
                return DefaultValidationResult;
            }

            var deckRepository = validationContext.GetService(typeof(IDeckRepository)) as IDeckRepository;
            var httpContextAccessor = validationContext.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
            var userName = httpContextAccessor.HttpContext.User.Identity.Name;

            var deckId = (Guid)value;
            var deck = deckRepository.GetDeck(deckId);

            if (deck == null)
            {
                return DefaultValidationResult;
            }

            var validationResult = deck.OwnerID == userName ? ValidationResult.Success : FailedValidationResult;

            return validationResult;
        }
    }
}
