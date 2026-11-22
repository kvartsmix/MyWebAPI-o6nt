using AutoMapper;
using MyWebAPI.Documents;
using MyWebAPI.Dtos;
using MyWebAPI.Models;

namespace MyWebAPI.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<CreateUserDto, User>();
        CreateMap<CreateUserDto, UserDocument>();

        CreateMap<User, UserDocument>().ReverseMap();
    }
}