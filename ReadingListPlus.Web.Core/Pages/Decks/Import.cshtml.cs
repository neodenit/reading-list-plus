using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Common;
using ReadingListPlus.Services;

namespace ReadingListPlus.Web.Core.Pages.Decks
{
    [Authorize(Policy = Constants.BackupPolicy)]
    public class ImportModel : PageModel
    {
        public const string PageName = "/Decks/Import";
        private readonly ISettings settings;
        private readonly IDeckService deckService;

        public ImportModel(ISettings settings, IDeckService deckService)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
        }

        [Required]
        [BindProperty]
        public IFormFile ImportFile { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await deckService.ImportAsync(ImportFile.OpenReadStream(), settings.ResetKeysOnImport, settings.FixOnImport);

            return RedirectToPage(DeckIndexModel.PageName);
        }
    }
}
