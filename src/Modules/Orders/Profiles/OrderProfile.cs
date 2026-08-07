using AutoMapper;
using Domain.Entities;
using Orders.DTOs;

namespace Orders.Profiles;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderListDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.Items.Count));
    }
}
