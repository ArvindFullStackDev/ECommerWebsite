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
public class CouponsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CouponsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index(string? search)
    {
        IQueryable<Coupon> query = _unitOfWork.Repository<Coupon>().GetQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Code.Contains(search));

        ViewBag.Search = search;
        return View(await query.OrderByDescending(c => c.Id).ToListAsync());
    }

    public IActionResult Create()
    {
        ViewBag.DiscountTypes = DiscountTypesSelect();
        return View(new Coupon { IsActive = true, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddMonths(1) });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Coupon model)
    {
        ViewBag.DiscountTypes = DiscountTypesSelect(model.DiscountType);
        ValidateCoupon(model);
        if (!ModelState.IsValid) return View(model);

        if (await _unitOfWork.Repository<Coupon>().GetQueryable().AnyAsync(c => c.Code == model.Code))
        {
            ModelState.AddModelError("Code", "A coupon with this code already exists");
            return View(model);
        }

        model.Code = model.Code.Trim().ToUpperInvariant();
        _unitOfWork.Repository<Coupon>().AddAsync(model);
        await _unitOfWork.CompleteAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var coupon = await _unitOfWork.Repository<Coupon>().GetByIdAsync(id);
        if (coupon == null) return NotFound();

        ViewBag.DiscountTypes = DiscountTypesSelect(coupon.DiscountType);
        return View(coupon);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Coupon model)
    {
        ViewBag.DiscountTypes = DiscountTypesSelect(model.DiscountType);
        ValidateCoupon(model);
        if (!ModelState.IsValid) return View(model);

        var coupon = await _unitOfWork.Repository<Coupon>().GetByIdAsync(model.Id);
        if (coupon == null) return NotFound();

        coupon.Code = model.Code.Trim().ToUpperInvariant();
        coupon.Description = model.Description;
        coupon.DiscountType = model.DiscountType;
        coupon.DiscountValue = model.DiscountValue;
        coupon.MinimumOrderAmount = model.MinimumOrderAmount;
        coupon.MaximumDiscount = model.MaximumDiscount;
        coupon.UsageLimit = model.UsageLimit;
        coupon.IsActive = model.IsActive;
        coupon.ValidFrom = model.ValidFrom;
        coupon.ValidTo = model.ValidTo;
        _unitOfWork.Repository<Coupon>().Update(coupon);
        await _unitOfWork.CompleteAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var coupon = await _unitOfWork.Repository<Coupon>().GetByIdAsync(id);
        if (coupon != null)
        {
            coupon.IsActive = !coupon.IsActive;
            _unitOfWork.Repository<Coupon>().Update(coupon);
            await _unitOfWork.CompleteAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var coupon = await _unitOfWork.Repository<Coupon>().GetByIdAsync(id);
        if (coupon != null)
        {
            _unitOfWork.Repository<Coupon>().Delete(coupon);
            await _unitOfWork.CompleteAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private void ValidateCoupon(Coupon model)
    {
        if (string.IsNullOrWhiteSpace(model.Code))
            ModelState.AddModelError("Code", "Code is required");
        if (model.DiscountValue <= 0)
            ModelState.AddModelError("DiscountValue", "Discount value must be greater than zero");
        if (model.ValidTo <= model.ValidFrom)
            ModelState.AddModelError("ValidTo", "Valid To must be after Valid From");
    }

    private static SelectList DiscountTypesSelect(DiscountType? selected = null)
    {
        return new SelectList(
            Enum.GetValues<DiscountType>().Select(e => new { Value = (int)e, Text = e.ToString() }),
            "Value", "Text", selected.HasValue ? (int)selected.Value : null);
    }
}
