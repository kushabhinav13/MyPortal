using System.ComponentModel.DataAnnotations;
namespace Myportal.Models
{
    public class Employee
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Department { get; set; } = "Unassigned";

        public DateTime HireDate { get; set; } = DateTime.UtcNow;
        public DateTime? TerminationDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        // Computed property
        public string FullName => $"{FirstName} {LastName}";
    }
}