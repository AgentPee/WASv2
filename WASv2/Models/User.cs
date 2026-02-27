namespace WASv2.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // short role key
        public string RoleName { get; set; } = string.Empty; // display name
        public int DeptId { get; set; }
    }
}
