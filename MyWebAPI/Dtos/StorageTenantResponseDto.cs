namespace MyWebAPI.Dtos;

public class StorageTenantResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // AutoMapper кине enum у текст: "Individual" або "SmallBusiness"
    public bool HasDebt { get; set; }
    public List<string> ContractsId { get; set; } = new();
}