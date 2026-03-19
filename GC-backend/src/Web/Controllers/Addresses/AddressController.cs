using Aplication.Interfaces.Address;
using Applications.dtos;
using Applications.dtos.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Addresses;

[ApiController]
[Route("api/addresses")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AddressDTO>>> GetAllAsync()
    {
        var result = await _addressService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AddressDTO>> GetByIdAsync(int id)
    {
        var result = await _addressService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AddressDTO>> CreateAsync(CreateAddressRequest request)
    {
        var result = await _addressService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AddressDTO>> UpdateAsync(int id, UpdateAddressRequest request)
    {
        var result = await _addressService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        await _addressService.DeleteAsync(id);
        return NoContent();
    }
}