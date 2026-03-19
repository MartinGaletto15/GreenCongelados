using Applications.dtos;
using Applications.dtos.Requests;

namespace Aplication.Interfaces.Address;

public interface IAddressWriteService
{
    Task<AddressDTO> CreateAsync(CreateAddressRequest request, int idUser);
    Task<AddressDTO> UpdateMyAddressAsync(UpdateAddressRequest request, int idUser);
    Task DeleteMyAddressAsync(int idUser);
}