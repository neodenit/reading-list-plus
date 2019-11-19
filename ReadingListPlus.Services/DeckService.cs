using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ReadingListPlus.Common;
using ReadingListPlus.Common.App_GlobalResources;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Repositories;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Services
{
    public class DeckService : IDeckService
    {
        private readonly IMapper mapper;
        private readonly IDeckRepository deckRepository;
        private readonly ICardRepository cardRepository;
        private readonly IUserRepository userRepository;
        private readonly ISchedulerService schedulerService;
        private readonly ITextConverterService textConverterService;
        private readonly ICardService cardService;

        public DeckService(IMapper mapper, IDeckRepository deckRepository, ICardRepository cardRepository, IUserRepository userRepository, ISchedulerService schedulerService, ITextConverterService textConverterService, ICardService cardService)
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.deckRepository = deckRepository ?? throw new System.ArgumentNullException(nameof(deckRepository));
            this.cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            this.textConverterService = textConverterService ?? throw new ArgumentNullException(nameof(textConverterService));
            this.cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
        }

        public async Task<IEnumerable<DeckViewModel>> GetUserDecksAsync(string userName)
        {
            IAsyncEnumerable<Deck> decks = deckRepository.GetUserDecks(userName);

            var deckList = new List<Deck>();

            await foreach (var deck in decks)
            {
                deckList.Add(deck);
            }

            var viewModel = deckList
                .OrderBy(d => d.Title)
                .Select(d => new DeckViewModel
                {
                    ID = d.ID,
                    Title = d.Title,
                    CardCount = d.ConnectedCards.Count()
                });


            return viewModel;
        }

        public async Task<DeckViewModel> GetDeckAsync(Guid id)
        {
            Deck deck = await deckRepository.GetDeckAsync(id);
            var viewModel = new DeckViewModel
            {
                ID = deck.ID,
                Title = deck.Title,
                CardCount = deck.ConnectedCards.Count()
            };

            return viewModel;
        }

        public async Task<string> GetExportDataAsync()
        {
            IAsyncEnumerable<Deck> decks = deckRepository.GetAllDecks();
            var exportDecks = mapper.Map<IEnumerable<ImportExportDeck3>>(decks);

            var orderedDecks = exportDecks
                .OrderBy(d => d.OwnerID)
                .ThenBy(d => d.Title);

            foreach (var deck in orderedDecks)
            {
                deck.Cards = deck.Cards
                    .OrderBy(c => c.Position)
                    .ThenBy(c => c.Text)
                    .ToList();
            }

            var result = JsonConvert.SerializeObject(orderedDecks, Formatting.Indented);
            return result;
        }

        public async Task ImportAsync(Stream stream, bool resetKeysOnImport, bool fixOnImport)
        {
            IEnumerable<IImportExportDeck> GetDecksFromStream()
            {
                using (var streamReader = new StreamReader(stream))
                {
                    var jsonSerializer = new JsonSerializer { MissingMemberHandling = MissingMemberHandling.Error };

                    try
                    {
                        using (var jsonReader = new JsonTextReader(streamReader) { CloseInput = false })
                        {
                            var decks = jsonSerializer.Deserialize<IEnumerable<ImportExportDeck3>>(jsonReader);
                            return decks;
                        }

                    }
                    catch (JsonSerializationException)
                    {
                        try
                        {
                            streamReader.BaseStream.Position = 0;
                            streamReader.DiscardBufferedData();

                            using (var jsonReader = new JsonTextReader(streamReader))
                            {
                                var decks = jsonSerializer.Deserialize<IEnumerable<ImportExportDeck2>>(jsonReader);
                                return decks;
                            }
                        }
                        catch (Exception)
                        {
                            streamReader.BaseStream.Position = 0;
                            streamReader.DiscardBufferedData();

                            using (var jsonReader = new JsonTextReader(streamReader))
                            {
                                var decks = jsonSerializer.Deserialize<IEnumerable<ImportExportDeck>>(jsonReader);
                                return decks;
                            }
                        }
                    }
                }
            }

            IEnumerable<IImportExportDeck> importDecks = GetDecksFromStream();
            var newDecks = mapper.Map<IEnumerable<Deck>>(importDecks);

            if (resetKeysOnImport)
            {
                foreach (var deck in newDecks)
                {
                    deck.ID = Guid.NewGuid();

                    foreach (var card in deck.Cards)
                    {
                        card.ID = Guid.NewGuid();
                        card.ParentCardID = null;
                    }
                }
            }

            if (fixOnImport)
            {
                foreach (var deck in newDecks)
                {
                    foreach (var card in deck.Cards)
                    {
                        card.OwnerID = deck.OwnerID;
                    }
                }
            }

            cardRepository.RemoveAll();
            deckRepository.RemoveAll();
            deckRepository.AddRange(newDecks);

            await deckRepository.SaveChangesAsync();
        }

        public async Task<Guid> GetFirstCardIdOrDefaultAsync(Guid deckId)
        {
            Deck deck = await deckRepository.GetDeckAsync(deckId);
            var result = deck.ConnectedCards.Any() ? schedulerService.GetFirstCard(deck).ID : Guid.Empty;
            return result;
        }

        public Task CreateDeckAsync(string title, string userName)
        {
            var deck = new Deck
            {
                ID = Guid.NewGuid(),
                Title = title,
                OwnerID = userName
            };

            deckRepository.Add(deck);

            return deckRepository.SaveChangesAsync();
        }

        public async Task UpdateDeckTitleAsync(Guid id, string title)
        {
            Deck deck = await deckRepository.GetDeckAsync(id);

            deck.Title = title;

            await deckRepository.SaveChangesAsync();
        }

        public async Task DeleteDeckAsync(Guid id)
        {
            Deck deck = await deckRepository.GetDeckAsync(id);

            foreach (var card in deck.Cards)
            {
                if (card.ParentCardID == null &&
                    card.ChildCards?.Any() != true &&
                    string.IsNullOrEmpty(textConverterService.GetTagText(card.Text, Constants.RepetitionCardLabel)))
                {
                    cardRepository.Remove(card);
                }
                else
                {
                    card.Position = Constants.DisconnectedCardPosition;
                }
            }

            deckRepository.Remove(deck);

            await deckRepository.SaveChangesAsync();
        }

        public Guid? GetUserLastDeck(string userName) =>
            userRepository.GetUser(userName).LastDeck;

        public Task SetUserLastDeckAsync(string userName, Guid deckId)
        {
            var user = userRepository.GetUser(userName);
            user.LastDeck = deckId;
            return userRepository.SaveChangesAsync();
        }

        public async Task FixDeckAsync(Guid deckId)
        {
            Deck deck = await deckRepository.GetDeckAsync(deckId);

            var cards = deck.ConnectedCards.OrderBy(item => item.Position).ToList();

            Enumerable.Range(Constants.FirstCardPosition, cards.Count)
                .Zip(cards, (i, item) => new { i, card = item })
                .ToList()
                .ForEach(item => item.card.Position = item.i);

            await deckRepository.SaveChangesAsync();
        }
    }
}
