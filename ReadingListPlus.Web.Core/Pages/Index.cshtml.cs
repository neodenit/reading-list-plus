using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Web.Core.Pages.Decks;

namespace ReadingListPlus.Web.Core.Pages
{
    public class HomeIndexModel : PageModel
    {
        public const string PageName = "/Index";

        public ActionResult OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToPage(DeckIndexModel.PageName);
            }
            else
            {
                return Page();
            }
        }
    }
}