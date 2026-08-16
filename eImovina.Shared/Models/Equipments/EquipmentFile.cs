using eImovina.Shared.Models.Users;

namespace eImovina.Shared.Models.Equipments
{
    public class EquipmentFile
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; } = null!;
        public EquipmentFileType FileType { get; set; }
        public string OriginalFileName { get; set; } = null!;
        public string StoredFileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public int SizeBytes { get; set; }
        public bool IsCoverImage { get; set; }
        public DateTime UploadedAt { get; set; }
        public int UploadedByUserId { get; set; }
        public User UploadedByUser { get; set; } = null!;
    }
}
