using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class CategoriesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoriesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _unitOfWork.Repository<Category>().GetQueryable()
            .Include(c => c.ParentCategory)
            .Include(c => c.Products)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        ViewBag.Parents = categories.Where(c => c.ParentCategoryId == null).ToList();
        return View(categories);
    }

    public async Task<IActionResult> Create()
    {
        var parents = await _unitOfWork.Repository<Category>()
            .GetQueryable().Where(c => c.ParentCategoryId == null).OrderBy(c => c.Name).ToListAsync();
        ViewBag.Parents = parents;
        return View(new Category { IsActive = true });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Category model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError("Name", "Name is required");
        }

        if (ModelState.IsValid)
        {
            model.Slug = Slugify(model.Name);
            var existing = await _unitOfWork.Repository<Category>()
                .GetQueryable().AnyAsync(c => c.Slug == model.Slug);
            if (existing)
            {
                model.Slug = model.Slug + "-" + Guid.NewGuid().ToString("N")[..6];
            }
            await _unitOfWork.Repository<Category>().AddAsync(model);
            await _unitOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Parents = await _unitOfWork.Repository<Category>()
            .GetQueryable().Where(c => c.ParentCategoryId == null).OrderBy(c => c.Name).ToListAsync();
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
        if (category == null) return NotFound();

        ViewBag.Parents = await _unitOfWork.Repository<Category>()
            .GetQueryable().Where(c => c.ParentCategoryId == null && c.Id != id).OrderBy(c => c.Name).ToListAsync();
        return View(category);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Category model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError("Name", "Name is required");
        }

        if (ModelState.IsValid)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(model.Id);
            if (category == null) return NotFound();

            category.Name = model.Name;
            category.Description = model.Description;
            category.ParentCategoryId = model.ParentCategoryId;
            category.IsActive = model.IsActive;
            category.DisplayOrder = model.DisplayOrder;
            _unitOfWork.Repository<Category>().Update(category);
            await _unitOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Parents = await _unitOfWork.Repository<Category>()
            .GetQueryable().Where(c => c.ParentCategoryId == null && c.Id != model.Id).OrderBy(c => c.Name).ToListAsync();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
        if (category != null)
        {
            _unitOfWork.Repository<Category>().Delete(category);
            await _unitOfWork.CompleteAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private static string Slugify(string name) =>
        name.Trim().ToLowerInvariant().Replace(" ", "-").Replace("&", "and")
            .Replace("+", "-").Replace("'", "").Replace("\"", "").Replace("/", "-");
}
