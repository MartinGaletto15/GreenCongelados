using Aplication.Interfaces.Address;
using Applications.dtos;
using Applications.dtos.Requests;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;

namespace Aplication.Services;

public class AddressService : IAddressService
{
    private readonly IGenericRepository<Address> _repository;

    public AddressService(IGenericRepository<Address> repository)
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

    public async Task<AddressDTO> CreateAsync(CreateAddressRequest request)
    {
        var entity = new Address
        {
            IdUser = request.IdUser,
            User = null!, // EF handles navigation via FK
            Street = request.Street,
            City = request.City,
            ZipCode = request.ZipCode,
            Dpto = request.Dpto,
            References = request.References
        };

        await _repository.AddAsync(entity);
        return AddressDTO.Create(entity);
    }

    public async Task<AddressDTO> UpdateAsync(int id, UpdateAddressRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Address not found", "ADDRESS_NOT_FOUND");

        entity.Street = request.Street ?? entity.Street;
        entity.City = request.City ?? entity.City;
        entity.ZipCode = request.ZipCode ?? entity.ZipCode;
        entity.Dpto = request.Dpto ?? entity.Dpto;
        entity.References = request.References ?? entity.References;

        await _repository.UpdateAsync(entity);
        return AddressDTO.Create(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Address not found", "ADDRESS_NOT_FOUND");
        await _repository.DeleteAsync(entity);
    }
}