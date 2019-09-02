using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ReadingListPlus.Common;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Services;
using ReadingListPlus.Web.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReadingListPlus.Web.Core.Controllers
{
    [Authorize]
    public class DecksController : Controller
    {
        public const string Name = "Decks";

        private readonly IDeckService deckService;
        private readonly ICardService cardService;
        private readonly ISchedulerService schedulerService;
        private readonly ISettings settings;

        public DecksController(IDeckService deckService, ICardService cardService, ISchedulerService schedulerService, ISettings settings)
        {
            this.deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
            this.cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            this.schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        // GET: Decks
        public async Task<ActionResult> Index()
        {
            var items = deckService.GetUserDecks(User).OrderBy(d => d.Title);

            return View(await items.ToListAsync());
        }

        [Authorize(Policy = Constants.BackupPolicy)]
        public async Task<ActionResult> Export(string id)
        {
            _ = id;

            var decks = await deckService.Decks.Include(d => d.Cards).ToListAsync();
            var jsonResult = new JsonResult(decks, new JsonSerializerSettings { Formatting = Formatting.Indented });

            return jsonResult;
        }

        [Authorize(Policy = Constants.BackupPolicy)]
        public ActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = Constants.BackupPolicy)]
        public async Task<ActionResult> Import(ImportViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var streamReader = new StreamReader(model.File.OpenReadStream()))
                {
                    var jsonSerializer = new JsonSerializer();
                    var jsonReader = new JsonTextReader(streamReader);
                    var newDecks = jsonSerializer.Deserialize<IEnumerable<Deck>>(jsonReader);

                    if (settings.ResetKeysOnImport)
                    {
                        foreach (var deck in newDecks)
                        {
                            deck.ID = Guid.NewGuid();

                            foreach (var card in deck.Cards)
                            {
                                card.ID = Guid.NewGuid();
                                card.ParentCardID = null;
                            }
                        }
                    }

                    cardService.Cards.RemoveRange(cardService.Cards);
                    deckService.Decks.RemoveRange(deckService.Decks);
                    await deckService.SaveChangesAsync();

                    deckService.Decks.AddRange(newDecks);
                    await deckService.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View();
            }
        }

        // GET: Decks/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var deck = await deckService.GetDeckAsync(id.Value);

            if (deck == null)
            {
                return NotFound();
            }
            else if (!deck.IsAuthorized(User))
            {
                return Unauthorized();
            }

            var cards = deck.ConnectedCards;

            if (cards.Any())
            {
                var card = schedulerService.GetFirstCard(cards);

                return RedirectToAction(nameof(CardsController.Details), CardsController.Name, new { card.ID });
            }
            else
            {
                return RedirectToAction(nameof(CardsController.Details), CardsController.Name, new { DeckID = id.Value });
            }
        }

        // GET: Decks/Create
        public ActionResult Create()
        {
            var deck = new Deck();

            return View(deck);
        }

        // POST: Decks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DeckViewModel deckViewModel)
        {
            if (ModelState.IsValid)
            {
                var deck = new Deck
                {
                    ID = Guid.NewGuid(),
                    Title = deckViewModel.Title,
                    OwnerID = User.Identity.Name
                };

                deckService.Decks.Add(deck);

                await deckService.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            else
            {
                return View(deckViewModel);
            }
        }

        // GET: Decks/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var deck = await deckService.GetDeckAsync(id.Value);

            if (deck == null)
            {
                return NotFound();
            }
            else if (!deck.IsAuthorized(User))
            {
                return Unauthorized();
            }
            else
            {
                return View(deck);
            }
        }

        // POST: Decks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind("ID", "Title")] Deck deck)
        {
            if (ModelState.IsValid)
            {
                var dbDeck = await deckService.GetDeckAsync(deck.ID);

                if (deck == null)
                {
                    return NotFound();
                }
                else if (!dbDeck.IsAuthorized(User))
                {
                    return Unauthorized();
                }

                dbDeck.Title = deck.Title;

                await deckService.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            else
            {
                return View(deck);
            }
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var deck = await deckService.GetDeckAsync(id.Value);

            if (deck == null)
            {
                return NotFound();
            }
            else if (!deck.IsAuthorized(User))
            {
                return Unauthorized();
            }
            else
            {
                return View(deck);
            }
        }

        // POST: Decks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            var deck = await deckService.GetDeckAsync(id);

            if (!deck.IsAuthorized(User))
            {
                return Unauthorized();
            }

            deckService.Decks.Remove(deck);

            await deckService.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
