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
    public class DeckDeleteModel : PageModel
    {
        public const string PageName = "/Decks/Delete";

        private readonly IDeckService deckService;

        public DeckDeleteModel(IDeckService deckService)
        {
            this.deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
        }

        [BindProperty]
        public DeckViewModel Deck { get; set; }

        public async Task<IActionResult> OnGetAsync([Required, DeckFound, DeckOwned]Guid? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Deck = await deckService.GetDeckAsync(id.Value);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync([Required, DeckFound, DeckOwned]Guid? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await deckService.DeleteDeckAsync(id.Value);

            return RedirectToPage(DeckIndexModel.PageName);
        }
    }
}
