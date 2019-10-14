using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using ReadingListPlus.Common;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.Services;
using ReadingListPlus.Services.Attributes;
using ReadingListPlus.Services.ViewModels;
using ReadingListPlus.Web.Core.Controllers;

namespace ReadingListPlus.Web.Core.Pages.Cards
{
    [Authorize]
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

                DeckListItems = settings.AllowDeckSelection && Card.CreationMode != CreationMode.Add
                    ? await deckService.GetUserDecks(User.Identity.Name).ToList()
                    : null;

                PriorityList = Card.CreationMode == CreationMode.Extract
                    ? cardService.GetShortPriorityList()
                    : cardService.GetFullPriorityList();
            }
            else
            {
                DeckViewModel deck = await deckService.GetDeckAsync(deckId.Value);

                IEnumerable<KeyValuePair<string, string>> priorities = cardService.GetFullPriorityList();

                Card = new CreateCardViewModel
                {
                    DeckID = deck.ID,
                    DeckTitle = deck.Title,
                    Type = CardType.Common,
                    CreationMode = CreationMode.Add
                };
            }

            return Page();
        }

        [BindProperty]
        public CreateCardViewModel Card { get; set; }

        public IEnumerable<DeckViewModel> DeckListItems { get; set; }

        public IEnumerable<KeyValuePair<string, string>> PriorityList { get; set; }

        public async Task<ActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                Guid newCardDeckId = await cardService.AddAsync(Card);

                if (Card.CreationMode == CreationMode.FromUrl)
                {
                    await deckService.SetUserLastDeckAsync(User.Identity.Name, newCardDeckId);
                }

                return Card.CreationMode == CreationMode.Extract
                    ? RedirectToAction(nameof(DecksController.Read), DecksController.Name, new { Id = Card.OldDeckID }) as ActionResult
                    : RedirectToPage(CardIndexModel.PageName, new { DeckId = newCardDeckId });
            }
            else
            {
                DeckListItems = settings.AllowDeckSelection && Card.CreationMode != CreationMode.Add
                    ? await deckService.GetUserDecks(User.Identity.Name).ToList()
                    : null;

                PriorityList = Card.CreationMode == CreationMode.Extract
                    ? cardService.GetShortPriorityList()
                    : cardService.GetFullPriorityList();

                return Page();
            }
        }
    }
}