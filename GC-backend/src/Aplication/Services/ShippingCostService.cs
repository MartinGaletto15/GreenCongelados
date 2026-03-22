using Aplication.Interfaces.ShippingCost;
using Applications.dtos;
using Applications.dtos.Requests;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;

namespace Aplication.Services;

public class ShippingCostService : IShippingCostReadOnlyService, IShippingCostWriteService
{
    private readonly IGenericRepository<ShippingCost> _repository;

    public ShippingCostService(IGenericRepository<ShippingCost> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ShippingCostDTO>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return ShippingCostDTO.Create(entities);
    }

    public async Task<ShippingCostDTO> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("ShippingCost not found", "SHIPPINGCOST_NOT_FOUND");
        return ShippingCostDTO.Create(entity);
    }

    public async Task<ShippingCostDTO?> GetActiveAsync()
    {
        var entities = await _repository.GetAllAsync();
        var entity = entities.FirstOrDefault(x => x.IsActive);
        return entity != null ? ShippingCostDTO.Create(entity) : null;
    }

    private async Task DeactivateAllOthersAsync(int? exceptId = null)
    {
        var allEntities = await _repository.GetAllAsync();
        var activeEntities = allEntities.Where(x => x.IsActive && x.IdShippingCost != exceptId).ToList();
        
        foreach (var entity in activeEntities)
        {
            entity.IsActive = false;
            await _repository.UpdateAsync(entity);
        }
    }

    public async Task<ShippingCostDTO> CreateAsync(CreateShippingCostRequest request)
    {
        if (request.IsActive)
        {
            await DeactivateAllOthersAsync();
        }

        var entity = new ShippingCost
        {
            Name = request.Name,
            Cost = request.Cost,
            IsActive = request.IsActive
        };

        await _repository.AddAsync(entity);
        return ShippingCostDTO.Create(entity);
    }

    public async Task<ShippingCostDTO> UpdateAsync(int id, UpdateShippingCostRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("ShippingCost not found", "SHIPPINGCOST_NOT_FOUND");

        if (request.IsActive.HasValue && request.IsActive.Value)
        {
            await DeactivateAllOthersAsync(id);
        }

        entity.Name = request.Name ?? entity.Name;
        entity.Cost = request.Cost ?? entity.Cost;
        if (request.IsActive.HasValue)
        {
            entity.IsActive = request.IsActive.Value;
        }

        await _repository.UpdateAsync(entity);
        return ShippingCostDTO.Create(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("ShippingCost not found", "SHIPPINGCOST_NOT_FOUND");
        await _repository.DeleteAsync(entity);
    }
}
