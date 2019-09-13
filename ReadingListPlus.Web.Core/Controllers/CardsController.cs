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
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Services;
using ReadingListPlus.Services.ArticleExtractorService;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Web.Core.Controllers
{
    [Authorize]
    public class CardsController : Controller
    {
        public const string Name = "Cards";

        private readonly IDeckService deckService;
        private readonly ICardService cardService;
        private readonly IArticleExtractorService articleExtractor;
        private readonly ISchedulerService schedulerService;
        private readonly ITextConverterService textConverterService;
        private readonly ISettings settings;
        private readonly IHttpClientWrapper httpClientWrapper;

        private string UserName => User.Identity.Name;

        public CardsController(IDeckService deckService, ICardService cardService, IArticleExtractorService articleExtractor, ISchedulerService schedulerService, ITextConverterService textConverterService, ISettings settings, IHttpClientWrapper httpClientWrapper)
        {
            this.deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
            this.cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            this.articleExtractor = articleExtractor ?? throw new ArgumentException(nameof(articleExtractor));
            this.schedulerService = schedulerService ?? throw new ArgumentException(nameof(schedulerService));
            this.textConverterService = textConverterService ?? throw new ArgumentException(nameof(textConverterService));
            this.settings = settings ?? throw new ArgumentException(nameof(settings));
            this.httpClientWrapper = httpClientWrapper ?? throw new ArgumentNullException(nameof(httpClientWrapper));
        }

        // GET: Cards
        public async Task<ActionResult> Index(Guid? DeckID)
        {
            if (DeckID == null)
            {
                return BadRequest();
            }
            else
            {
                var deck = await deckService.GetDeckAsync(DeckID.Value);

                if (!deck.IsAuthorized(UserName))
                {
                    return Unauthorized();
                }
                else
                {
                    return View(deck.ConnectedCards.OrderBy(c => c.Position).Select(c => MapCardToViewModel(c)).ToList());
                }
            }
        }

        public async Task<ActionResult> Fix(Guid DeckID)
        {
            var deck = await deckService.GetDeckAsync(DeckID);

            var cards = deck.ConnectedCards.OrderBy(item => item.Position).ToList();

            Enumerable.Range(Constants.FirstCardPosition, cards.Count).Zip(cards, (i, item) => new { i = i, card = item }).ToList().ForEach(item => item.card.Position = item.i);

            await deckService.SaveChangesAsync();

            return View("Index", cards.Select(c => MapCardToViewModel(c)));
        }

        public async Task<ActionResult> Details(Guid? id, Guid? deckId)
        {
            if (id == null)
            {
                if (deckId == null)
                {
                    return BadRequest();
                }
                else
                {
                    return RedirectToAction(nameof(Create), new { deckId });
                }
            }
            else
            {
                var card = await cardService.GetCardAsync(id.Value);

                if (card == null)
                {
                    return NotFound();
                }
                else if (!card.IsAuthorized(UserName))
                {
                    return Unauthorized();
                }
                else
                {
                    var newRepetitionCardText = textConverterService.GetNewRepetitionCardText(card.Text);

                    if (!string.IsNullOrEmpty(newRepetitionCardText))
                    {
                        var reservedId = textConverterService.GetIdParameter(card.Text, Constants.NewRepetitionCardLabel);
                        var isValid = await IsRepetitionCardValidAsync(new Guid(reservedId), card.ID);
                        var newRepetitionCardState = isValid ? NewRepetitionCardState.Done : NewRepetitionCardState.Pending;
                        var viewModel = MapCardToHtmlViewModel(card, newRepetitionCardState);
                        return View(viewModel);
                    }
                    else
                    {
                        var viewModel = MapCardToHtmlViewModel(card, NewRepetitionCardState.None);
                        return View(viewModel);
                    }
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult> Details([Bind("ID", "NextAction", "Selection", "Priority")] CardViewModel card)
        {
            var dbCard = await cardService.GetCardAsync(card.ID);

            var newRepetitionCardText = textConverterService.GetNewRepetitionCardText(dbCard.Text);
            var hasNewRepetitionCard = !string.IsNullOrEmpty(newRepetitionCardText);

            var newRepetitionCardActions = new[] { "CancelRepetitionCardCreation", "CompleteRepetitionCardCreation" };

            if (hasNewRepetitionCard && !newRepetitionCardActions.Contains(card.NextAction))
            {
                return BadRequest();
            }

            switch (card.NextAction)
            {
                case "Extract":
                    return await Extract(card.ID, card.Selection);
                case "Cloze":
                    return await Cloze(card.ID, card.Selection);
                case "Highlight":
                    return await Highlight(card.ID, card.Selection);
                case "Bookmark":
                    return await Bookmark(card.ID, card.Selection);
                case "Remember":
                    return await Remember(card.ID, card.Selection);
                case "DeleteRegion":
                    return await DeleteRegion(card.ID, card.Selection);
                case "CancelRepetitionCardCreation":
                    return await CancelRepetitionCardCreation(card.ID, card.Selection);
                case "CompleteRepetitionCardCreation":
                    return await CompleteRepetitionCardCreation(card.ID, card.Selection);
                case "Postpone":
                    return await Postpone(card.ID, card.Priority.ToString());
                default:
                    return BadRequest();
            }
        }

        private async Task<ActionResult> Remember(Guid cardId, string text)
        {
            if (!settings.RememberEnabled)
            {
                return BadRequest();
            }

            var card = await cardService.GetCardAsync(cardId);

            if (!card.IsAuthorized(UserName))
            {
                return Unauthorized();
            }
            else
            {
                var repetitionCardText = textConverterService.GetSelection(text);
                var encodedRepetitionCardText = Uri.EscapeDataString(repetitionCardText);

                var textWithNewRepetitionCard = textConverterService.ReplaceTag(text, Constants.SelectionLabel, Constants.NewRepetitionCardLabel);
                var repetitionCardId = Guid.NewGuid().ToString();
                var textWithNewRepetitionCardId = textConverterService.AddParameter(textWithNewRepetitionCard, Constants.NewRepetitionCardLabel, repetitionCardId);

                card.Text = textWithNewRepetitionCardId;

                await deckService.SaveChangesAsync();

                var baseUri = new Uri(settings.SpacedRepetionServer);
                var uri = new Uri(baseUri, $"Cards/Create?readingCardId={cardId}&repetitionCardId={repetitionCardId}&text={encodedRepetitionCardText}");
                return Redirect(uri.AbsoluteUri);
            }
        }

        // GET: Cards/Create/5
        public async Task<ActionResult> Create(Guid deckID, string text)
        {
            var deck = await deckService.GetDeckAsync(deckID);

            if (!deck.IsAuthorized(UserName))
            {
                return Unauthorized();
            }
            else
            {
                var priorities = GetFullPriorityList();

                var card = new CreateCardViewModel
                {
                    DeckID = deck.ID,
                    DeckTitle = deck.Title,
                    Text = text,
                    PriorityList = priorities,
                    Type = CardType.Common,
                    CreationMode = CreationMode.Add
                };

                return View(card);
            }
        }

        public async Task<ActionResult> CreateFromUrl(string url)
        {
            var deckListItems = await deckService
                .GetUserDecks(UserName)
                .OrderBy(d => d.Title)
                .ToList();

            var articleText = await articleExtractor.GetArticleText(url);
            var formattedText = articleText.Replace("\n", Environment.NewLine + Environment.NewLine);

            var priorities = GetFullPriorityList();

            var user = deckService.Users.Single(u => u.UserName == UserName);
            var lastDeck = user.LastDeck;

            var card = new CreateCardViewModel
            {
                DeckListItems = deckListItems,
                DeckID = lastDeck.GetValueOrDefault(),
                Text = formattedText,
                Url = url,
                PriorityList = priorities,
                Type = CardType.Article,
                CreationMode = CreationMode.FromUrl
            };

            return View("Create", card);
        }

        // POST: Cards/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("DeckID", "DeckTitle", "OldDeckID", "Title", "Text", "Url", "Priority", "Type", "ParentCardID", "ParentCardUpdatedText", "CreationMode")] CreateCardViewModel card)
        {
            if (ModelState.IsValid)
            {
                var deck = await deckService.GetDeckAsync(card.DeckID.Value);
                var oldDeck = card.OldDeckID.HasValue ? await deckService.GetDeckAsync(card.OldDeckID.Value) : null;
                var parentCard = card.ParentCardID.HasValue ? await cardService.GetCardAsync(card.ParentCardID.Value) : null;

                if (!deck.IsAuthorized(UserName))
                {
                    return Unauthorized();
                }
                else if (oldDeck?.IsAuthorized(UserName) == false)
                {
                    return Unauthorized();
                }
                else if (parentCard?.IsAuthorized(UserName) == false)
                {
                    return Unauthorized();
                }
                else
                {
                    var priority = card.Priority.Value;

                    var newCard = new Card
                    {
                        ID = Guid.NewGuid(),
                        DeckID = card.DeckID,
                        Title = card.Title,
                        Text = card.Text,
                        Url = card.Url,
                        Type = card.Type,
                        ParentCardID = card.ParentCardID,
                    };

                    schedulerService.PrepareForAdding(deck, deck.ConnectedCards, newCard, priority);

                    cardService.Cards.Add(newCard);

                    if (oldDeck != null)
                    {
                        oldDeck.DependentDeckID = newCard.DeckID;
                    }

                    if (parentCard != null)
                    {
                        await deckService.SaveChangesAsync();

                        var textWithNewCardID = textConverterService.AddParameter(card.ParentCardUpdatedText, "selection", newCard.ID.ToString());
                        var textWithoutSelection = textConverterService.ReplaceTag(textWithNewCardID, "selection", "extract");

                        parentCard.Text = textWithoutSelection;
                    }

                    var user = deckService.Users.Single(u => u.UserName == UserName);
                    user.LastDeck = newCard.DeckID;

                    await deckService.SaveChangesAsync();

                    return card.CreationMode == CreationMode.Extract ?
                        RedirectToDeckDetails(card.OldDeckID ?? newCard.DeckID) :
                        RedirectToAction(nameof(Index), new { newCard.DeckID });
                }
            }
            else
            {
                card.DeckListItems =
                    card.CreationMode == CreationMode.Add ?
                        null :
                        await deckService
                            .GetUserDecks(UserName)
                            .OrderBy(d => d.Title)
                            .ToList();

                card.PriorityList = card.CreationMode == CreationMode.Extract ? GetShortPriorityList() : GetFullPriorityList();

                return View(card);
            }
        }

        // GET: Cards/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var card = await cardService.GetCardAsync(id.Value);

            if (card == null)
            {
                return NotFound();
            }
            else if (!card.IsAuthorized(UserName))
            {
                return Unauthorized();
            }
            else
            {
                var viewModel = MapCardToEditViewModel(card);
                return View(viewModel);
            }
        }

        // POST: Cards/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind("ID", "Title", "Text", "Url", "Type")] EditCardViewModel card)
        {
            if (ModelState.IsValid)
            {
                var dbCard = await cardService.GetCardAsync(card.ID);

                if (!dbCard.IsAuthorized(UserName))
                {
                    return Unauthorized();
                }
                else
                {
                    dbCard.Title = card.Title;
                    dbCard.Text = card.Text;
                    dbCard.Url = card.Url;
                    dbCard.Type = card.Type;

                    await deckService.SaveChangesAsync();

                    return RedirectToDeckDetails(dbCard.DeckID);
                }
            }
            else
            {
                return View(card);
            }
        }

        #region Actions
        public async Task<ActionResult> Postpone(Guid ID, string Priority)
        {
            var card = await cardService.GetCardAsync(ID);

            if (!card.IsAuthorized(UserName))
            {
                return Unauthorized();
            }
            else if (card.Position != Constants.FirstCardPosition)
            {
                return RedirectToDeckDetails(card.DeckID);
            }
            else
            {
                var deck = card.Deck;
                var deckCards = deck.ConnectedCards;

                var priority = schedulerService.ParsePriority(Priority);

                schedulerService.ChangeFirstCardPosition(deck, deckCards, card, priority);

                await deckService.SaveChangesAsync();

                return RedirectToDeckDetails(card.DeckID);
            }
        }

        public async Task<ActionResult> Highlight(Guid ID, string text)
        {
            if (!settings.HighlightEnabled)
            {
                return BadRequest();
            }

            var card = await cardService.GetCardAsync(ID);

            if (!card.IsAuthorized(UserName))
            {
                return Unauthorized();
            }
            else
            {
                var newText = textConverterService.AddHighlight(card.Text, text);

                card.Text = newText;

                await deckService.SaveChangesAsync();

                return RedirectToDeckDetails(card.DeckID);
            }
        }

        public async Task<ActionResult> Cloze(Guid ID, string text)
        {
            if (!settings.ClozeEnabled)
            {
                return BadRequest();
            }

            var card = await cardService.GetCardAsync(ID);

            if (!card.IsAuthorized(UserName))
            {
                return Unauthorized();
            }
            else
            {
                var newText = textConverterService.AddCloze(card.Text, text);

                card.Text = newText;

                await deckService.SaveChangesAsync();

                return RedirectToDeckDetails(card.DeckID);
            }
        }

        public async Task<ActionResult> Extract(Guid ID, string text)
        {
            if (!settings.ExtractEnabled)
            {
                return BadRequest();
            }

            var card = await cardService.GetCardAsync(ID);

            if (!card.IsAuthorized(UserName))
            {
                return Unauthorized();
            }
            else
            {
                ModelState.Clear();

                var selection = textConverterService.GetSelection(text);

                var priorities = GetShortPriorityList();

                var newCard = new CreateCardViewModel
                {
                    Url = card.Url,
                    ParentCardID = card.ID,
                    Text = selection,
                    ParentCardUpdatedText = text,
                    PriorityList = priorities,
                    Type = CardType.Extract,
                    CreationMode = CreationMode.Extract
                };

                if (settings.AllowDeckSelection)
                {
                    var userDecks = await deckService
                        .GetUserDecks(UserName)
                        .OrderBy(d => d.Title)
                        .ToList();

                    newCard.OldDeckID = card.DeckID;

                    if (card.Deck.DependentDeckID.HasValue)
                    {
                        newCard.DeckID = card.Deck.DependentDeckID;
                        newCard.DeckListItems = userDecks;
                    }
                    else
                    {
                        newCard.DeckListItems = userDecks;
                    }
                }
                else
                {
                    newCard.DeckID = card.DeckID;
                }

                return View("Create", newCard);
            }
        }

        public async Task<ActionResult> Bookmark(Guid ID, string text)
        {
            if (!settings.BookmarkEnabled)
            {
                return BadRequest();
            }

            var card = await cardService.GetCardAsync(ID);

            if (!card.IsAuthorized(UserName))
            {
                return Unauthorized();
            }
            else
            {
                var textWithoutBookmarks = textConverterService.DeleteTagByName(text, "bookmark");

                var newText = textConverterService.ReplaceTag(textWithoutBookmarks, "selection", "bookmark");

                card.Text = newText;

                await deckService.SaveChangesAsync();

                var viewModel = MapCardToHtmlViewModel(card, NewRepetitionCardState.None);

                viewModel.IsBookmarked = true;

                return View("Details", viewModel);
            }
        }

        public async Task<ActionResult> DeleteRegion(Guid ID, string text)
        {
            if (!settings.DropEnabled)
            {
                return BadRequest();
            }

            var card = await cardService.GetCardAsync(ID);

            if (!card.IsAuthorized(UserName))
            {
                return Unauthorized();
            }
            else
            {
                var newText = textConverterService.DeleteTagByText(card.Text, text);

                card.Text = newText;

                await deckService.SaveChangesAsync();

                return RedirectToDeckDetails(card.DeckID);
            }
        }

        public async Task<ActionResult> CancelRepetitionCardCreation(Guid ID, string text)
        {
            if (!settings.RememberEnabled)
            {
                return BadRequest();
            }

            var card = await cardService.GetCardAsync(ID);

            if (!card.IsAuthorized(UserName))
            {
                return Unauthorized();
            }
            else
            {
                var newText = textConverterService.DeleteTagByName(card.Text, Constants.NewRepetitionCardLabel);

                card.Text = newText;

                await deckService.SaveChangesAsync();

                return RedirectToDeckDetails(card.DeckID);
            }
        }

        public async Task<ActionResult> CompleteRepetitionCardCreation(Guid ID, string text)
        {
            if (!settings.RememberEnabled)
            {
                return BadRequest();
            }

            var card = await cardService.GetCardAsync(ID);

            if (!card.IsAuthorized(UserName))
            {
                return Unauthorized();
            }
            else
            {
                var newText = textConverterService.ReplaceTag(card.Text, Constants.NewRepetitionCardLabel, Constants.RepetitionCardLabel);

                card.Text = newText;

                await deckService.SaveChangesAsync();

                return RedirectToDeckDetails(card.DeckID);
            }
        }
        #endregion

        // GET: Cards/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var card = await cardService.GetCardAsync(id.Value);

            if (card == null)
            {
                return NotFound();
            }
            else if (!card.IsAuthorized(UserName))
            {
                return Unauthorized();
            }
            else
            {
                schedulerService.PrepareForDeletion(card.Deck.ConnectedCards, card);

                card.Position = Constants.DisconnectedCardPosition;

                await deckService.SaveChangesAsync();

                return RedirectToDeckDetails(card.DeckID);
            }
        }

        // POST: Cards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            var card = await cardService.GetCardAsync(id);

            if (!card.IsAuthorized(UserName))
            {
                return Unauthorized();
            }
            else if (card.Position != Constants.DisconnectedCardPosition)
            {
                return BadRequest();
            }
            else
            {
                cardService.Cards.Remove(card);

                await deckService.SaveChangesAsync();

                return RedirectToDeckDetails(card.DeckID);
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> IsValid(Guid readingCardId, Guid repetitionCardId)
        {
            Card card = await cardService.GetCardAsync(readingCardId);
            string param = textConverterService.GetIdParameter(card.Text, Constants.NewRepetitionCardLabel);
            var reservedId = new Guid(param);
            var result = reservedId == repetitionCardId;
            return Json(result);
        }

        private ActionResult RedirectToDeckDetails(Guid? deckID)
        {
            return RedirectToAction(nameof(DecksController.Details), DecksController.Name, new { id = deckID });
        }

        private IEnumerable<KeyValuePair<string, string>> GetFullPriorityList()
        {
            var priorities = new[]
            {
                Priority.Highest,
                Priority.High,
                Priority.Medium,
                Priority.Low,
            };

            return GetPriorities(priorities);
        }

        private IEnumerable<KeyValuePair<string, string>> GetShortPriorityList()
        {
            var priorities = new[]
            {
                Priority.High,
                Priority.Medium,
                Priority.Low,
            };

            return GetPriorities(priorities);
        }

        private IEnumerable<KeyValuePair<string, string>> GetPriorities(IEnumerable<Priority> priorities) =>
            from x in priorities
            let displayName = x.GetAttribute<DisplayAttribute>()
            select KeyValuePair.Create(
                ((int)x).ToString(),
                displayName != null ? displayName.GetName() : x.ToString());

        private CardViewModel MapCardToViewModel(Card card) =>
            new CardViewModel
            {
                ID = card.ID,
                DeckID = card.DeckID,
                DeckTitle = card.Deck.Title,
                Type = card.Type,
                Title = card.Title,
                Text = card.Text,
                Url = card.Url,
                Position = card.Position
            };

        private CardViewModel MapCardToHtmlViewModel(Card card, NewRepetitionCardState newRepetitionCardState)
        {
            var cardUrlTemplate =
                    Uri.UnescapeDataString(
                        Url.Action(
                            nameof(Details),
                            new { ID = $"${{{Constants.IdGroup}}}" }));

            var repetitionCardUrlTemplate = new Uri(new Uri(settings.SpacedRepetionServer), $"Cards/Edit/{textConverterService.GetIdParameter(card.Text, Constants.RepetitionCardLabel)}").AbsoluteUri;

            var newRepetitionCardUrlTemplate = newRepetitionCardState ==
                NewRepetitionCardState.Done ?
                    new Uri(new Uri(settings.SpacedRepetionServer), $"Cards/Edit/{textConverterService.GetIdParameter(card.Text, Constants.NewRepetitionCardLabel)}").AbsoluteUri :
                    new Uri(new Uri(settings.SpacedRepetionServer), $"Cards/Create?readingCardId={card.ID}&repetitionCardId={textConverterService.GetIdParameter(card.Text, Constants.NewRepetitionCardLabel)}&text={Uri.EscapeDataString(textConverterService.GetNewRepetitionCardText(card.Text))}").AbsoluteUri;

            var newRepetitionCardClass = newRepetitionCardState == NewRepetitionCardState.Done ?
                Constants.RepetitionCardLabel :
                Constants.NewRepetitionCardLabel;

            var model = new CardViewModel
            {
                ID = card.ID,
                DeckID = card.DeckID,
                DeckTitle = card.Deck.Title,
                Type = card.Type,
                Title = card.Title,
                Text = card.Text,
                HtmlText = textConverterService.GetHtml(
                    card.Text,
                    cardUrlTemplate,
                    repetitionCardUrlTemplate,
                    newRepetitionCardUrlTemplate,
                    newRepetitionCardClass),
                Url = card.Url,
                Position = card.Position,
                NewRepetitionCardState = newRepetitionCardState
            };

            return model;
        }

        private CreateCardViewModel MapCardToCreateViewModel(Card card) =>
                new CreateCardViewModel
                {
                    DeckID = card.DeckID,
                    DeckTitle = card.Deck.Title,
                    Type = card.Type,
                    Title = card.Title,
                    Text = card.Text,
                    Url = card.Url
                };

        private EditCardViewModel MapCardToEditViewModel(Card card) =>
            new EditCardViewModel
            {
                ID = card.ID,
                DeckID = card.DeckID,
                DeckTitle = card.Deck.Title,
                Type = card.Type,
                Title = card.Title,
                Text = card.Text,
                Url = card.Url
            };

        private async Task<bool> IsRepetitionCardValidAsync(Guid repetitionCardId, Guid readingCardId)
        {
            var baseUri = new Uri(settings.SpacedRepetionServer);
            var uri = new Uri(baseUri, $"Cards/IsValid?readingCardId={readingCardId}&repetitionCardId={repetitionCardId}");
            var response = await httpClientWrapper.GetAsync(uri);
            var responseString = await response.Content.ReadAsStringAsync();
            var isValid = JsonConvert.DeserializeObject<bool>(responseString);
            return isValid;
        }
    }
}
