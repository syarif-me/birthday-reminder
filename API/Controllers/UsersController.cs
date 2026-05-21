using BirthdayReminder.Application.DTOs;
using BirthdayReminder.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BirthdayReminder.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await userService.CreateUserAsync(request, cancellationToken);
        return CreatedAtAction(nameof(CreateUser), new { id = user.Id }, user);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        await userService.DeleteUserAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        await userService.UpdateUserAsync(id, request, cancellationToken);
        return NoContent();
    }
}