using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ReadingListPlus.Common;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.Services;
using ReadingListPlus.Services.ArticleExtractorService;
using ReadingListPlus.Services.Attributes;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Web.Core.Controllers
{
    [Authorize]
    public class CardsController : Controller
    {
        public const string Name = "Cards";

        private readonly ISettings settings;
        private readonly IDeckService deckService;
        private readonly ICardService cardService;
        private readonly IArticleExtractorService articleExtractor;
        private readonly IRepetitionCardService repetitionCardService;

        private string UserName => User.Identity.Name;

        public CardsController(ISettings settings, IDeckService deckService, ICardService cardService, IArticleExtractorService articleExtractor, IRepetitionCardService repetitionCardService)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
            this.cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            this.articleExtractor = articleExtractor ?? throw new ArgumentException(nameof(articleExtractor));
            this.repetitionCardService = repetitionCardService ?? throw new ArgumentNullException(nameof(repetitionCardService));
        }

        public async Task<ActionResult> Index([Required, DeckFound, DeckOwned]Guid? deckId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IEnumerable<CardViewModel> cards = await cardService.GetConnectedCards(deckId.Value);
            var orderedCards = cards.OrderBy(c => c.Position);
            return View(orderedCards);
        }

        [Authorize(Policy = Constants.FixPolicy)]
        public async Task<ActionResult> Fix(Guid deckId)
        {
            await deckService.FixDeckAsync(deckId);
            return RedirectToAction(nameof(Index), new { deckId });
        }

        public async Task<ActionResult> Details([Required, CardFound, CardOwned]Guid? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CardViewModel viewModel = await cardService.GetCardForReadingAsync(id.Value);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Details([Bind("ID", "NextAction", "Selection", "Priority")] CardViewModel card)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool isActionValid = await repetitionCardService.IsActionValidAsync(card.ID, card.NextAction);

            if (!isActionValid)
            {
                return BadRequest();
            }

            switch (card.NextAction)
            {
                case "Extract":
                    ModelState.Clear();
                    return View(nameof(Create), await cardService.ExtractAsync(card.ID, card.Selection, UserName));
                case "Cloze":
                    return RedirectToAction(nameof(Details), new { id = await cardService.ClozeAsync(card.ID, card.Selection) });
                case "Highlight":
                    return RedirectToAction(nameof(Details), new { id = await cardService.HighlightAsync(card.ID, card.Selection) });
                case "Bookmark":
                    return View(nameof(Details), await cardService.BookmarkAsync(card.ID, card.Selection));
                case "Remember":
                    Uri uri = await cardService.RememberAsync(card.ID, card.Selection);
                    return Redirect(uri.AbsoluteUri);
                case "DeleteRegion":
                    return RedirectToAction(nameof(Details), new { id = await cardService.DeleteRegionAsync(card.ID, card.Selection) });
                case "CancelRepetitionCardCreation":
                    return RedirectToAction(nameof(Details), new { id = await cardService.CancelRepetitionCardCreationAsync(card.ID) });
                case "CompleteRepetitionCardCreation":
                    return RedirectToAction(nameof(Details), new { id = await cardService.CompleteRepetitionCardCreationAsync(card.ID) });
                case "Postpone":
                    CardViewModel cardViewModel = await cardService.PostponeAsync(card.ID, card.Priority.Value);
                    return RedirectToAction(nameof(DecksController.Details), DecksController.Name, new { id = cardViewModel.DeckID });
                default:
                    return BadRequest();
            }
        }

        public async Task<ActionResult> Create([DeckFound, DeckOwned]Guid deckID, string text)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DeckViewModel deck = await deckService.GetDeckAsync(deckID);

            IEnumerable<KeyValuePair<string, string>> priorities = cardService.GetFullPriorityList();

            var viewModel = new CreateCardViewModel
            {
                DeckID = deck.ID,
                DeckTitle = deck.Title,
                Text = text,
                PriorityList = priorities,
                Type = CardType.Common,
                CreationMode = CreationMode.Add
            };

            return View(viewModel);
        }

        public async Task<ActionResult> CreateFromUrl(string url)
        {
            IEnumerable<DeckViewModel> deckListItems = await deckService
                .GetUserDecks(UserName)
                .OrderBy(d => d.Title)
                .ToList();

            var viewModel = new CreateCardViewModel
            {
                DeckListItems = deckListItems,
                DeckID = deckService.GetUserLastDeck(UserName),
                Text = await articleExtractor.GetArticleText(url),
                Url = url,
                PriorityList = cardService.GetFullPriorityList(),
                Type = CardType.Article,
                CreationMode = CreationMode.FromUrl
            };

            return View(nameof(Create), viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("DeckID", "DeckTitle", "OldDeckID", "Title", "Text", "Url", "Priority", "Type", "ParentCardID", "ParentCardUpdatedText", "CreationMode")] CreateCardViewModel card)
        {
            if (ModelState.IsValid)
            {
                Guid newCardDeckId = await cardService.AddAsync(card);

                await deckService.SetUserLastDeckAsync(UserName, newCardDeckId);

                return card.CreationMode == CreationMode.Extract ?
                    RedirectToAction(nameof(DecksController.Details), DecksController.Name, new { id = card.OldDeckID ?? newCardDeckId }):
                    RedirectToAction(nameof(Index), new { newCardDeckId });
            }
            else
            {
                card.DeckListItems = settings.AllowDeckSelection && card.CreationMode != CreationMode.Add ?
                        await deckService
                            .GetUserDecks(UserName)
                            .OrderBy(d => d.Title)
                            .ToList() :
                        null;

                card.PriorityList = card.CreationMode == CreationMode.Extract ?
                    cardService.GetShortPriorityList() :
                    cardService.GetFullPriorityList();

                return View(card);
            }
        }

        public async Task<ActionResult> Edit([Required, CardFound, CardOwned]Guid? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            EditCardViewModel viewModel = await cardService.GetCardForEditingAsync(id.Value);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind("ID", "Title", "Text", "Url", "Type")] EditCardViewModel card)
        {
            if (ModelState.IsValid)
            {
                CardViewModel viewModel = await cardService.UpdateAsync(card);
                return RedirectToAction(nameof(DecksController.Details), DecksController.Name, new { id = viewModel.DeckID });
            }
            else
            {
                return View(card);
            }
        }

        public async Task<ActionResult> Delete([Required, CardFound, CardOwned]Guid? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CardViewModel card = await cardService.HideCardAsync(id.Value);

            return RedirectToAction(nameof(DecksController.Details), DecksController.Name, new { id = card.DeckID });
        }

        [HttpPost, ActionName(nameof(Delete))]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed([CardFound, CardOwned]Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await cardService.RemoveAsync(id);

            return RedirectToAction(nameof(DecksController.Index), DecksController.Name);
        }

        [AllowAnonymous]
        public async Task<ActionResult> IsValid(Guid readingCardId, Guid repetitionCardId)
        {
            bool isValid = await repetitionCardService.IsLocalIdValidAsync(readingCardId, repetitionCardId);
            return Json(isValid);
        }
    }
}
