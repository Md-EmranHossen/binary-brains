using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceSystem.Models
{
   public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        public decimal Price { get; set; }
       // [Required]
       // public int UnitOfMeasurementId { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        public decimal DiscountAmount { get; set; } // cut kore je price ta  dekai 

        public bool IsActive { get; set; } = true;

        [Required]
        public String CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? UpdatedBy { get; set; } 
        public DateTime? UpdatedDate { get; set; } 
    }
}
