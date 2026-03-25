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
    private readonly IUnitOfWork _unitOfWork;

    public PromotionService(IGenericRepository<Promotion> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
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
        var entity = new Promotion(
            request.CouponCode,
            request.DiscountValue,
            request.DiscountType,
            request.MinAmount,
            request.StartDate,
            request.EndDate
        );

        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return PromotionDTO.Create(entity);
    }

    public async Task<PromotionDTO> UpdateAsync(int id, UpdatePromotionRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Promotion not found", "PROMOTION_NOT_FOUND");

        entity.UpdateDetails(
            request.CouponCode,
            request.DiscountValue,
            request.DiscountType,
            request.MinAmount,
            request.StartDate,
            request.EndDate
        );

        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return PromotionDTO.Create(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Promotion not found", "PROMOTION_NOT_FOUND");
        await _repository.DeleteAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
