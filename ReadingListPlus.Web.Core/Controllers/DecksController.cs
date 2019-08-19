using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ReadingListPlus.Common;
using ReadingListPlus.DataAccess;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Web.Core.ViewModels;

namespace ReadingListPlus.Web.Core.Controllers
{
    [Authorize]
    public class DecksController : Controller
    {
        private ApplicationContext db;
        private readonly ISettings settings;

        public DecksController(ApplicationContext db, ISettings settings)
        {
            this.db = db;
            this.settings = settings;
        }

        // GET: Decks
        public async Task<ActionResult> Index()
        {
            var items = db.GetUserDecks(User).OrderBy(d => d.Title);

            return View(await items.ToListAsync());
        }

        [Authorize(Policy = Constants.BackupPolicy)]
        public async Task<ActionResult> Export(string id)
        {
            var decks = await db.Decks.Include(d => d.Cards).ToListAsync();
            var jsonResult = new JsonResult(decks);

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

                    db.Cards.RemoveRange(db.Cards);
                    db.Decks.RemoveRange(db.Decks);
                    await db.SaveChangesAsync();

                    db.Decks.AddRange(newDecks);
                    await db.SaveChangesAsync();
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

            var deck = await db.GetDeckAsync(id.Value);

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
                var card = cards.GetMinElement(c => c.Position);

                return RedirectToAction("Details", "Cards", new { id = card.ID });
            }
            else
            {
                return RedirectToAction("Details", "Cards", new { DeckID = id.Value });
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

                db.Decks.Add(deck);

                await db.SaveChangesAsync();

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

            var deck = await db.GetDeckAsync(id.Value);

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
                var dbDeck = await db.GetDeckAsync(deck.ID);

                if (deck == null)
                {
                    return NotFound();
                }
                else if (!dbDeck.IsAuthorized(User))
                {
                    return Unauthorized();
                }

                dbDeck.Title = deck.Title;

                await db.SaveChangesAsync();

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

            var deck = await db.GetDeckAsync(id.Value);

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
            var deck = await db.GetDeckAsync(id);

            if (!deck.IsAuthorized(User))
            {
                return Unauthorized();
            }

            db.Decks.Remove(deck);

            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
