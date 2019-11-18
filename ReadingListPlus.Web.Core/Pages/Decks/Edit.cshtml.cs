using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Common.App_GlobalResources;
using ReadingListPlus.Services;
using ReadingListPlus.Services.Attributes;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Web.Core.Pages.Decks
{
    [Authorize]
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
            var title = Deck.Title.Trim();
            var deckId = Deck.ID;
            var decks = await deckService.GetUserDecksAsync(User.Identity.Name);

            if (decks.Any(d => d.Title == title && d.ID != deckId))
            {
                ModelState.AddModelError(string.Empty, $"{Resources.Collection} '{title}' already exists");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            await deckService.UpdateDeckTitleAsync(Deck.ID, Deck.Title);
            return RedirectToPage(DeckIndexModel.PageName);
        }
    }
}
