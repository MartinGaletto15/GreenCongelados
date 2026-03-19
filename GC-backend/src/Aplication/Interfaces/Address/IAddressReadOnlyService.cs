using Applications.dtos;

namespace Aplication.Interfaces.Address;

public interface IAddressReadOnlyService
{
    Task<AddressDTO> GetByIdAsync(int id);
    Task<IEnumerable<AddressDTO>> GetAllAsync();
    Task<AddressDTO> GetByUserIdAsync(int userId);
}