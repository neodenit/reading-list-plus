using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Services;
using ReadingListPlus.Services.Attributes;
using ReadingListPlus.Web.Core.Controllers;

namespace ReadingListPlus.Web.Core.Pages.Decks
{
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
                ? RedirectToAction(nameof(CardsController.Create), CardsController.Name, new { DeckId = id })
                : RedirectToAction(nameof(CardsController.Details), CardsController.Name, new { Id = cardId });

        }
    }
}