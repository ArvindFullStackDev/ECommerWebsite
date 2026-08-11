using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class BrandsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public BrandsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var brands = await _unitOfWork.Repository<Brand>().GetQueryable()
            .Include(b => b.Products)
            .OrderBy(b => b.Name)
            .ToListAsync();

        return View(brands);
    }

    public IActionResult Create()
    {
        return View(new Brand { IsActive = true });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Brand model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError("Name", "Name is required");
        }

        if (ModelState.IsValid)
        {
            model.Slug = model.Name.Trim().ToLowerInvariant().Replace(" ", "-").Replace("&", "and");
            var existing = await _unitOfWork.Repository<Brand>()
                .GetQueryable().AnyAsync(b => b.Slug == model.Slug);
            if (existing)
            {
                model.Slug = model.Slug + "-" + Guid.NewGuid().ToString("N")[..6];
            }
            await _unitOfWork.Repository<Brand>().AddAsync(model);
            await _unitOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var brand = await _unitOfWork.Repository<Brand>().GetByIdAsync(id);
        if (brand == null) return NotFound();

        return View(brand);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Brand model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError("Name", "Name is required");
        }

        if (ModelState.IsValid)
        {
            var brand = await _unitOfWork.Repository<Brand>().GetByIdAsync(model.Id);
            if (brand == null) return NotFound();

            brand.Name = model.Name;
            brand.Description = model.Description;
            brand.IsActive = model.IsActive;
            brand.DisplayOrder = model.DisplayOrder;
            _unitOfWork.Repository<Brand>().Update(brand);
            await _unitOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var brand = await _unitOfWork.Repository<Brand>().GetByIdAsync(id);
        if (brand != null)
        {
            _unitOfWork.Repository<Brand>().Delete(brand);
            await _unitOfWork.CompleteAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
