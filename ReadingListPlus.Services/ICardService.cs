using Microsoft.EntityFrameworkCore;
using ReadingListPlus.DataAccess.Models;
using System;
using System.Threading.Tasks;

namespace ReadingListPlus.Services
{
    public interface ICardService
    {
        DbSet<Card> Cards { get; }

        Task<Card> GetCardAsync(Guid id);
    }
}