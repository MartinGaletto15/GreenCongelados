using Domain.Entities;

namespace Domain.Interfaces;

public interface IAddressRepository : IGenericRepository<Address>
{
    Task<Address?> GetByUserIdAsync(int userId);
}
