using System;
using System.Collections.Generic;
using System.Text;

namespace CodexaManagement.Core.DTO.Account
{
    public class UserListView
    {
        public string Id { get; set; } = "";
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Location { get; set; }
        public bool EmailConfirmed { get; set; }
        public string RoleName { get; set; } = "";
    }
}
