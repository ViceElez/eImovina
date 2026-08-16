namespace eImovina.Shared.Models
{
    public static class AssignmentStatusNames
    {
        public const string Active = "Aktivno";
        public const string Returned = "Vraćeno";
        public const string Transferred = "Premješteno";
        public const string Cancelled = "Stornirano";
    }

    public static class InventoryStatusNames
    {
        public const string Draft = "Nacrt";
        public const string Open = "Otvorena";
        public const string InProgress = "U tijeku";
        public const string Completed = "Završena";
        public const string Locked = "Zaključana";
    }

    public static class EquipmentStatusNames
    {
        public const string InStock = "Na skladištu";
        public const string Assigned = "Zaduženo";
        public const string InService = "Na servisu";
        public const string Missing = "Nedostaje";
        public const string WrittenOff = "Otpisano";
    }

    public enum EquipmentFileType
    {
        Image = 0,
        Invoice = 1,
        Warranty = 2,
        ServiceDoc = 3
    }
}