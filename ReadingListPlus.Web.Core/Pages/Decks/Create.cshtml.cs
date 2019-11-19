using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Common.App_GlobalResources;
using ReadingListPlus.Services;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Web.Core.Pages.Decks
{
    [Authorize]
    public class DeckCreateModel : PageModel
    {
        public const string PageName = "/Decks/Create";

        private readonly IDeckService deckService;

        public DeckCreateModel(IDeckService deckService)
        {
            this.deckService = deckService ?? throw new System.ArgumentNullException(nameof(deckService));
        }

        public void OnGet(string returnUrl)
        {
            ReturnUrl = returnUrl;
            Deck = new CreateDeckViewModel();
        }

        [BindProperty]
        public string ReturnUrl { get; set; }

        [BindProperty]
        public CreateDeckViewModel Deck { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var title = Deck.Title.Trim();
            var decks = await deckService.GetUserDecksAsync(User.Identity.Name);

            if (decks.Any(d => d.Title == title))
            {
                ModelState.AddModelError(string.Empty, $"{Resources.Collection} '{title}' already exists");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            await deckService.CreateDeckAsync(title, User.Identity.Name);

            return string.IsNullOrEmpty(ReturnUrl)
                ? RedirectToPage(DeckIndexModel.PageName) as IActionResult
                : Redirect(ReturnUrl);
        }
    }
}