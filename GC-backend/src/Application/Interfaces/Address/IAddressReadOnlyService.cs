using Application.DTOs;

namespace Application.Interfaces.Address;

public interface IAddressReadOnlyService
{
    Task<AddressDTO> GetByIdAsync(int id);
    Task<IEnumerable<AddressDTO>> GetAllAsync();
    Task<AddressDTO> GetByUserIdAsync(int userId);
}