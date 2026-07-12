using EventManagement.Identity.Application.Interfaces;
using EventManagement.Identity.Domain.Entities;
using EventManagement.Identity.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Identity.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IdentityDbContext _appDbContext;

        public UserRepository(IdentityDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Users.AddAsync(user, cancellationToken);
        }

        public async Task<User?> GetUserByLoginAsync(string login, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(u => u.Login == login, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _appDbContext.SaveChangesAsync();
        }
    }
}
