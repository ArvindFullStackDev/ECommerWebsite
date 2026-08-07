using AutoMapper;
using Catalog.DTOs;
using Domain.Entities;

namespace Catalog.Profiles;

public class CatalogProfile : Profile
{
    public CatalogProfile()
    {
        // Category mappings
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ParentCategoryName, opt => opt.MapFrom(s => s.ParentCategory != null ? s.ParentCategory.Name : null))
            .ForMember(d => d.SubCategories, opt => opt.MapFrom(s => s.SubCategories))
            .ForMember(d => d.ProductCount, opt => opt.Ignore());

        CreateMap<CategoryDto, Category>()
            .ForMember(d => d.SubCategories, opt => opt.Ignore())
            .ForMember(d => d.ParentCategory, opt => opt.Ignore())
            .ForMember(d => d.Products, opt => opt.Ignore());

        // Brand mappings
        CreateMap<Brand, BrandDto>()
            .ForMember(d => d.ProductCount, opt => opt.Ignore());

        CreateMap<BrandDto, Brand>()
            .ForMember(d => d.Products, opt => opt.Ignore());

        // Product mappings
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name))
            .ForMember(d => d.BrandName, opt => opt.MapFrom(s => s.Brand != null ? s.Brand.Name : null))
            .ForMember(d => d.PrimaryImageUrl, opt => opt.MapFrom(s => s.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault() ?? s.Images.Select(i => i.ImageUrl).FirstOrDefault()))
            .ForMember(d => d.ImageUrls, opt => opt.MapFrom(s => s.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList()))
            .ForMember(d => d.HasFlashSale, opt => opt.MapFrom(s => s.FlashSaleStart <= DateTime.UtcNow && s.FlashSaleEnd >= DateTime.UtcNow && s.FlashSalePrice.HasValue))
            .ForMember(d => d.DiscountType, opt => opt.MapFrom(s => s.DiscountType.HasValue ? s.DiscountType.Value.ToString() : null));
    }
}
