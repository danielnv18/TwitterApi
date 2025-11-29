using AutoMapper;

namespace TwitterCloneApi.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<Domain.Entities.User, Features.Users.Common.UserDto>();
    }
}
