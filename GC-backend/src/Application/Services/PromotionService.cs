using Application.Interfaces.Promotion;
using Application.DTOs;
using Application.DTOs.Requests;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;

namespace Application.Services;

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
        var normalizedCode = Promotion.NormalizeCouponCode(couponCode);
        var entity = entities.FirstOrDefault(x => x.CouponCode == normalizedCode);
        return entity != null ? PromotionDTO.Create(entity) : null;
    }

    public async Task<PromotionDTO> CreateAsync(CreatePromotionRequest request)
    {
        await EnsureCouponCodeIsUniqueAsync(request.CouponCode);

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

        if (request.CouponCode != null)
            await EnsureCouponCodeIsUniqueAsync(request.CouponCode, id);

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

    private async Task EnsureCouponCodeIsUniqueAsync(string couponCode, int? excludedPromotionId = null)
    {
        var normalizedCode = Promotion.NormalizeCouponCode(couponCode);
        var promotions = await _repository.GetAllAsync();
        if (promotions.Any(promotion =>
                promotion.IdPromotion != excludedPromotionId &&
                promotion.CouponCode == normalizedCode))
        {
            throw new AppValidationException("Ya existe una promoción con ese código.", "PROMOTION_CODE_EXISTS");
        }
    }
}
