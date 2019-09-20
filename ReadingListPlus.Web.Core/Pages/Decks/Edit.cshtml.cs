using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Services;
using ReadingListPlus.Services.Attributes;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Web.Core.Pages.Decks
{
    public class DeckEditModel : PageModel
    {
        public const string PageName = "/Decks/Edit";

        private readonly IDeckService deckService;

        public DeckEditModel(IDeckService deckService)
        {
            this.deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
        }

        [BindProperty]
        public DeckViewModel Deck { get; set; }

        public async Task<ActionResult> OnGetAsync([Required, DeckFound, DeckOwned]Guid? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Deck = await deckService.GetDeckAsync(id.Value);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await deckService.UpdateDeckTitleAsync(Deck.ID, Deck.Title);
            return RedirectToPage(DeckIndexModel.PageName);
        }
    }
}
