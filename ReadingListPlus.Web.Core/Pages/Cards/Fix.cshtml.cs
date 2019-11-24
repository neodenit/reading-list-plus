using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadingListPlus.Common;
using ReadingListPlus.Common.Enums;
using ReadingListPlus.Services;

namespace ReadingListPlus.Web.Core.Pages.Cards
{
    [Authorize(Policy = Constants.FixPolicy)]
    public class CardFixModel : PageModel
    {
        public const string PageName = "/Cards/Fix";

        private readonly ICardService cardService;

        public CardFixModel(ICardService cardService)
        {
            this.cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
        }

        public async Task OnPostFixAmpersandsAsync() =>
            await cardService.FixAsync(FixAction.FixAmpersands);

        public async Task OnPostFixSpacesAsync() =>
            await cardService.FixAsync(FixAction.FixSpaces);
    }
}