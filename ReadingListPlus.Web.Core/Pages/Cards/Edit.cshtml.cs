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
    public class CardEditModel : PageModel
    {
        public const string PageName = "/Cards/Edit";

        private readonly ICardService cardService;

        public CardEditModel(ICardService cardService)
        {
            this.cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
        }

        public async Task<ActionResult> OnGetAsync([Required, CardFound, CardOwned]Guid? id, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Card = await cardService.GetCardForEditingAsync(id.Value);

            ReturnUrl = returnUrl;

            return Page();
        }

        [BindProperty]
        public EditCardViewModel Card { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; }

        public async Task<ActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                await cardService.UpdateAsync(Card);

                return string.IsNullOrEmpty(ReturnUrl)
                    ? RedirectToPage(DeckIndexModel.PageName) as ActionResult
                    : Redirect(ReturnUrl);
            }
            else
            {
                return Page();
            }
        }
    }
}