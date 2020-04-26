using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Common.App_GlobalResources;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.Services;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Web.Core.Pages.Decks
{
    [Authorize]
    public class DeckIndexModel : PageModel
    {
        public const string PageName = "/Decks/Index";

        private readonly IDeckService deckService;
        private readonly ICardService cardService;

        public IEnumerable<DeckViewModel> Decks { get; set; }

        public DeckIndexModel(IDeckService deckService, ICardService cardService)
        {
            this.deckService = deckService ?? throw new System.ArgumentNullException(nameof(deckService));
            this.cardService = cardService ?? throw new System.ArgumentNullException(nameof(cardService));
        }

        public async Task OnGet()
        {
            IAsyncEnumerable<CardViewModel> cards = cardService.GetUnparentedCardsAsync(User.Identity.Name);
            var cardList = await cards.ToListAsync();

            IEnumerable<DeckViewModel> userDecks = await deckService.GetUserDecksAsync(User.Identity.Name);

            var unparentedDeck = new DeckViewModel
            {
                Title = $"No {Resources.Collection}",
                CardCount = cardList.Count(),
                ArticleCount = cardList.Count(c => c.CardType == CardType.Article),
                ExtractCount = cardList.Count(c => c.CardType == CardType.Extract)
            };

            Decks = userDecks.Append(unparentedDeck);
        }
    }
}