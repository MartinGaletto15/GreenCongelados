namespace Aplication.Interfaces.ProductCategory;

public interface IProductCategoryWriteService
{
    Task AddCategoryToProductAsync(int productId, int categoryId);
    Task RemoveCategoryFromProductAsync(int productId, int categoryId);
    Task SyncCategoriesToProductAsync(int productId, IEnumerable<int> categoryIds);
}