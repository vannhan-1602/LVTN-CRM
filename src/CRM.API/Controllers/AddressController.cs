using CRM.Application.Features.Addresses.Commands.CreateAddress;
using CRM.Application.Features.Addresses.Commands.DeleteAddress;
using CRM.Application.Features.Addresses.Commands.UpdateAddress;
using CRM.Application.Features.Addresses.Queries.GetAddressesByCustomer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IMediator _mediator;

    public AddressController(IMediator mediator) => _mediator = mediator;

    [HttpGet("customer/{khachHangId}")]
    public async Task<IActionResult> GetByCustomer(ulong khachHangId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAddressesByCustomerQuery(khachHangId), ct);
        return Ok(result);
    }

    [HttpPost("customer/{khachHangId}")]
    public async Task<IActionResult> Create(ulong khachHangId, [FromBody] CreateAddressCommand command, CancellationToken ct)
    {
        if (khachHangId != command.KhachHangId)
        {
            command = command with { KhachHangId = khachHangId };
        }
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateAddressCommand command, CancellationToken ct)
    {
        if (id != command.Id)
        {
            command = command with { Id = id };
        }
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<bool> Delete(ulong id, CancellationToken ct)
    {
        return await _mediator.Send(new DeleteAddressCommand(id), ct);
    }
}