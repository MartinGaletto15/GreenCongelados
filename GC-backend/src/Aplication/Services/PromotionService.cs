using Aplication.Interfaces.Promotion;
using Applications.dtos;
using Applications.dtos.Requests;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;

namespace Aplication.Services;

public class PromotionService : IPromotionReadOnlyService, IPromotionWriteService
{
    private readonly IGenericRepository<Promotion> _repository;

    public PromotionService(IGenericRepository<Promotion> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PromotionDTO>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return PromotionDTO.Create(entities);
    }

    public async Task<PromotionDTO> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Promotion not found", "PROMOTION_NOT_FOUND");
        return PromotionDTO.Create(entity);
    }

    public async Task<PromotionDTO?> GetByCodeAsync(string couponCode)
    {
        var entities = await _repository.GetAllAsync();
        var entity = entities.FirstOrDefault(x => x.CouponCode == couponCode);
        return entity != null ? PromotionDTO.Create(entity) : null;
    }

    public async Task<PromotionDTO> CreateAsync(CreatePromotionRequest request)
    {
        var entity = new Promotion
        {
            CouponCode = request.CouponCode,
            DiscountValue = request.DiscountValue,
            DiscountType = request.DiscountType,
            MinAmount = request.MinAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        await _repository.AddAsync(entity);
        return PromotionDTO.Create(entity);
    }

    public async Task<PromotionDTO> UpdateAsync(int id, UpdatePromotionRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Promotion not found", "PROMOTION_NOT_FOUND");

        entity.CouponCode = request.CouponCode ?? entity.CouponCode;
        entity.DiscountValue = request.DiscountValue ?? entity.DiscountValue;
        entity.DiscountType = request.DiscountType ?? entity.DiscountType;
        entity.MinAmount = request.MinAmount ?? entity.MinAmount;
        entity.StartDate = request.StartDate ?? entity.StartDate;
        entity.EndDate = request.EndDate ?? entity.EndDate;

        await _repository.UpdateAsync(entity);
        return PromotionDTO.Create(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Promotion not found", "PROMOTION_NOT_FOUND");
        await _repository.DeleteAsync(entity);
    }
}
