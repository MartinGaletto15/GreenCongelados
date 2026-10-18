using Application.DTOs;
using Application.DTOs.Requests;

namespace Application.Interfaces.Address;

public interface IAddressWriteService
{
    Task<AddressDTO> CreateAsync(CreateAddressRequest request, int idUser);
    Task<AddressDTO> UpdateMyAddressAsync(UpdateAddressRequest request, int idUser);
    Task DeleteMyAddressAsync(int idUser);
}