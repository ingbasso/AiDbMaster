using System.Collections.Generic;

namespace AiDbMaster.ViewModels
{
    public class UserViewModel
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public bool IsActive { get; set; }
        public List<string>? Roles { get; set; } = new List<string>();
    }
} 