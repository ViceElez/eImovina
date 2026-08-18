namespace eImovina.Shared.DTOs.Equipments;

public class EquipmentFileDto
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int SizeBytes { get; set; }
    public bool IsCoverImage { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
}