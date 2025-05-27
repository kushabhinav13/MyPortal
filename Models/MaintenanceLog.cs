namespace Myportal.Models
{
    public class MaintenanceLog
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public DateTime MaintenanceDate { get; set; } = DateTime.UtcNow;
        public string Description { get; set; } = string.Empty;
        public string Technician { get; set; } = "Unassigned";
        public decimal Cost { get; set; }
        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Pending;
        
        // Navigation property
        public Asset Asset { get; set; } = null!;

        // Enum for status values
        public enum MaintenanceStatus
        {
            Pending,
            InProgress,
            Completed,
            Cancelled
        }
    }
}