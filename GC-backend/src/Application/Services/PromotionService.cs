using Application.Interfaces.Promotion;
using Application.DTOs;
using Application.DTOs.Requests;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;

namespace Application.Services;

public class PromotionService : IPromotionReadOnlyService, IPromotionWriteService
{
    private readonly IPromotionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public PromotionService(IPromotionRepository repository, IUnitOfWork unitOfWork)
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
        if (entity == null) throw new AppValidationException("Promoción no encontrada.", "PROMOTION_NOT_FOUND", 404);
        return PromotionDTO.Create(entity);
    }

    public async Task<PromotionDTO?> GetByCodeAsync(string couponCode)
    {
        var entity = await _repository.GetByCodeAsync(couponCode);
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
        if (entity == null) throw new AppValidationException("Promoción no encontrada.", "PROMOTION_NOT_FOUND", 404);

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
        if (entity == null) throw new AppValidationException("Promoción no encontrada.", "PROMOTION_NOT_FOUND", 404);
        await _repository.DeleteAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task EnsureCouponCodeIsUniqueAsync(string couponCode, int? excludedPromotionId = null)
    {
        if (await _repository.ExistsByCodeAsync(couponCode, excludedPromotionId))
        {
            throw new AppValidationException("Ya existe una promoción con ese código.", "PROMOTION_CODE_EXISTS", 409);
        }
    }
}
