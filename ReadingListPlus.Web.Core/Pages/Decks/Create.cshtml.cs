using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Services;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Web.Core.Pages.Decks
{
    public class DeckCreateModel : PageModel
    {
        public const string PageName = "/Decks/Create";

        private readonly IDeckService deckService;

        public DeckCreateModel(IDeckService deckService)
        {
            this.deckService = deckService ?? throw new System.ArgumentNullException(nameof(deckService));
        }

        public void OnGet()
        {
            Deck = new CreateDeckViewModel();
        }

        [BindProperty]
        public CreateDeckViewModel Deck { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await deckService.CreateDeckAsync(Deck.Title, User.Identity.Name);

            return RedirectToPage(DeckIndexModel.PageName);
        }
    }
}