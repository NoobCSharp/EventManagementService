namespace EventManagementService.Models
{
    public class Event
    {
        public Guid EvendId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }
    }
}
