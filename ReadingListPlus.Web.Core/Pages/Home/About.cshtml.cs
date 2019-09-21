using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Common;

namespace ReadingListPlus.Web.Core.Pages.Home
{
    public class AboutModel : PageModel
    {
        public const string PageName = "/Home/About";

        public void OnGet()
        {
            ViewData[Constants.ViewMessage] = "Application description page.";
        }
    }
}