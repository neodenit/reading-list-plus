﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ReadingListPlus.Web.Models;
using Boilerpipe.Net.Extractors;
using System.IO;

namespace ReadingListPlus.Web.Controllers
{
    [Authorize]
#if !DEBUG
[RequireHttps]
#endif
    public class CardsController : Controller
    {
        private ReadingListPlusContext db = new ReadingListPlusContext();

        // GET: Cards
        public async Task<ActionResult> Index(int? DeckID)
        {
            if (DeckID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var deck = await db.Decks.FindAsync(DeckID);

                if (!deck.IsAuthorized(User))
                {
                    return new HttpUnauthorizedResult();
                }
                else
                {
                    return View(deck.Cards.OrderBy(c => c.Position).ToList());
                }
            }
        }

        public ActionResult Fix(int DeckID)
        {
            var deck = db.Decks.Find(DeckID);

            var cards = deck.Cards.OrderBy(item => item.Position).ToList();

            Enumerable.Range(0, cards.Count).Zip(cards, (i, item) => new { i = i, card = item }).ToList().ForEach(item => item.card.Position = item.i);

            db.SaveChanges();

            return View("Index", deck.Cards);
        }

        public async Task<ActionResult> Details(int? id, int? DeckID)
        {
            if (id == null)
            {
                if (DeckID == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                else
                {
                    return View(new Card { DeckID = DeckID.Value, ID = -1 });
                }
            }
            else
            {
                var card = await db.Cards.FindAsync(id);

                if (card == null)
                {
                    return HttpNotFound();
                }
                else if (!card.IsAuthorized(User))
                {
                    return new HttpUnauthorizedResult();
                }
                else
                {
                    ViewBag.Markup = TextConverter.GetHtml(card.Text);

                    return View(card);
                }
            }
        }

        [HttpPost]
        public Task<ActionResult> Details([Bind(Include = "ID, NextAction, Selection")] Card card)
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
                default:
                    throw new Exception();
            }
        }

        // GET: Cards/Create/5
        public ActionResult Create(int? DeckID, string text)
        {
            if (DeckID == null)
            {
                ViewBag.DeckID = new SelectList(db.GetUserDecks(User), "ID", "Title");

                var card = new CreateCardViewModel { Text = text };

                return View(card);
            }
            else
            {
                var card = new CreateCardViewModel { DeckID = DeckID.Value, Text = text };

                return View(card);
            }
        }

        public async Task<ActionResult> CreateFromUrl(string url)
        {
            ViewBag.DeckID = new SelectList(db.GetUserDecks(User), "ID", "Title");

            var urlParameter = Uri.EscapeDataString(url);
            var fullUrl = $"https://boilerpipe-web.appspot.com/extract?url={urlParameter}&output=text";

            var request = WebRequest.Create(fullUrl);
            var responce = await request.GetResponseAsync();
            var responseStream = responce.GetResponseStream();
            var streamReader = new StreamReader(responseStream);

            var text = await streamReader.ReadToEndAsync();
            var formattedText = text.Replace("\n", Environment.NewLine + Environment.NewLine);
            var card = new CreateCardViewModel { Text = formattedText, Url = url };

            return View("Create", card);
        }

        // POST: Cards/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,DeckID,Title,Text,Priority")] CreateCardViewModel card)
        {
            card.Text = card.Text.Trim();

            if (ModelState.IsValid)
            {
                var deck = db.Decks.Find(card.DeckID);

                if (!deck.IsAuthorized(User))
                {
                    return new HttpUnauthorizedResult();
                }
                else
                {
                    Scheduler.PrepareForAdding(deck, deck.Cards, card, card.Priority.Value);

                    db.Cards.Add(card);

                    await db.SaveChangesAsync();

                    return RedirectToDeckDetails(card.DeckID);
                }
            }
            else
            {
                return View(card);
            }
        }

        // GET: Cards/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var card = await db.Cards.FindAsync(id);

