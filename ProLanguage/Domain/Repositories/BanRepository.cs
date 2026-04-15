using Domain.Data;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Domain.Repositories;

public class BanRepository(ApplicationDbContext context) : AbstractRepository<Ban>(context), IBanRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Ban?> GetActiveBanByUserNameAsync(string userName)
    {
        return await _context.Bans
            .Where(b => b.UserName == userName &&
                        (b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow))
            .OrderByDescending(b => b.BannedAt)
            .FirstOrDefaultAsync();
    }
}
