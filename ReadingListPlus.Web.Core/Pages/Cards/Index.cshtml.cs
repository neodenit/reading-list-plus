using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Common;
using ReadingListPlus.Common.App_GlobalResources;
using ReadingListPlus.Services;
using ReadingListPlus.Services.Attributes;
using ReadingListPlus.Services.ViewModels;
using ReadingListPlus.Web.Core.Pages.Decks;

namespace ReadingListPlus.Web.Core.Pages.Cards
{
    [Authorize]
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

        public async Task<ActionResult> OnGetAsync([DeckFound, DeckOwned]Guid? deckId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (deckId == null)
            {
                IAsyncEnumerable<CardViewModel> cards = cardService.GetUnparentedCardsAsync(User.Identity.Name);

                var cardList = new List<CardViewModel>();

                await foreach (var card in cards)
                {
                    cardList.Add(card);
                }

                if (!cardList.Any())
                {
                    return RedirectToPage(DeckIndexModel.PageName);
                }

                Cards = cardList.OrderBy(c => c.DisplayText);
            }
            else
            {
                IEnumerable<CardViewModel> cards = settings.ShowHiddenCardsInIndex
                    ? await cardService.GetAllCardsAsync(deckId.Value)
                    : await cardService.GetConnectedCardsAsync(deckId.Value);

                if (!cards.Any())
                {
                    return RedirectToPage(DeckIndexModel.PageName);
                }

                Cards = cards
                    .OrderByDescending(c => c.IsConnected)
                    .ThenBy(c => c.Position)
                    .ThenBy(c => c.DisplayText);

            }

            return Page();
        }

        public IEnumerable<CardViewModel> Cards { get; set; }
    }
}