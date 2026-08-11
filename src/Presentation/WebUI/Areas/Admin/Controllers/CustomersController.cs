using Admin.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class CustomersController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public CustomersController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        var model = new List<UserManagementDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            model.Add(new UserManagementDto
            {
                Id = user.Id,
                Email = user.Email,
                IsLocked = await _userManager.IsLockedOutAsync(user),
                LockoutEnd = user.LockoutEnd,
                Roles = roles.ToList()
            });
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (await _userManager.IsLockedOutAsync(user))
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
        }
        else
        {
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        }

        return RedirectToAction(nameof(Index));
    }
}
