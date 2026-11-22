using AutoMapper;
using MyWebAPI.Documents;
using MyWebAPI.Dtos;
using MyWebAPI.Models;

namespace MyWebAPI.Mappings;

public class StorageBoxProfile : Profile
{
	public StorageBoxProfile()
	{
		CreateMap<StorageBox, StorageBoxResponseDto>();
		CreateMap<StorageBoxDocument, StorageBoxResponseDto>();

		CreateMap<StorageBoxCreateDto, StorageBoxDocument>();
		CreateMap<StorageBoxCreateDto, StorageBox>();
		CreateMap<StorageBoxPatchDto, StorageBoxDocument>()
			.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
	}
}