using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ReadingListPlus.Common;
using ReadingListPlus.Services;
using ReadingListPlus.Services.Attributes;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Web.Core.Controllers
{
    [Authorize]
    public class DecksController : Controller
    {
        public const string Name = "Decks";

        private readonly IDeckService deckService;
        private readonly ISettings settings;

        private string UserName => User.Identity.Name;

        public DecksController(IDeckService deckService, ISettings settings)
        {
            this.deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task<ActionResult> Index()
        {
            IAsyncEnumerable<DeckViewModel> decks = deckService.GetUserDecks(UserName);
            var orderedDecks = await decks.OrderBy(d => d.Title).ToList();
            return View(orderedDecks);
        }

        [Authorize(Policy = Constants.BackupPolicy)]
        public async Task<ActionResult> Export(string id)
        {
            _ = id;

            string exportData = await deckService.GetExportDataAsync();
            return Content(exportData, MediaTypeNames.Application.Json);
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
                await deckService.ImportAsync(model, settings.ResetKeysOnImport);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View();
            }
        }

        public async Task<ActionResult> Details([Required, DeckFound, DeckOwned]Guid? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Guid cardId = await deckService.GetFirstCardIdOrDefaultAsync(id.Value);

            return RedirectToAction(nameof(CardsController.Details), CardsController.Name, new { Id = cardId, DeckId = id.Value });
        }

        public ActionResult Create()
        {
            var deck = new CreateDeckViewModel();
            return View(deck);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateDeckViewModel deckViewModel)
        {
            if (ModelState.IsValid)
            {
                await deckService.CreateDeckAsync(deckViewModel.Title, UserName);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(deckViewModel);
            }
        }

        public async Task<ActionResult> Edit([Required, DeckFound, DeckOwned]Guid? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DeckViewModel viewModel = await deckService.GetDeckViewModelAsync(id.Value);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind("ID", "Title")] DeckViewModel deck)
        {
            if (ModelState.IsValid)
            {
                await deckService.UpdateDeckTitleAsync(deck.ID, deck.Title);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(deck);
            }
        }

        public async Task<ActionResult> Delete([Required, DeckFound, DeckOwned]Guid? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DeckViewModel viewModel = await deckService.GetDeckViewModelAsync(id.Value);
            return View(viewModel);
        }

        [HttpPost, ActionName(nameof(Delete))]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed([DeckFound, DeckOwned]Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await deckService.DeleteDeckAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}
