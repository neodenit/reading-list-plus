using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Services;
using ReadingListPlus.Services.Attributes;
using ReadingListPlus.Services.ViewModels;
using ReadingListPlus.Web.Core.Pages.Decks;

namespace ReadingListPlus.Web.Core.Pages.Cards
{
    [Authorize]
    public class CardDeleteModel : PageModel
    {
        public const string PageName = "/Cards/Delete";

        private readonly ICardService cardService;

        public CardDeleteModel(ICardService cardService)
        {
            this.cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
        }

        public async Task<ActionResult> OnGetAsync([Required, CardFound, CardOwned]Guid? id)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Card = await cardService.GetCardAsync(id.Value);

            return Page();
        }

        public CardViewModel Card { get; set; }

        public async Task<ActionResult> OnPostAsync([CardFound, CardOwned]Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await cardService.RemoveAsync(id);

            return RedirectToPage(DeckIndexModel.PageName);
        }
    }
}