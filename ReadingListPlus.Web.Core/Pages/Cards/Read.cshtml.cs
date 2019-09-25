using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using ReadingListPlus.Services;
using ReadingListPlus.Services.Attributes;
using ReadingListPlus.Services.ViewModels;
using ReadingListPlus.Web.Core.Controllers;

namespace ReadingListPlus.Web.Core.Pages.Cards
{
    [Authorize]
    public class CardReadModel : PageModel
    {
        public const string PageName = "/Cards/Read";

        private readonly ICardService cardService;
        private readonly IRepetitionCardService repetitionCardService;

        public CardReadModel(ICardService cardService, IRepetitionCardService repetitionCardService)
        {
            this.cardService = cardService ?? throw new System.ArgumentNullException(nameof(cardService));
            this.repetitionCardService = repetitionCardService ?? throw new ArgumentNullException(nameof(repetitionCardService));
        }

        public async Task<ActionResult> OnGetAsync([Required, CardFound, CardOwned]Guid? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Card = await cardService.GetCardForReadingAsync(id.Value);

            return Page();
        }

        [BindProperty]
        public CardViewModel Card { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool isActionValid = await repetitionCardService.IsActionValidAsync(Card.ID, Card.NextAction);

            if (!isActionValid)
            {
                return BadRequest();
            }

            switch (Card.NextAction)
            {
                case "Extract":
                    CreateCardViewModel viewModel = await cardService.ExtractAsync(Card.ID, Card.Selection, User.Identity.Name);
                    TempData[nameof(CreateCardViewModel)] = JsonConvert.SerializeObject(viewModel);
                    return RedirectToPage(CardCreateModel.PageName);
                case "Cloze":
                    return RedirectToPage(PageName, new { Id = await cardService.ClozeAsync(Card.ID, Card.Selection) });
                case "Highlight":
                    return RedirectToPage(PageName, new { Id = await cardService.HighlightAsync(Card.ID, Card.Selection) });
                case "Bookmark":
                    Card = await cardService.BookmarkAsync(Card.ID, Card.Selection);
                    return Page();
                case "Remember":
                    Uri uri = await cardService.RememberAsync(Card.ID, Card.Selection);
                    return Redirect(uri.AbsoluteUri);
                case "DeleteRegion":
                    return RedirectToPage(PageName, new { Id = await cardService.DeleteRegionAsync(Card.ID, Card.Selection) });
                case "CancelRepetitionCardCreation":
                    return RedirectToPage(PageName, new { Id = await cardService.CancelRepetitionCardCreationAsync(Card.ID) });
                case "CompleteRepetitionCardCreation":
                    return RedirectToPage(PageName, new { Id = await cardService.CompleteRepetitionCardCreationAsync(Card.ID) });
                case "Postpone":
                    CardViewModel cardViewModel = await cardService.PostponeAsync(Card.ID, Card.Priority.Value);
                    return RedirectToAction(nameof(DecksController.Read), DecksController.Name, new { Id = cardViewModel.DeckID });
                default:
                    return BadRequest();
            }
        }
    }
}