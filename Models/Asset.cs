namespace Myportal.Models
{
    public class Asset
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Unspecified";
        public string SerialNumber { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
        public decimal PurchaseCost { get; set; }
        public AssetStatus Status { get; set; } = AssetStatus.Available;
        
        // Navigation properties
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<MaintenanceLog> MaintenanceLogs { get; set; } = new List<MaintenanceLog>();

        public enum AssetStatus
        {
            Available,
            Assigned,
            InMaintenance,
            Retired,
            Lost
        }
    }
}