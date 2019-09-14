using Microsoft.EntityFrameworkCore;
using ReadingListPlus.DataAccess.Models;
using System;
using System.Threading.Tasks;

namespace ReadingListPlus.Services
{
    public interface ICardService
    {
        Task<Card> GetCardAsync(Guid id);

        Task AddAsync(Card card);

        Task RemoveAsync(Card card);
    }
}