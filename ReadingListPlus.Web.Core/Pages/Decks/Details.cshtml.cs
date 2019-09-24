using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Services;
using ReadingListPlus.Services.Attributes;
using ReadingListPlus.Web.Core.Pages.Cards;

namespace ReadingListPlus.Web.Core.Pages.Decks
{
    [Authorize]
    public class DeckDetailsModel : PageModel
    {
        public const string PageName = "/Decks/Details";

        private readonly IDeckService deckService;

        public DeckDetailsModel(IDeckService deckService)
        {
            this.deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
        }

        public async Task<ActionResult> OnGetAsync([Required, DeckFound, DeckOwned]Guid? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Guid cardId = await deckService.GetFirstCardIdOrDefaultAsync(id.Value);

            return cardId == Guid.Empty
                ? RedirectToPage(CardCreateModel.PageName, new { DeckId = id })
                : RedirectToPage(CardReadModel.PageName, new { Id = cardId });

        }
    }
}