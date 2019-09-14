using System.Linq;
using System.Threading.Tasks;
using ReadingListPlus.DataAccess;

namespace ReadingListPlus.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IApplicationContext context;

        public UserRepository(IApplicationContext context)
        {
            this.context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public ApplicationUser GetUser(string userName) =>
            context.Users.Single(u => u.UserName == userName);

        public Task SaveChangesAsync() =>
            context.SaveChangesAsync();
    }
}
