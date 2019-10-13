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
    public class CardRestoreModel : PageModel
    {
        public const string PageName = "/Cards/Restore";

        private readonly ICardService cardService;
        private readonly IDeckService deckService;

        public CardRestoreModel(ICardService cardService, IDeckService deckService)
        {
            this.cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            this.deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
        }

        public async Task<ActionResult> OnGetAsync([Required, CardFound, CardOwned]Guid? id, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            Card = await cardService.GetCardAsync(id.Value);

            if (Card.DeckID == null)
            {
                DeckListItems = await deckService.GetUserDecks(User.Identity.Name).ToList();
            }

            PriorityList = cardService.GetFullPriorityList();

            return Page();
        }

        [BindProperty]
        public string ReturnUrl { get; set; }

        [BindProperty]
        public CardViewModel Card { get; set; }

        public IEnumerable<DeckViewModel> DeckListItems { get; set; }

        public IEnumerable<KeyValuePair<string, string>> PriorityList { get; set; }

        [Required]
        [BindProperty]
        public Priority? Priority { get; set; }

        public async Task<ActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                Card.Priority = Priority;

                await cardService.RestoreAsync(Card);

                return string.IsNullOrEmpty(ReturnUrl)
                    ? RedirectToPage(DeckIndexModel.PageName) as ActionResult
                    : Redirect(ReturnUrl);
            }
            else
            {
                if (Card.DeckID == null)
                {
                    DeckListItems = await deckService.GetUserDecks(User.Identity.Name).ToList();
                }

                PriorityList = cardService.GetFullPriorityList();

                return Page();
            }
        }
    }
}