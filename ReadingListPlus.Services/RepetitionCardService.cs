using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReadingListPlus.Common;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Repositories;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Services
{
    public class RepetitionCardService : IRepetitionCardService
    {
        private readonly ISettings settings;
        private readonly IHttpClientWrapper httpClientWrapper;
        private readonly ITextConverterService textConverterService;
        private readonly ICardRepository cardRepository;

        public RepetitionCardService(ISettings settings, IHttpClientWrapper httpClientWrapper, ITextConverterService textConverterService, ICardRepository cardRepository)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.httpClientWrapper = httpClientWrapper ?? throw new ArgumentNullException(nameof(httpClientWrapper));
            this.textConverterService = textConverterService ?? throw new ArgumentNullException(nameof(textConverterService));
            this.cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
        }

        public async Task<bool> IsLocalIdValidAsync(Guid readingCardId, Guid repetitionCardId)
        {
            Card card = await cardRepository.GetCardAsync(readingCardId);
            string param = textConverterService.GetIdParameter(card.Text, Constants.NewRepetitionCardLabel);
            var reservedId = new Guid(param);
            var isValid = reservedId == repetitionCardId;
            return isValid;
        }

        public async Task<bool> IsActionValidAsync(Guid id, CardAction action)
        {
            Card card = await cardRepository.GetCardAsync(id);
            string newRepetitionCardText = textConverterService.GetNewRepetitionCardText(card.Text);

            if (string.IsNullOrEmpty(newRepetitionCardText))
            {
                return true;
            }

            var newRepetitionCardActions = new[] { CardAction.CancelRepetitionCardCreation, CardAction.CompleteRepetitionCardCreation };
            var isValid = newRepetitionCardActions.Contains(action);
            return isValid;
        }

        public async Task<bool> IsRemoteIdValidAsync(Guid readingCardId, Guid repetitionCardId)
        {
            try
            {
                var baseUri = new Uri(settings.SpacedRepetitionServer);
                var uri = new Uri(baseUri, $"Cards/IsValid?readingCardId={readingCardId}&repetitionCardId={repetitionCardId}");
                HttpResponseMessage response = await httpClientWrapper.GetAsync(uri);
                var responseString = await response.Content.ReadAsStringAsync();
                var isValid = JsonConvert.DeserializeObject<bool>(responseString);
                return isValid;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}
