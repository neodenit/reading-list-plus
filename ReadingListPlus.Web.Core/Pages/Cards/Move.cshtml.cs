using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using ReadingListPlus.Common;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.Services;
using ReadingListPlus.Services.Attributes;
using ReadingListPlus.Services.ViewModels;
using ReadingListPlus.Web.Core.Pages.Decks;

namespace ReadingListPlus.Web.Core.Pages.Cards
{
    [Authorize]
    public class CardMoveModel : PageModel
    {
        public const string PageName = "/Cards/Move";
        private readonly ISettings settings;
        private readonly ICardService cardService;
        private readonly IDeckService deckService;

        public CardMoveModel(ISettings settings, ICardService cardService, IDeckService deckService)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            this.deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
        }

        public async Task<ActionResult> OnGetAsync([Required, CardFound, CardOwned]Guid? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Card = await cardService.GetCardAsync(id.Value);

            DeckListItems = await deckService
                .GetUserDecks(User.Identity.Name)
                .Where(d => d.ID != Card.DeckID)
                .ToList();

            PriorityList = settings.AllowHighestPriority
                ? cardService.GetFullPriorityList()
                : cardService.GetShortPriorityList();

            return Page();
        }

        [BindProperty]
        public CardViewModel Card { get; set; }

        public IEnumerable<DeckViewModel> DeckListItems { get; set; }

        public IEnumerable<KeyValuePair<string, string>> PriorityList { get; set; }

        [Required]
        [BindProperty]
        public Guid? NewDeckId { get; set; }

        [Required]
        [BindProperty]
        public Priority? Priority { get; set; }

        public async Task<ActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                await cardService.MoveAsync(Card.ID, NewDeckId.Value, Priority.Value);

                return RedirectToPage(DeckIndexModel.PageName);
            }
            else
            {
                DeckListItems = await deckService
                    .GetUserDecks(User.Identity.Name)
                    .Where(d => d.ID != Card.DeckID)
                    .ToList();

                PriorityList = settings.AllowHighestPriority
                    ? cardService.GetFullPriorityList()
                    : cardService.GetShortPriorityList();

                return Page();
            }
        }
    }
}