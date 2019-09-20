﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Services;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Web.Core.Pages.Decks
{
    public class DeckIndexModel : PageModel
    {
        public const string PageName = "/Decks/Index";

        private readonly IDeckService deckService;

        public IEnumerable<DeckViewModel> Decks { get; set; }

        public DeckIndexModel(IDeckService deckService)
        {
            this.deckService = deckService ?? throw new System.ArgumentNullException(nameof(deckService));
        }

        public async Task OnGet()
        {
            IAsyncEnumerable<DeckViewModel> decks = deckService.GetUserDecks(User.Identity.Name);
            var orderedDecks = await decks.OrderBy(d => d.Title).ToList();
            Decks = orderedDecks;
        }
    }
}