using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadingListPlus.Common;
using ReadingListPlus.Services;
using ReadingListPlus.Services.Attributes;
using ReadingListPlus.Services.ViewModels;
using ReadingListPlus.Web.Core.Pages.Cards;

namespace ReadingListPlus.Web.Core.Controllers
{
    [Authorize]
    public class DecksController : Controller
    {
        public const string Name = "Decks";

        private readonly IDeckService deckService;

        private string UserName => User.Identity.Name;

        public DecksController(IDeckService deckService)
        {
            this.deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
        }

        public async Task<ActionResult> Read([Required, DeckFound, DeckOwned]Guid? id, bool? isBookmarked)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Guid cardId = await deckService.GetFirstCardIdOrDefaultAsync(id.Value);

            return cardId == Guid.Empty
                ? RedirectToPage(CardCreateModel.PageName, new { DeckId = id })
                : RedirectToPage(CardReadModel.PageName, new { Id = cardId, IsBookmarked = isBookmarked });
        }

        [Authorize(Policy = Constants.BackupPolicy)]
        public async Task<ActionResult> Export(string id)
        {
            _ = id;

            string exportData = await deckService.GetExportDataAsync();
            return Content(exportData, MediaTypeNames.Application.Json);
        }

        public async Task<ActionResult> Tree(string id)
        {
            _ = id;

            IEnumerable<DeckViewModel> decks = await deckService.GetUserDecksAsync(UserName);

            var json = decks.Select(d => new
            {
                d.ID,
                Text = d.Title,
                a_attr = new { href = Url.Action(nameof(Read), Name, new { d.ID }) },
                Children = true
            });

            return Json(json);
        }
    }
}
