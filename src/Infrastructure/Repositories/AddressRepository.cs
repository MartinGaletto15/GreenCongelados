using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AddressRepository : GenericRepository<Address>, IAddressRepository
{
    public AddressRepository(GCContext context) : base(context)
    {
    }

    public async Task<Address?> GetByUserIdAsync(int userId)
    {
        return await _context.Addresses.FirstOrDefaultAsync(x => x.IdUser == userId);
    }
}