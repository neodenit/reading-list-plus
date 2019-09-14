using System.Threading.Tasks;
using ReadingListPlus.DataAccess;

namespace ReadingListPlus.Repositories
{
    public interface IUserRepository
    {
        ApplicationUser GetUser(string userName);

        Task SaveChangesAsync();
    }
}