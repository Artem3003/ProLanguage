using Domain.Entities;

namespace Domain.Interfaces;

public interface IBanRepository : IRepository<Ban>
{
    Task<Ban?> GetActiveBanByUserNameAsync(string userName);
}
