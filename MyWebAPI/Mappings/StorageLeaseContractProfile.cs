using AutoMapper;
using MyWebAPI.Documents;
using MyWebAPI.Dtos;
using MyWebAPI.Models;

namespace MyWebAPI.Mappings;

public class StorageLeaseContractProfile : Profile
{
    public StorageLeaseContractProfile()
    {
        CreateMap<StorageLeaseContract, StorageLeaseContractResponseDto>();
        CreateMap<StorageLeaseContractDocument, StorageLeaseContractResponseDto>();

        CreateMap<StorageLeaseContractCreateDto, StorageLeaseContractDocument>();
        CreateMap<StorageLeaseContractCreateDto, StorageLeaseContract>();
        CreateMap<StorageLeaseContractPatchDto, StorageLeaseContractDocument>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}