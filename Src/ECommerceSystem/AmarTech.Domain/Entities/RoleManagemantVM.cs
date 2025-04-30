using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace AmarTech.Domain.Entities
{
    [ValidateNever]
    public class RoleManagemantVM
    {
        public ApplicationUser? User { get; set; }
        public IEnumerable<SelectListItem> RoleList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> CompanyList { get; set; } = new List<SelectListItem>();
    }
}
