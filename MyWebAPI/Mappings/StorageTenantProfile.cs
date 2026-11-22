using AutoMapper;
using MyWebAPI.Documents;
using MyWebAPI.Dtos;
using MyWebAPI.Models;

namespace MyWebAPI.Mappings;

public class StorageTenantProfile : Profile
{
    public StorageTenantProfile()
    {
        CreateMap<StorageTenant, StorageTenantResponseDto>();
        CreateMap<StorageTenantDocument, StorageTenantResponseDto>();

        CreateMap<StorageTenantCreateDto, StorageTenantDocument>();
        CreateMap<StorageTenantCreateDto, StorageTenant>();
        CreateMap<StorageTenantPatchDto, StorageTenantDocument>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}