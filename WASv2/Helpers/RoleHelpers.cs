namespace WASv2.Helpers
{
    public static class RoleHelpers
    {
        // Role ID constants - match these with your AuthController
        public const string PurchasingOfficer = "1";
        public const string DepartmentHead = "2";
        public const string Supplier = "3";
        public const string DepartmentAdminStaff = "4";
        public const string PamStaff = "5";
        public const string TopManagement = "6";
        public const string ManagerDirector = "7";

        // Get role name for display
        public static string GetRoleName(string roleId)
        {
            return roleId switch
            {
                PurchasingOfficer => "Purchasing Officer",
                DepartmentHead => "Department Head",
                Supplier => "Supplier",
                DepartmentAdminStaff => "Dept Admin Staff",
                PamStaff => "PAM Staff",
                TopManagement => "Top Management",
                ManagerDirector => "Manager/Director",
                _ => "Unknown Role"
            };
        }

        // Get role name from integer (for controller use)
        public static string GetRoleName(int roleId)
        {
            return GetRoleName(roleId.ToString());
        }

        // Check if user has specific role
        public static bool IsInRole(this System.Security.Claims.ClaimsPrincipal user, string roleId)
        {
            return user.Claims.Any(c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == roleId);
        }

        // Get user's role from claims
        public static string GetUserRole(this System.Security.Claims.ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
        }

        // Get controller name based on role (for redirection)
        public static string GetDashboardController(int roleId)
        {
            return roleId switch
            {
                1 => "PurchasingOfficer",
                2 => "DepartmentHead",
                3 => "Supplier",
                4 => "DepartmentAdminStaff",
                5 => "PamStaff",
                6 => "TopManagement",
                7 => "ManagerDirector",
                _ => "Home"
            };
        }

        // Get action name based on role
        public static string GetDashboardAction(int roleId)
        {
            return "Index"; // All roles use Index action for dashboard
        }
    }
}