using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Common;
using ReadingListPlus.Services;

namespace ReadingListPlus.Web.Core.Pages.Decks
{
    [Authorize(Policy = Constants.BackupPolicy)]
    public class ExportModel : PageModel
    {
        public const string PageName = "/Decks/Export";

        private readonly IDeckService deckService;

        public ExportModel(IDeckService deckService)
        {
            this.deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
        }

        public async Task<ActionResult> OnGetAsync(string id)
        {

            _ = id;

            string exportData = await deckService.GetExportDataAsync();
            return Content(exportData, MediaTypeNames.Application.Json);
        }
    }
}