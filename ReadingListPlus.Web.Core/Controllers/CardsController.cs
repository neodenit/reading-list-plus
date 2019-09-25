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
using ReadingListPlus.Web.Core.Pages.Cards;

namespace ReadingListPlus.Web.Core.Controllers
{
    [Authorize]
    public class CardsController : Controller
    {
        public const string Name = "Cards";

        private readonly IDeckService deckService;
        private readonly ICardService cardService;
        private readonly IArticleExtractorService articleExtractor;
        private readonly IRepetitionCardService repetitionCardService;

        private string UserName => User.Identity.Name;

        public CardsController(IDeckService deckService, ICardService cardService, IArticleExtractorService articleExtractor, IRepetitionCardService repetitionCardService)
        {
            this.deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
            this.cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            this.articleExtractor = articleExtractor ?? throw new ArgumentException(nameof(articleExtractor));
            this.repetitionCardService = repetitionCardService ?? throw new ArgumentNullException(nameof(repetitionCardService));
        }

        [Authorize(Policy = Constants.FixPolicy)]
        public async Task<ActionResult> Fix(Guid deckId)
        {
            await deckService.FixDeckAsync(deckId);
            return RedirectToPage(CardIndexModel.PageName, new { DeckId = deckId });
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

            TempData[nameof(CreateCardViewModel)] = JsonConvert.SerializeObject(viewModel);
            return RedirectToPage(CardCreateModel.PageName);
        }

        public async Task<ActionResult> Hide([Required, CardFound, CardOwned]Guid? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CardViewModel card = await cardService.HideCardAsync(id.Value);

            return RedirectToAction(nameof(DecksController.Read), DecksController.Name, new { Id = card.DeckID });
        }

        [AllowAnonymous]
        public async Task<ActionResult> IsValid(Guid readingCardId, Guid repetitionCardId)
        {
            bool isValid = await repetitionCardService.IsLocalIdValidAsync(readingCardId, repetitionCardId);
            return Json(isValid);
        }
    }
}
