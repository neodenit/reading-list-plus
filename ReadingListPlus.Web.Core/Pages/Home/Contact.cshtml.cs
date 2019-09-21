using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Common;

namespace ReadingListPlus.Web.Core.Pages.Home
{
    public class ContactModel : PageModel
    {
        public const string PageName = "/Home/Contact";

        private readonly ISettings settings;

        public ContactModel(ISettings settings)
        {
            this.settings = settings ?? throw new System.ArgumentNullException(nameof(settings));
        }

        public string ContactEmail { get; set; }

        public void OnGet()
        {
            ViewData[Constants.ViewMessage] = "Contact page.";
            ContactEmail = settings.ContactEmail;
        }
    }
}