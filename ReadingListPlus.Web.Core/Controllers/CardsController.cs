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
using ReadingListPlus.Web.Core.Pages.Decks;

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
        private readonly ITextConverterService textConverterService;

        private string UserName => User.Identity.Name;

        public CardsController(ISettings settings, IDeckService deckService, ICardService cardService, IArticleExtractorService articleExtractor, IRepetitionCardService repetitionCardService, ITextConverterService textConverterService)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
            this.cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            this.articleExtractor = articleExtractor ?? throw new ArgumentException(nameof(articleExtractor));
            this.repetitionCardService = repetitionCardService ?? throw new ArgumentNullException(nameof(repetitionCardService));
            this.textConverterService = textConverterService ?? throw new ArgumentNullException(nameof(textConverterService));
        }

        [Authorize(Policy = Constants.FixPolicy)]
        public async Task<ActionResult> Fix(Guid deckId)
        {
            await deckService.FixDeckAsync(deckId);
            return RedirectToPage(CardIndexModel.PageName, new { DeckId = deckId });
        }

        [Authorize(Policy = Constants.FixPolicy)]
        public async Task<ActionResult> FixCardOwner()
        {
            await cardService.FixCardOwnerAsync(User.Identity.Name);

            return RedirectToPage(DeckIndexModel.PageName);
        }

        [Authorize(Policy = Constants.FixPolicy)]
        public async Task<ActionResult> FixSyntax()
        {
            await cardService.FixSyntax();

            return RedirectToPage(DeckIndexModel.PageName);
        }

        public async Task<ActionResult> CreateFromUrl(string url, [DeckFound, DeckOwned]Guid? deckId)
        {
            (string text, string title) = await articleExtractor.GetTextAndTitleAsync(url);

            var viewModel = new CreateCardViewModel
            {
                DeckID = deckId ?? deckService.GetUserLastDeck(UserName),
                DeckTitle = deckId.HasValue ? (await deckService.GetDeckAsync(deckId.Value)).Title : null,
                Title = title,
                Text = text,
                Url = url,
                CardType = CardType.Article,
                CreationMode = CreationMode.FromUrl
            };

            TempData[nameof(CreateCardViewModel)] = JsonConvert.SerializeObject(viewModel);

            return RedirectToPage(CardCreateModel.PageName, new { DeckId = deckId });
        }

        public async Task<ActionResult> Hide([Required, CardFound, CardOwned]Guid? id, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await cardService.HideCardAsync(id.Value);

            return string.IsNullOrEmpty(returnUrl)
                ? RedirectToPage(DeckIndexModel.PageName) as ActionResult
                : Redirect(returnUrl);
        }

        [AllowAnonymous]
        public async Task<ActionResult> IsValid(Guid readingCardId, Guid repetitionCardId)
        {
            bool isValid = await repetitionCardService.IsLocalIdValidAsync(readingCardId, repetitionCardId);
            return Json(isValid);
        }

        public async Task<ActionResult> Tree(Guid id)
        {
            IEnumerable<CardViewModel> cards = await cardService.GetConnectedCardsAsync(id);

            var json = cards
                .OrderBy(d => d.Text)
                .Select(c => new
                {
                    c.ID,
                    Text = c.Text.Truncate(Constants.MaxTreeTextLength),
                    a_attr = new { href = Url.Page(CardReadModel.PageName, new { c.ID }) },
                    Children = textConverterService
                        .GetTags(c.Text, Constants.RepetitionCardLabel)
                        .Select(x => new
                        {
                            Text = textConverterService.GetTagText(x, Constants.RepetitionCardLabel),
                            a_attr = new
                            {
                                href = new Uri(
                                    new Uri(settings.SpacedRepetitionServer),
                                    $"Cards/Edit/{textConverterService.GetIdParameter(x, Constants.RepetitionCardLabel)}").AbsoluteUri,
                            }
                        })
                });

            return Json(json);
        }
    }
}
