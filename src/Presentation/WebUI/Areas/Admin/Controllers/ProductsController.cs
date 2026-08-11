using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ProductsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        const int pageSize = 20;
        IQueryable<Product> query = _unitOfWork.Repository<Product>().GetQueryable()
            .Include(p => p.Category)
            .Include(p => p.Brand);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.Name.Contains(search) ||
                (p.SKU != null && p.SKU.Contains(search)));

        var total = await query.CountAsync();
        var products = await query
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Total = total;
        ViewBag.Page = page;
        ViewBag.Pages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));

        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View(new Product { IsActive = true, StockQuantity = 0 });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product model)
    {
        ModelState.Remove(nameof(Product.Category));
        ModelState.Remove(nameof(Product.Brand));
        ValidateProduct(model);
        if (ModelState.IsValid)
        {
            model.Slug = Slugify(model.Name);
            var slugExists = await _unitOfWork.Repository<Product>()
                .GetQueryable().AnyAsync(p => p.Slug == model.Slug);
            if (slugExists)
            {
                model.Slug = model.Slug + "-" + Guid.NewGuid().ToString("N")[..6];
            }

            _unitOfWork.Repository<Product>().AddAsync(model);
            await _unitOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync(model.CategoryId, model.BrandId);
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
        if (product == null) return NotFound();

        await PopulateLookupsAsync(product.CategoryId, product.BrandId);
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Product model)
    {
        ModelState.Remove(nameof(Product.Category));
        ModelState.Remove(nameof(Product.Brand));
        ValidateProduct(model);
        if (ModelState.IsValid)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(model.Id);
            if (product == null) return NotFound();

            product.Name = model.Name;
            product.ShortDescription = model.ShortDescription;
            product.Description = model.Description;
            product.Price = model.Price;
            product.DiscountPrice = model.DiscountPrice;
            product.DiscountType = model.DiscountType;
            product.DiscountValue = model.DiscountValue;
            product.SKU = model.SKU;
            product.Barcode = model.Barcode;
            product.StockQuantity = model.StockQuantity;
            product.LowStockThreshold = model.LowStockThreshold;
            product.IsActive = model.IsActive;
            product.IsFeatured = model.IsFeatured;
            product.IsTrending = model.IsTrending;
            product.IsBestSeller = model.IsBestSeller;
            product.CategoryId = model.CategoryId;
            product.BrandId = model.BrandId;
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync(model.CategoryId, model.BrandId);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
        if (product != null)
        {
            product.IsActive = !product.IsActive;
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
        if (product != null)
        {
            _unitOfWork.Repository<Product>().Delete(product);
            await _unitOfWork.CompleteAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(int? categoryId = null, int? brandId = null)
    {
        var categories = await _unitOfWork.Repository<Category>().GetQueryable()
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name).ToListAsync();
        var brands = await _unitOfWork.Repository<Brand>().GetQueryable()
            .OrderBy(b => b.Name).ToListAsync();

        var categoryItems = new List<SelectListItem>();
        foreach (var c in categories.Where(c => c.ParentCategoryId == null))
        {
            categoryItems.Add(new SelectListItem(c.Name, c.Id.ToString(), categoryId == c.Id));
            foreach (var sub in categories.Where(s => s.ParentCategoryId == c.Id))
            {
                categoryItems.Add(new SelectListItem("-- " + sub.Name, sub.Id.ToString(), categoryId == sub.Id));
            }
        }

        ViewBag.CategoryList = new SelectList(categoryItems, "Value", "Text");
        ViewBag.BrandList = new SelectList(brands, "Id", "Name", brandId);
        ViewBag.DiscountTypes = new SelectList(
            Enum.GetValues<DiscountType>().Select(e => new { Value = (int)e, Text = e.ToString() }),
            "Value", "Text");
    }

    private void ValidateProduct(Product model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError("Name", "Name is required");
        if (model.Price <= 0)
            ModelState.AddModelError("Price", "Price must be greater than zero");
        if (model.StockQuantity < 0)
            ModelState.AddModelError("StockQuantity", "Stock cannot be negative");
        if (model.DiscountPrice.HasValue && model.DiscountPrice.Value >= model.Price)
            ModelState.AddModelError("DiscountPrice", "Discount price must be lower than the price");
        if (model.DiscountPrice.HasValue && model.DiscountType == null)
            ModelState.AddModelError("DiscountType", "Select a discount type when a discount price is set");
    }

    private static string Slugify(string name) =>
        name.Trim().ToLowerInvariant().Replace(" ", "-").Replace("&", "and")
            .Replace("+", "-").Replace("'", "").Replace("\"", "").Replace("/", "-");
}
