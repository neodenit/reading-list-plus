using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Services;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Web.Core.Pages.Decks
{
    [Authorize]
    public class DeckIndexModel : PageModel
    {
        public const string PageName = "/Decks/Index";

        private readonly IDeckService deckService;

        public IEnumerable<DeckViewModel> Decks { get; set; }

        public DeckIndexModel(IDeckService deckService)
        {
            this.deckService = deckService ?? throw new System.ArgumentNullException(nameof(deckService));
        }

        public async Task OnGet()
        {
            Decks = await deckService.GetUserDecksAsync(User.Identity.Name);
        }
    }
}