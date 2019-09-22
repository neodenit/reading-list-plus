using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using ReadingListPlus.Common;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.Services;
using ReadingListPlus.Services.Attributes;
using ReadingListPlus.Services.ViewModels;
using ReadingListPlus.Web.Core.Pages.Decks;

namespace ReadingListPlus.Web.Core.Pages.Cards
{
    public class CardCreateModel : PageModel
    {
        public const string PageName = "/Cards/Create";

        private readonly ISettings settings;
        private readonly ICardService cardService;
        private readonly IDeckService deckService;

        public CardCreateModel(ISettings settings, ICardService cardService, IDeckService deckService)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            this.deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
        }

        public async Task<ActionResult> OnGetAsync([DeckFound, DeckOwned]Guid? deckId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (TempData.TryGetValue(nameof(CreateCardViewModel), out var viewModel))
            {
                var viewModelJson = viewModel as string;
                Card = JsonConvert.DeserializeObject<CreateCardViewModel>(viewModelJson);
            }
            else
            {
                DeckViewModel deck = await deckService.GetDeckAsync(deckId.Value);

                IEnumerable<KeyValuePair<string, string>> priorities = cardService.GetFullPriorityList();

                Card = new CreateCardViewModel
                {
                    DeckID = deck.ID,
                    DeckTitle = deck.Title,
                    PriorityList = priorities,
                    Type = CardType.Common,
                    CreationMode = CreationMode.Add
                };
            }

            return Page();
        }

        [BindProperty]
        public CreateCardViewModel Card { get; set; }

        public async Task<ActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                Guid newCardDeckId = await cardService.AddAsync(Card);

                await deckService.SetUserLastDeckAsync(User.Identity.Name, newCardDeckId);

                return Card.CreationMode == CreationMode.Extract
                    ? RedirectToPage(DeckDetailsModel.PageName, new { Id = Card.OldDeckID ?? newCardDeckId })
                    : RedirectToPage(CardIndexModel.PageName, new { DeckId = newCardDeckId });
            }
            else
            {
                Card.DeckListItems = settings.AllowDeckSelection && Card.CreationMode != CreationMode.Add
                    ? await deckService
                            .GetUserDecks(User.Identity.Name)
                            .OrderBy(d => d.Title)
                            .ToList()
                    : null;

                Card.PriorityList = Card.CreationMode == CreationMode.Extract
                    ? cardService.GetShortPriorityList()
                    : cardService.GetFullPriorityList();

                return Page();
            }
        }
    }
}