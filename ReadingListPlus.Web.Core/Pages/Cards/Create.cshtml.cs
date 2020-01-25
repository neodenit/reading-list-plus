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
using ReadingListPlus.Web.Core.Pages.Decks;

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

        public async Task<ActionResult> OnGetAsync([DeckFound, DeckOwned]Guid? deckId, string text)
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
            else if (!string.IsNullOrEmpty(text))
            {
                Card = new CreateCardViewModel
                {
                    CardType = CardType.Common,
                    CreationMode = CreationMode.FromUrl,
                    Text = text
                };
            }
            else
            {
                DeckViewModel deck = await deckService.GetDeckAsync(deckId.Value);

                Card = new CreateCardViewModel
                {
                    DeckID = deck.ID,
                    DeckTitle = deck.Title,
                    CardType = CardType.Common,
                    CreationMode = CreationMode.Add,
                    Text = text
                };
            }

            AllowDeckSelection = settings.AllowDeckSelection && deckId == null;

            DeckListItems = AllowDeckSelection
                ? await deckService.GetUserDecksAsync(User.Identity.Name)
                : null;

            if (ShowPriorities)
            {
                PriorityList = Card.CreationMode == CreationMode.Extract || !settings.AllowHighestPriority
                    ? cardService.GetShortPriorityList()
                    : cardService.GetFullPriorityList();
            }
            else
            {
                Card.Priority = DefaultPriority;
            }

            return Page();
        }

        public bool ShowPriorities =>
            Card.CreationMode switch
            {
                CreationMode.FromUrl => settings.EnableArticlePrioritySelection,
                CreationMode.Extract => settings.EnableExtractPrioritySelection,
                CreationMode.Add => settings.EnableCommonNotePrioritySelection,
                _ => false
            };

        public Priority DefaultPriority =>
            Card.CreationMode switch
            {
                CreationMode.FromUrl => settings.DefaultArticlePriority,
                CreationMode.Extract => settings.DefaultExtractPriority,
                CreationMode.Add => settings.DefaultCommonNotePriority,
                _ => Priority.Low
            };

        [BindProperty]
        public CreateCardViewModel Card { get; set; }

        [BindProperty]
        public bool AllowDeckSelection { get; set; }

        public IEnumerable<DeckViewModel> DeckListItems { get; set; }

        public IEnumerable<KeyValuePair<string, string>> PriorityList { get; set; }

        public async Task<ActionResult> OnPostAsync(CreateAction action)
        {
            switch (action)
            {
                case CreateAction.CreateCard:
                    IEnumerable<string> invalidTagNames = cardService.ValidateTagNames(Card.Text);

                    if (invalidTagNames.Any())
                    {
                        foreach (var name in invalidTagNames)
                        {
                            ModelState.AddModelError(string.Empty, $"Invalid name: {name}");
                        }
                    }
                    else if (ModelState.IsValid)
                    {
                        if (!ShowPriorities)
                        {
                            Card.Priority = DefaultPriority;
                        }

                        Guid newCardDeckId = await cardService.AddAsync(Card, User.Identity.Name);

                        if (Card.CreationMode == CreationMode.FromUrl)
                        {
                            await deckService.SetUserLastDeckAsync(User.Identity.Name, newCardDeckId);
                        }

                        return Card.CreationMode == CreationMode.Extract
                            ? RedirectToPage(CardReadModel.PageName, new { Id = Card.ParentCardID, IsBookmarked = true })
                            : RedirectToPage(CardIndexModel.PageName, new { DeckId = newCardDeckId });
                    }

                    break;
                case CreateAction.CreateDeck:
                    TempData[nameof(CreateCardViewModel)] = JsonConvert.SerializeObject(Card);
                    return RedirectToPage(DeckCreateModel.PageName, new { ReturnUrl = Url.Page(CardCreateModel.PageName) });
            }

            DeckListItems = AllowDeckSelection
                ? await deckService.GetUserDecksAsync(User.Identity.Name)
                : null;

            if (ShowPriorities)
            {
                PriorityList = Card.CreationMode == CreationMode.Extract || !settings.AllowHighestPriority
                ? cardService.GetShortPriorityList()
                : cardService.GetFullPriorityList();
            }
            else
            {
                Card.Priority = DefaultPriority;
            }

            return Page();
        }
    }
}