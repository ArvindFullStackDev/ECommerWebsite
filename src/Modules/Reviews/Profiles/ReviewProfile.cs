using AutoMapper;
using Reviews.Commands.CreateReview;
using Reviews.DTOs;

namespace Reviews.Profiles;

public class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<CreateReviewDto, CreateReviewCommand>();
    }
}
