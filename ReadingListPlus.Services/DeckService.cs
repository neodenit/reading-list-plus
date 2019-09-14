﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ReadingListPlus.DataAccess;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Repositories;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Services
{
    public class DeckService : IDeckService
    {
        private readonly IDeckRepository deckRepository;
        private readonly ICardRepository cardRepository;
        private readonly ISchedulerService schedulerService;

        public DeckService(IDeckRepository deckRepository, ICardRepository cardRepository, ISchedulerService schedulerService)
        {
            this.deckRepository = deckRepository ?? throw new System.ArgumentNullException(nameof(deckRepository));
            this.cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
            this.schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
        }

        public DbSet<Deck> Decks => deckRepository.Decks;

        public DbSet<ApplicationUser> Users => deckRepository.Users;

        public IAsyncEnumerable<DeckViewModel> GetUserDecks(string userName)
        {
            var items = deckRepository.GetUserDecks(userName);

            var viewModel = items
                .Select(d => new DeckViewModel
                {
                    ID = d.ID,
                    Title = d.Title,
                    CardCount = d.ConnectedCards.Count()
                });

            return viewModel;
        }

        public Task<Deck> GetDeckAsync(Guid id) => deckRepository.GetDeckAsync(id);

        public async Task<DeckViewModel> GetDeckViewModelAsync(Guid id)
        {
            Deck deck = await deckRepository.GetDeckAsync(id);
            var viewModel = new DeckViewModel {
                ID = deck.ID,
                Title = deck.Title,
                CardCount = deck.ConnectedCards.Count()
            };

            return viewModel;
        }

        public async Task<string> GetExportDataAsync()
        {
            IAsyncEnumerable<Deck> decks = deckRepository.GetAllDecks();

            var orderedDecks = await decks
                .OrderBy(d => d.OwnerID)
                .ThenBy(d => d.Title)
                .ToList();

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

        public async Task ImportAsync(ImportViewModel model, bool resetKeysOnImport)
        {
            using (var streamReader = new StreamReader(model.File.OpenReadStream()))
            {
                var jsonSerializer = new JsonSerializer();
                var jsonReader = new JsonTextReader(streamReader);
                var newDecks = jsonSerializer.Deserialize<IEnumerable<Deck>>(jsonReader);

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

                cardRepository.RemoveAll();
                deckRepository.RemoveAll();
                deckRepository.AddRange(newDecks);

                await deckRepository.SaveChangesAsync();
            }
        }

        public async Task<Guid> GetFirstCardIdOrDefaultAsync(Guid deckId)
        {
            Deck deck = await deckRepository.GetDeckAsync(deckId);
            var cards = deck.ConnectedCards;
            var result = cards.Any() ? schedulerService.GetFirstCard(cards).ID : Guid.Empty;
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

            deckRepository.Remove(deck);

            await deckRepository.SaveChangesAsync();
        }

        public Task<int> SaveChangesAsync() => deckRepository.SaveChangesAsync();
    }
}
