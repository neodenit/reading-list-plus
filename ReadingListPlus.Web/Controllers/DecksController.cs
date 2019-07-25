using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ReadingListPlus.Persistence.Models;
using ReadingListPlus.Web.ViewModels;

namespace ReadingListPlus.Web.Controllers
{
    [Authorize]
    [RequireHttps]
    public class DecksController : Controller
    {
        private ReadingListPlusContext db = new ReadingListPlusContext();
        private const int MaxFileLength = int.MaxValue;

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
                MaxJsonLength = MaxFileLength
            };

            return jsonResult;
        }

        public ActionResult Import()
        {
            return View(new ImportViewModel());
        }

        [HttpPost]
        public async Task<ActionResult> Import(ImportViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var streamReader = new StreamReader(model.File.InputStream))
                {
                    var jsonString = await streamReader.ReadToEndAsync();
                    var javaScriptSerializer = new JavaScriptSerializer { MaxJsonLength = MaxFileLength };
                    var newDecks = javaScriptSerializer.Deserialize<IEnumerable<Deck>>(jsonString);

                    foreach (var deck in newDecks)
                    {
                        deck.OwnerID = User.Identity.Name;
                    }

                    var userDecks = db.GetUserDecks(User);
                    db.Decks.RemoveRange(userDecks);
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
            var deck = new Deck();

            return View(deck);
        }

        // POST: Decks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Title")] Deck deck)
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
        public async Task<ActionResult> Edit([Bind(Include = "ID, Title")] Deck deck)
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
