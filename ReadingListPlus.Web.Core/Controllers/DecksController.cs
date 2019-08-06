﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ReadingListPlus.DataAccess;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Web.Core.ViewModels;

namespace ReadingListPlus.Web.Core.Controllers
{
    [Authorize]
    [RequireHttps]
    public class DecksController : Controller
    {
        private ApplicationContext db ;

        public DecksController(ApplicationContext db)
        {
            this.db = db;
        }

        // GET: Decks
        public async Task<ActionResult> Index()
        {
            var items = db.GetUserDecks(User).OrderBy(d => d.Title);

            return View(await items.ToListAsync());
        }

        public async Task<ActionResult> Export()
        {
            var decks = await db.GetUserDecks(User).ToListAsync();
            var jsonResult = new JsonResult(decks);

            return jsonResult;
        }

        public ActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Import(ImportViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var streamReader = new StreamReader(model.File.OpenReadStream()))
                {
                    var jsonSerializer = new JsonSerializer();
                    var jsonReader = new JsonTextReader(streamReader);
                    var newDecks = jsonSerializer.Deserialize<IEnumerable<Deck>>(jsonReader);

                    var userName = User.Identity.Name;

                    foreach (var deck in newDecks)
                    {
                        deck.OwnerID = userName;
                    }

                    var userDecks = db.GetUserDecks(User);

                    db.Decks.RemoveRange(userDecks);
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
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var deck = await db.Decks.FindAsync(id);

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
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var deck = await db.Decks.FindAsync(id);

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
                var dbDeck = await db.Decks.FindAsync(deck.ID);

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

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var deck = await db.Decks.FindAsync(id);

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
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var deck = await db.Decks.FindAsync(id);

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
