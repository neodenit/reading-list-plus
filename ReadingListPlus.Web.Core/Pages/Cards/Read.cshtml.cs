using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using ReadingListPlus.Common.App_GlobalResources;
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
        private readonly ITextActionService textActionService;
        private readonly IRepetitionCardService repetitionCardService;

        public CardReadModel(ICardService cardService, ITextActionService textActionService, IRepetitionCardService repetitionCardService)
        {
            this.cardService = cardService ?? throw new System.ArgumentNullException(nameof(cardService));
            this.textActionService = textActionService ?? throw new ArgumentNullException(nameof(CardReadModel.textActionService));
            this.repetitionCardService = repetitionCardService ?? throw new ArgumentNullException(nameof(repetitionCardService));
        }

        public async Task<ActionResult> OnGetAsync([Required, CardFound, CardOwned]Guid? id, bool? isBookmarked)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Card = await cardService.GetCardForReadingAsync(id.Value);

            IsBookmarked = isBookmarked.GetValueOrDefault();

            return Page();
        }

        [BindProperty]
        public ReadCardViewModel Card { get; set; }

        [BindProperty]
        public bool IsBookmarked { get; set; }

        public string PositionMessage => Resources.PositionConfirmation;

        public string BookmarkMessage => Resources.BookmarkConfirmation;

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
                case CardAction.Postpone:
                    CardViewModel cardViewModel = await cardService.PostponeAsync(Card.ID, Card.Priority.Value);
                    return RedirectToAction(nameof(DecksController.Read), DecksController.Name, new { Id = cardViewModel.DeckID });
                case CardAction.Extract:
                    CreateCardViewModel viewModel = await textActionService.ExtractAsync(Card.ID, Card.Selection, User.Identity.Name);
                    TempData[nameof(CreateCardViewModel)] = JsonConvert.SerializeObject(viewModel);
                    return RedirectToPage(CardCreateModel.PageName);
                case CardAction.Bookmark:
                    await textActionService.BookmarkAsync(Card.ID, Card.Selection);
                    return RedirectToPage(PageName, new { Id = Card.ID, IsBookmarked = true });
                case CardAction.Highlight:
                    await textActionService.HighlightAsync(Card.ID, Card.Selection);
                    return RedirectToPage(PageName, new { Id = Card.ID, IsBookmarked });
                case CardAction.Cloze:
                    await textActionService.ClozeAsync(Card.ID, Card.Selection);
                    return RedirectToPage(PageName, new { Id = Card.ID, IsBookmarked });
                case CardAction.DeleteRegion:
                    await textActionService.DeleteRegionAsync(Card.ID, Card.Selection);
                    return RedirectToPage(PageName, new { Id = Card.ID });
                case CardAction.Remember:
                    Uri uri = await textActionService.RememberAsync(Card.ID, Card.Selection);
                    return Redirect(uri.AbsoluteUri);
                case CardAction.CancelRepetitionCardCreation:
                    await textActionService.CancelRepetitionCardCreationAsync(Card.ID);
                    return RedirectToPage(PageName, new { Id = Card.ID });
                case CardAction.CompleteRepetitionCardCreation:
                    await textActionService.CompleteRepetitionCardCreationAsync(Card.ID);
                    return RedirectToPage(PageName, new { Id = Card.ID });
                default:
                    return BadRequest();
            }
        }
    }
}