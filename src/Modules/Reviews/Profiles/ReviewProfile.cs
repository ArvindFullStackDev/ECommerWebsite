using AutoMapper;
using Reviews.DTOs;

namespace Reviews.Profiles;

public class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<CreateReviewDto, CreateReviewCommand>();
    }
}
