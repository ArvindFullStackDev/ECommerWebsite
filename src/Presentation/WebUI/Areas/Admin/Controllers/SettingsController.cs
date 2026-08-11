using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class SettingsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public SettingsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _unitOfWork.Repository<SiteSetting>().GetQueryable()
            .OrderBy(s => s.Group).ThenBy(s => s.Key).ToListAsync();
        ViewBag.Groups = settings.GroupBy(s => s.Group ?? "General");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Save(Dictionary<string, string> values)
    {
        var numericKeys = new[] { "FreeShippingThreshold", "StandardShippingCharge", "TaxRate" };
        foreach (var key in numericKeys)
        {
            if (values.TryGetValue(key, out var v) && !decimal.TryParse(v, out _))
            {
                ModelState.AddModelError(key, $"'{key}' must be a valid number");
            }
        }
        if (ModelState.IsValid)
        {
            var settings = await _unitOfWork.Repository<SiteSetting>().GetQueryable().ToListAsync();
            foreach (var setting in settings)
            {
                if (values.TryGetValue(setting.Key, out var newValue))
                {
                    setting.Value = newValue;
                    _unitOfWork.Repository<SiteSetting>().Update(setting);
                }
            }
            await _unitOfWork.CompleteAsync();
            TempData["SettingsSaved"] = true;
        }
        return RedirectToAction(nameof(Index));
    }
}
