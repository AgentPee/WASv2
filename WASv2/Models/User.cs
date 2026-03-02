using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WASv2.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; } // Primary Key matching your database

        [Required, StringLength(50)]
        public string Username { get; set; }

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [Required, StringLength(255)]
        public string PasswordHash { get; set; }

        [StringLength(100)]
        public string? FullName { get; set; }

        public int? RoleID { get; set; }
        public int? DeptID { get; set; }

        public int IsActive { get; set; } = 1;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties to link tables
        [ForeignKey("RoleID")]
        public virtual Role? Role { get; set; }

        [ForeignKey("DeptID")]
        public virtual Department? Department { get; set; }

        // Helper property used in AuthController and ClaimsTransformation
        [NotMapped]
        public string RoleName => Role?.Name ?? "User";
    }
}