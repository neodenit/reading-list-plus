using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Common;
using ReadingListPlus.Services;
using ReadingListPlus.Services.Attributes;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Web.Core.Pages.Cards
{
    public class CardIndexModel : PageModel
    {
        public const string PageName = "/Cards/Index";
        private readonly ISettings settings;
        private readonly ICardService cardService;

        public CardIndexModel(ISettings settings, ICardService cardService)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
        }

        public async Task<ActionResult> OnGetAsync([Required, DeckFound, DeckOwned]Guid? deckId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IEnumerable<CardViewModel> cards = settings.ShowHiddenCardsInIndex
                ? await cardService.GetAllCardsAsync(deckId.Value)
                : await cardService.GetConnectedCardsAsync(deckId.Value);

            Cards = cards
                .OrderByDescending(c => c.IsConnected)
                .ThenBy(c => c.Position)
                .ThenBy(c => c.Text);

            return Page();
        }

        public IEnumerable<CardViewModel> Cards { get; set; }
    }
}