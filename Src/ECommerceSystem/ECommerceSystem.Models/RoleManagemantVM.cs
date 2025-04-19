using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ECommerceSystem.Models
{
    public class RoleManagemantVM
    {
        public ApplicationUser? User { get; set; } 

        public IEnumerable<SelectListItem> RoleList { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> CompanyList { get; set; } = new List<SelectListItem>();
    }
}
