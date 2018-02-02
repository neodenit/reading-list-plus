﻿using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using ReadingListPlus.Persistence.Models;

namespace ReadingListPlus.Web.Controllers
{
    [Authorize]
#if !DEBUG
[RequireHttps]
#endif
    public class DecksController : Controller
    {
        private ReadingListPlusContext db = new ReadingListPlusContext();

        // GET: Decks
        public async Task<ActionResult> Index()
        {
            var items = db.GetUserDecks(User).OrderBy(d => d.Title);

            return View(await items.ToListAsync());
        }

        public async Task<ActionResult> Export()
        {
            var decks = await db.GetUserDecks(User).ToListAsync();
            var jsonResult = new JsonResult
            {
                Data = decks,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = int.MaxValue
            };

            return jsonResult;
        }

        // GET: Decks/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var deck = await db.Decks.FindAsync(id);

            if (deck == null)
            {
                return HttpNotFound();
            }
            else if (!deck.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }

            if (deck.Cards.Any())
            {
                var card = deck.Cards.GetMinElement(c => c.Position);

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
            var deck = new Deck { Coeff = Settings.Default.Coeff, StartDelay = Settings.Default.StartDelay };

            return View(deck);
        }

        // POST: Decks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Title,Type,StartDelay,Coeff")] Deck deck)
        {
            if (ModelState.IsValid)
            {
                deck.OwnerID = User.Identity.Name;

                db.Decks.Add(deck);

                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            else
            {
                return View(deck);
            }
        }

        // GET: Decks/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var deck = await db.Decks.FindAsync(id);

            if (deck == null)
            {
                return HttpNotFound();
            }
            else if (!deck.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                return View(deck);
            }
        }

        // POST: Decks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,Title,StartDelay,Coeff,Type")] Deck deck)
        {
            if (ModelState.IsValid)
            {
                var dbDeck = await db.Decks.FindAsync(deck.ID);

                if (deck == null)
                {
                    return HttpNotFound();
                }
                else if (!dbDeck.IsAuthorized(User))
                {
                    return new HttpUnauthorizedResult();
                }

                dbDeck.Title = deck.Title;
                dbDeck.StartDelay = deck.StartDelay;
                dbDeck.Coeff = deck.Coeff;
                dbDeck.Type = deck.Type;

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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var deck = await db.Decks.FindAsync(id);

            if (deck == null)
            {
                return HttpNotFound();
            }
            else if (!deck.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
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
                return new HttpUnauthorizedResult();
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
