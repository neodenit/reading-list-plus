﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReadingListPlus.DataAccess;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.Repositories
{
    public class DeckRepository : IDeckRepository
    {
        private readonly IApplicationContext context;

        public DeckRepository(IApplicationContext context)
        {
            this.context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public IAsyncEnumerable<Deck> GetAllDecks() =>
            context.Decks.Include(d => d.Cards).AsAsyncEnumerable();

        public IAsyncEnumerable<Deck> GetUserDecks(string userName) =>
            context.Decks.Include(d => d.Cards).Where(item => item.OwnerID == userName).AsAsyncEnumerable();

        public Task<Deck> GetDeckAsync(Guid id) =>
            context.Decks.Include(d => d.Cards).ThenInclude(c => c.ChildCards).SingleOrDefaultAsync(d => d.ID == id);

        public Deck GetDeck(Guid id) =>
            context.Decks.Include(d => d.Cards).SingleOrDefault(d => d.ID == id);

        public void AddRange(IEnumerable<Deck> newDecks)
        {
            context.Decks.AddRange(newDecks);
        }

        public void Add(Deck deck)
        {
            context.Decks.Add(deck);
        }

        public void RemoveAll()
        {
            context.Decks.RemoveRange(context.Decks);
        }

        public void Remove(Deck deck)
        {
            context.Decks.Remove(deck);
        }

        public Task SaveChangesAsync() => context.SaveChangesAsync();
    }
}
