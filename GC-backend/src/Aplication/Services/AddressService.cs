using Aplication.Interfaces.Address;
using Applications.dtos;
using Applications.dtos.Requests;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;

namespace Aplication.Services;

public class AddressService : IAddressReadOnlyService, IAddressWriteService
{
    private readonly IAddressRepository _repository;

    public AddressService(IAddressRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<AddressDTO>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return AddressDTO.Create(entities);
    }

    public async Task<AddressDTO> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Address not found", "ADDRESS_NOT_FOUND");
        return AddressDTO.Create(entity);
    }

    public async Task<AddressDTO> GetByUserIdAsync(int userId)
    {
        var entity = await _repository.GetByUserIdAsync(userId);
        if (entity == null) throw new AppValidationException("Address not found", "ADDRESS_NOT_FOUND");
        return AddressDTO.Create(entity);
    }

    public async Task<AddressDTO> CreateAsync(CreateAddressRequest request, int idUser)
    {
        var entity = new Address
        {
            IdUser = idUser,
            Street = request.Street,
            Dpto = request.Dpto,
            References = request.References
        };

        await _repository.AddAsync(entity);
        return AddressDTO.Create(entity);
    }

    public async Task<AddressDTO> UpdateMyAddressAsync(UpdateAddressRequest request, int idUser)
    {
        var entity = await _repository.GetByUserIdAsync(idUser);
        if (entity == null) throw new AppValidationException("Address not found", "ADDRESS_NOT_FOUND");

        entity.Street = request.Street ?? entity.Street;
        entity.Dpto = request.Dpto ?? entity.Dpto;
        entity.References = request.References ?? entity.References;

        await _repository.UpdateAsync(entity);
        return AddressDTO.Create(entity);
    }

    public async Task DeleteMyAddressAsync(int idUser)
    {
        var entity = await _repository.GetByUserIdAsync(idUser);
        if (entity == null) throw new AppValidationException("Address not found", "ADDRESS_NOT_FOUND");
        await _repository.DeleteAsync(entity);
    }
}