            if (card == null)
            {
                return HttpNotFound();
            }
            else if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                return View(card);
            }
        }

        // POST: Cards/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,Title,Text,Url")] Card card)
        {
            if (ModelState.IsValid)
            {
                var dbCard = await db.Cards.FindAsync(card.ID);

                if (!dbCard.IsAuthorized(User))
                {
                    return new HttpUnauthorizedResult();
                }
                else
                {
                    dbCard.Title = card.Title;
                    dbCard.Text = card.Text;
                    dbCard.Url = card.Url;

                    await db.SaveChangesAsync();

                    return RedirectToAction("Details", "Decks", new { id = dbCard.DeckID });
                }
            }
            else
            {
                return View(card);
            }
        }

        #region Actions
        public async Task<ActionResult> Postpone(int ID, string Priority)
        {
            var card = await db.Cards.FindAsync(ID);

            if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else if (card.Position != 0)
            {
                return RedirectToDeckDetails(card.DeckID);
            }
            else
            {
                var deck = card.Deck;
                var deckCards = deck.Cards;

                var priority = Scheduler.ParsePriority(Priority);

                Scheduler.ChangeFirstCardPosition(deck, deckCards, card, priority);

                await db.SaveChangesAsync();

                return RedirectToDeckDetails(card.DeckID);
            }
        }

        public async Task<ActionResult> Highlight(int ID, string text)
        {
            var card = await db.Cards.FindAsync(ID);

            if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                var newText = TextConverter.AddHighlight(card.Text, text);

                card.Text = newText;

                await db.SaveChangesAsync();

                return RedirectToDeckDetails(card.DeckID);
            }
        }

        public async Task<ActionResult> Cloze(int ID, string text)
        {
            var card = await db.Cards.FindAsync(ID);

            if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                var newText = TextConverter.AddCloze(card.Text, text);

                card.Text = newText;

                await db.SaveChangesAsync();

                return RedirectToDeckDetails(card.DeckID);
            }
        }

        public async Task<ActionResult> Extract(int ID, string text)
        {
            var card = await db.Cards.FindAsync(ID);

            if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                ModelState.Clear();

                var newText = TextConverter.ReplaceTag(text, "selection", "extract");

                card.Text = newText;

                await db.SaveChangesAsync();

                var cards = card.Deck.Cards;

                var selection = TextConverter.GetSelection(text);

                var newCard = new CreateCardViewModel { DeckID = card.DeckID, Url = card.Url, ParentCardID = card.ID, Text = selection };

                return View("Create", newCard);
            }
        }

        public async Task<ActionResult> Bookmark(int ID, string text)
        {
            var card = await db.Cards.FindAsync(ID);

            if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                var textWithoutBookmarks = TextConverter.DeleteTagByName(text, "bookmark");

                var newText = TextConverter.ReplaceTag(textWithoutBookmarks, "selection", "bookmark");

                card.Text = newText;

                await db.SaveChangesAsync();

                return RedirectToDeckDetails(card.DeckID);
            }
        }

        public async Task<ActionResult> DeleteRegion(int ID, string text)
        {
            var card = await db.Cards.FindAsync(ID);

            if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                var newText = TextConverter.DeleteTagByText(card.Text, text);

                card.Text = newText;

                await db.SaveChangesAsync();

                return RedirectToDeckDetails(card.DeckID);
            }
        }
        #endregion

        // GET: Cards/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var card = await db.Cards.FindAsync(id);

            if (card == null)
            {
                return HttpNotFound();
            }
            else if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                return View(card);
            }
        }

        // POST: Cards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var card = await db.Cards.Include(c => c.Deck.Cards).SingleAsync(c => c.ID == id);

            if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }

            var cards = card.Deck.Cards;

            Scheduler.PrepareForDeletion(cards, card);

            db.Cards.Remove(card);

            await db.SaveChangesAsync();

            return RedirectToDeckDetails(card.DeckID);
        }

        private ActionResult RedirectToDeckDetails(int deckID)
        {
            return RedirectToAction("Details", "Decks", new { id = deckID });
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
