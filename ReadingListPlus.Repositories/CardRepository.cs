﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReadingListPlus.DataAccess;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly IApplicationContext context;

        public CardRepository(IApplicationContext context)
        {
            this.context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public IAsyncEnumerable<Card> GetAllCards() =>
            context.Cards.AsQueryable().AsAsyncEnumerable();

        public IAsyncEnumerable<Card> GetUnparentedCards(string userName) =>
            context.Cards.AsQueryable().Where(c => c.DeckID == null && c.OwnerID == userName).AsAsyncEnumerable();

        public Task<Card> GetCardAsync(Guid id) =>
            context.Cards.Include(c => c.Deck.Cards).SingleOrDefaultAsync(c => c.ID == id);

        public Card GetCard(Guid id) =>
            context.Cards.Include(c => c.Deck.Cards).SingleOrDefault(c => c.ID == id);

        public async Task AddAsync(Card card) =>
            await context.Cards.AddAsync(card);

        public void RemoveAll()
        {
            context.Cards.RemoveRange(context.Cards);
        }

        public void Remove(Card card) =>
            context.Cards.Remove(card);

        public Task SaveChangesAsync() => context.SaveChangesAsync();
    }
}
