namespace MyWebAPI.Dtos;

public class StorageBoxResponseDto
{
	public string Id { get; set; } = string.Empty;
	public double Square { get; set; }
	public double Volume { get; set; }
	public string Type { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty;
	public bool IsDisinfected { get; set; }
	public decimal MonthlyPrice { get; set; }
	public string? CurrentContractId { get; set; }
}