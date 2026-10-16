using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pragathi.Admin.Application.Dtos;
using Pragathi.Admin.Application.Exceptions;
using Pragathi.Admin.Application.Services;

namespace Pragathi.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Superuser")]      // only superusers get in here
public class UsersController : ControllerBase
{
    private readonly IAdminUserService _users;
    public UsersController(IAdminUserService users) => _users = users;

    [HttpGet]
    public Task<IActionResult> List() => Run(async () => Ok(await _users.ListAsync()));

    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateUserRequest request) =>
        Run(async () => Ok(await _users.CreateAsync(request)));

    [HttpPut("{id:int}")]
    public Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request) =>
        Run(async () => Ok(await _users.UpdateAsync(id, request)));

    [HttpDelete("{id:int}")]
    public Task<IActionResult> Delete(int id) =>
        Run(async () => { await _users.DeleteAsync(id); return NoContent(); });

    // Turns our own exceptions into proper HTTP answers
    private async Task<IActionResult> Run(Func<Task<IActionResult>> action)
    {
        try { return await action(); }
        catch (ValidationFailedException ex) { return BadRequest(new { message = ex.Message }); }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}