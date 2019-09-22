using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Services;
using ReadingListPlus.Services.Attributes;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Web.Core.Pages.Cards
{
    public class CardIndexModel : PageModel
    {
        public const string PageName = "/Cards/Index";

        private readonly ICardService cardService;

        public CardIndexModel(ICardService cardService)
        {
            this.cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
        }

        public async Task<ActionResult> OnGetAsync([Required, DeckFound, DeckOwned]Guid? deckId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IEnumerable<CardViewModel> cards = await cardService.GetConnectedCards(deckId.Value);

            Cards = cards.OrderBy(c => c.Position);

            return Page();
        }

        public IEnumerable<CardViewModel> Cards { get; set; }
    }
}