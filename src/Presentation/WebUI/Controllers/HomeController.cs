using System.Diagnostics;
using AutoMapper;
using Catalog.DTOs;
using Catalog.Queries.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

public class HomeController : Controller
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IMediator mediator, IMapper mapper, ILogger<HomeController> logger)
    {
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var featuredQuery = new GetAllProductsQuery { IsFeatured = true, PageSize = 8 };
        var bestSellerQuery = new GetAllProductsQuery { IsBestSeller = true, PageSize = 8 };
        var trendingQuery = new GetAllProductsQuery { IsTrending = true, PageSize = 8 };

        var featuredResult = await _mediator.Send(featuredQuery);
        var bestSellerResult = await _mediator.Send(bestSellerQuery);
        var trendingResult = await _mediator.Send(trendingQuery);

        var viewModel = new HomeViewModel
        {
            FeaturedProducts = (featuredResult.Data?.Items ?? new List<ProductDto>()).ToList(),
            BestSellers = (bestSellerResult.Data?.Items ?? new List<ProductDto>()).ToList(),
            TrendingProducts = (trendingResult.Data?.Items ?? new List<ProductDto>()).ToList()
        };

        return View(viewModel);
    }

    public IActionResult About() => View();
    public IActionResult Contact() => View();
    public IActionResult FAQ() => View();
    public IActionResult Privacy() => View();
    public IActionResult Terms() => View();
    public IActionResult ReturnPolicy() => View();
    public IActionResult ShippingPolicy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult NotFoundPage() => View();
    public IActionResult Maintenance() => View();
}

public class HomeViewModel
{
    public List<ProductDto> FeaturedProducts { get; set; } = new();
    public List<ProductDto> BestSellers { get; set; } = new();
    public List<ProductDto> TrendingProducts { get; set; } = new();
}

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
