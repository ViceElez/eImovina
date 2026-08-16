namespace eImovina.Shared.Models.WriteOffRequests
{
    public class WriteOffRequestStatus
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<WriteOffRequest> WriteOffRequests { get; set; } = new List<WriteOffRequest>();
    }
}
