using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReadingListPlus.Common;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Services;
using ReadingListPlus.Services.ArticleExtractorService;
using ReadingListPlus.Web.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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

        public CardsController(IDeckService deckService, ICardService cardService, IArticleExtractorService articleExtractor, ISchedulerService schedulerService, ITextConverterService textConverterService, ISettings settings)
        {
            this.deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
            this.cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            this.articleExtractor = articleExtractor ?? throw new ArgumentException(nameof(articleExtractor));
            this.schedulerService = schedulerService ?? throw new ArgumentException(nameof(schedulerService));
            this.textConverterService = textConverterService ?? throw new ArgumentException(nameof(textConverterService));
            this.settings = settings ?? throw new ArgumentException(nameof(settings));
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

                if (!deck.IsAuthorized(User))
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
                else if (!card.IsAuthorized(User))
                {
                    return Unauthorized();
                }
                else
                {
                    var viewModel = MapCardToViewModel(card);

                    return View(viewModel);
                }
            }
        }

        [HttpPost]
        public Task<ActionResult> Details([Bind("ID", "NextAction", "Selection", "Priority")] CardViewModel card)
        {
            switch (card.NextAction)
            {
                case "Extract":
                    return Extract(card.ID, card.Selection);
                case "Cloze":
                    return Cloze(card.ID, card.Selection);
                case "Highlight":
                    return Highlight(card.ID, card.Selection);
                case "Bookmark":
                    return Bookmark(card.ID, card.Selection);
                case "DeleteRegion":
                    return DeleteRegion(card.ID, card.Selection);
                case "Postpone":
                    return Postpone(card.ID, card.Priority.ToString());
                default:
                    throw new Exception();
            }
        }

        // GET: Cards/Create/5
        public async Task<ActionResult> Create(Guid deckID, string text)
        {
            var deck = await deckService.GetDeckAsync(deckID);

            if (!deck.IsAuthorized(User))
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
            var deckListItems = deckService.GetUserDecks(User)
                                .OrderBy(d => d.Title)
                                .Select(d => new SelectListItem { Value = d.ID.ToString(), Text = d.Title });

            var articleText = await articleExtractor.GetArticleText(url);
            var formattedText = articleText.Replace("\n", Environment.NewLine + Environment.NewLine);

            var priorities = GetFullPriorityList();

            var user = deckService.Users.Single(u => u.UserName == User.Identity.Name);
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

                if (!deck.IsAuthorized(User))
                {
                    return Unauthorized();
                }
                else if (oldDeck?.IsAuthorized(User) == false)
                {
                    return Unauthorized();
                }
                else if (parentCard?.IsAuthorized(User) == false)
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

                    var user = deckService.Users.Single(u => u.UserName == User.Identity.Name);
                    user.LastDeck = newCard.DeckID;

                    await deckService.SaveChangesAsync();

                    return RedirectToDeckDetails(card.OldDeckID ?? newCard.DeckID);
                }
            }
            else
            {
                card.DeckListItems =
                    card.CreationMode == CreationMode.Add ?
                        null :
                        deckService.GetUserDecks(User)
                            .OrderBy(d => d.Title)
                            .Select(d => new SelectListItem { Value = d.ID.ToString(), Text = d.Title });

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
            else if (!card.IsAuthorized(User))
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

                if (!dbCard.IsAuthorized(User))
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

                    return RedirectToAction("Details", "Decks", new { id = dbCard.DeckID });
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

            if (!card.IsAuthorized(User))
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
            var card = await cardService.GetCardAsync(ID);

            if (!card.IsAuthorized(User))
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
            var card = await cardService.GetCardAsync(ID);

            if (!card.IsAuthorized(User))
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
            var card = await cardService.GetCardAsync(ID);

            if (!card.IsAuthorized(User))
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
                    var userDecks = deckService.GetUserDecks(User)
                            .OrderBy(d => d.Title)
                            .Select(d => new SelectListItem { Value = d.ID.ToString(), Text = d.Title });

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
            var card = await cardService.GetCardAsync(ID);

            if (!card.IsAuthorized(User))
            {
                return Unauthorized();
            }
            else
            {
                var textWithoutBookmarks = textConverterService.DeleteTagByName(text, "bookmark");

                var newText = textConverterService.ReplaceTag(textWithoutBookmarks, "selection", "bookmark");

                card.Text = newText;

                await deckService.SaveChangesAsync();

                var viewModel = MapCardToViewModel(card);

                viewModel.IsBookmarked = true;

                return View("Details", viewModel);
            }
        }

        public async Task<ActionResult> DeleteRegion(Guid ID, string text)
        {
            var card = await cardService.GetCardAsync(ID);

            if (!card.IsAuthorized(User))
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
            else if (!card.IsAuthorized(User))
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

            if (!card.IsAuthorized(User))
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

        private ActionResult RedirectToDeckDetails(Guid? deckID)
        {
            return RedirectToAction("Details", "Decks", new { id = deckID });
        }

        private IEnumerable<SelectListItem> GetFullPriorityList()
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

        private IEnumerable<SelectListItem> GetShortPriorityList()
        {
            var priorities = new[]
            {
                Priority.High,
                Priority.Medium,
                Priority.Low,
            };

            return GetPriorities(priorities);
        }

        private IEnumerable<SelectListItem> GetPriorities(IEnumerable<Priority> priorities) =>
            from x in priorities
            let displayName = x.GetAttribute<DisplayAttribute>()
            select new SelectListItem
            {
                Value = ((int)x).ToString(),
                Text = displayName != null ? displayName.GetName() : x.ToString()
            };

        private CardViewModel MapCardToViewModel(Card card) =>
            new CardViewModel
            {
                ID = card.ID,
                DeckID = card.DeckID,
                DeckTitle = card.Deck.Title,
                Type = card.Type,
                Title = card.Title,
                Text = card.Text,
                HtmlText = textConverterService.GetHtml(
                    card.Text,
                    Uri.UnescapeDataString(
                        Url.Action(
                            nameof(Details),
                            new { ID = $"${{{Constants.CardIdGroup}}}" }))),
                Url = card.Url,
                Position = card.Position
            };

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
    }
}
