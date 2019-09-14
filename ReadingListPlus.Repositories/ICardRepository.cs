using Microsoft.EntityFrameworkCore;
using ReadingListPlus.DataAccess.Models;
using System;
using System.Threading.Tasks;

namespace ReadingListPlus.Repositories
{
    public interface ICardRepository
    {
        DbSet<Card> Cards { get; }

        Task<Card> GetCardAsync(Guid id);

        Card GetCard(Guid id);

        void RemoveAll();
    }
}