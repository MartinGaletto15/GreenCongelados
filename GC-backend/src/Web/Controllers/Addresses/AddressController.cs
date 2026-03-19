using Aplication.Interfaces.Address;
using Applications.dtos;
using Applications.dtos.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers.Addresses;

[ApiController]
[Route("api/addresses")]
public class AddressController : ControllerBase
{
    private readonly IAddressReadOnlyService _readOnlyService;
    private readonly IAddressWriteService _writeService;

    public AddressController(IAddressReadOnlyService readOnlyService, IAddressWriteService writeService)
    {
        _readOnlyService = readOnlyService;
        _writeService = writeService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<IEnumerable<AddressDTO>>> GetAllAsync()
    {
        var result = await _readOnlyService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<AddressDTO>> GetByIdAsync(int id)
    {
        var result = await _readOnlyService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AddressDTO>> GetMyAddressAsync()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var result = await _readOnlyService.GetByUserIdAsync(userId);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<AddressDTO>> CreateAsync(CreateAddressRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var result = await _writeService.CreateAsync(request, userId);
        return Ok(result);
    }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult<AddressDTO>> UpdateMyAddressAsync(UpdateAddressRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var result = await _writeService.UpdateMyAddressAsync(request, userId);
        return Ok(result);
    }

    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> DeleteMyAddressAsync()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        await _writeService.DeleteMyAddressAsync(userId);
        return NoContent();
    }
}