using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ECommerceSystem.API.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage ="Category Must Be Required.")]
        [DisplayName("Category Name")]
        [MaxLength(30)]
        public string Name { get; set; }
        [Required(ErrorMessage ="Display Order Must Be Required.")]
        [DisplayName("Display Order")]
        [Range(1,100,ErrorMessage ="Display Order must be between 1-100")]
        public int DsiplayOrder { get; set; }
        public bool IsActive { get; set; }
        [Required(ErrorMessage ="Created Name Must Be Required")]
        [DisplayName("Created By")]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }

    }
}
