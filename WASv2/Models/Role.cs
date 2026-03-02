using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Role
{
        public int Id { get; set; } // Unique identifier for the role
        public string Name { get; set; } = string.Empty; // Role name (e.g., "Purchasing")
        public string? Description { get; set; } // Optional details about role permissions
}
