using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Services;
using ReadingListPlus.Services.Attributes;
using ReadingListPlus.Services.ViewModels;
using ReadingListPlus.Web.Core.Pages.Decks;

namespace ReadingListPlus.Web.Core.Pages.Cards
{
    public class CardEditModel : PageModel
    {
        public const string PageName = "/Cards/Edit";

        private readonly ICardService cardService;

        public CardEditModel(ICardService cardService)
        {
            this.cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
        }

        public async Task<ActionResult> OnGetAsync([Required, CardFound, CardOwned]Guid? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Card = await cardService.GetCardForEditingAsync(id.Value);

            return Page();
        }

        [BindProperty]
        public EditCardViewModel Card { get; set; }

        public async Task<ActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                CardViewModel viewModel = await cardService.UpdateAsync(Card);
                return RedirectToPage(DeckDetailsModel.PageName, new { Id = viewModel.DeckID });
            }
            else
            {
                return Page();
            }
        }
    }
}