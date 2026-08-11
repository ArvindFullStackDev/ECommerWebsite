using Catalog.DTOs;
using Catalog.Queries;
using Catalog.Queries.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

public class CatalogController : Controller
{
    private readonly IMediator _mediator;

    public CatalogController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(int? categoryId, string? slug, int? brandId, string? sortBy, int page = 1)
    {
        var categoriesResult = await _mediator.Send(new GetAllCategoriesQuery { IsActive = true });
        var categories = categoriesResult.Data ?? new List<CategoryDto>();

        var query = new GetAllProductsQuery
        {
            CategoryId = categoryId,
            BrandId = brandId,
            SortBy = sortBy,
            Page = page,
            PageSize = 12
        };

        if (categoryId == null && !string.IsNullOrWhiteSpace(slug))
        {
            var match = categories.FirstOrDefault(c => string.Equals(c.Slug, slug, StringComparison.OrdinalIgnoreCase))
                ?? categories.SelectMany(c => c.SubCategories)
                    .FirstOrDefault(c => string.Equals(c.Slug, slug, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                query.CategoryId = match.Id;
                if (match.SubCategories.Count > 0)
                {
                    query.CategoryIds = new List<int> { match.Id };
                    query.CategoryIds.AddRange(match.SubCategories.Select(s => s.Id));
                }
            }
        }

        var brandsResult = await _mediator.Send(new GetAllBrandsQuery { IsActive = true });
        var productsResult = await _mediator.Send(query);

        ViewBag.Categories = categories;
        ViewBag.Brands = brandsResult.Data ?? new List<BrandDto>();
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.SelectedBrandId = brandId;
        ViewBag.SelectedSortBy = sortBy;

        return View(productsResult.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Search(ProductSearchDto searchDto)
    {
        var query = new SearchProductsQuery
        {
            SearchTerm = searchDto.SearchTerm,
            CategoryId = searchDto.CategoryId,
            BrandId = searchDto.BrandId,
            MinPrice = searchDto.MinPrice,
            MaxPrice = searchDto.MaxPrice,
            MinRating = searchDto.MinRating,
            SortBy = searchDto.SortBy,
            Page = searchDto.Page,
            PageSize = searchDto.PageSize,
            InStock = searchDto.InStock
        };

        var categoriesResult = await _mediator.Send(new GetAllCategoriesQuery { IsActive = true });
        var brandsResult = await _mediator.Send(new GetAllBrandsQuery { IsActive = true });
        var productsResult = await _mediator.Send(query);

        ViewBag.Categories = categoriesResult.Data ?? new List<CategoryDto>();
        ViewBag.Brands = brandsResult.Data ?? new List<BrandDto>();
        ViewBag.SearchTerm = searchDto.SearchTerm;

        return View("Index", productsResult.Data);
    }

    public async Task<IActionResult> Details(int id)
    {
        var query = new GetProductByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result.Data == null) return RedirectToAction("NotFoundPage", "Home");

        return View(result.Data);
    }

    public async Task<IActionResult> Deals()
    {
        var query = new GetAllProductsQuery { PageSize = 20 };
        var result = await _mediator.Send(query);
        return View(result.Data?.Items ?? new List<ProductDto>());
    }

    public async Task<IActionResult> BestSellers()
    {
        var query = new GetAllProductsQuery { IsBestSeller = true, PageSize = 20 };
        var result = await _mediator.Send(query);
        return View(result.Data?.Items ?? new List<ProductDto>());
    }

    [HttpGet]
    public async Task<IActionResult> Suggestions(string term)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            return Json(new List<object>());

        var query = new SearchProductsQuery
        {
            SearchTerm = term,
            PageSize = 5
        };

        var result = await _mediator.Send(query);
        var suggestions = result.Data?.Items.Select(p => new
        {
            p.Id,
            p.Name,
            p.PrimaryImageUrl,
            p.DisplayPrice,
            p.Slug
        }).ToList<object>();

        return Json(suggestions ?? new List<object>());
    }
}
