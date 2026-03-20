using Domain.Entities;

namespace Domain.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetByNameAsync(string name);
}
