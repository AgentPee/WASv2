using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Department
{
        public int Id { get; set; } // Unique identifier for the department
        public string Name { get; set; } = string.Empty; // Department name
}
