namespace Myportal.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ReturnDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Active;
        
        // Navigation properties
        public Asset Asset { get; set; } = null!;
        public Employee Employee { get; set; } = null!;

        public enum AssignmentStatus
        {
            Active,
            Returned,
            Lost,
            Damaged
        }
    }
}