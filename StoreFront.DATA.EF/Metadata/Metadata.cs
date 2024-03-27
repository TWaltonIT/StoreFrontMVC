using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace StoreFront.DATA.EF.Models
{
    
    public class CategoryMetadata
    {
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Category")]
        [StringLength(50)]
        public string CategoryName { get; set; } = null!;

        [Display(Name = "Description")]
        [StringLength(150)]
        public string? CategoryDescription { get; set; }
    }

    public class OrderMetadata
    {
        public int OrderId { get; set; }
        public string UserId { get; set; } = null!;

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [Display(Name = "Order Date")]
        [Required(ErrorMessage = "Order Date is required.")]
        public DateTime OrderDate { get; set; }

        [StringLength(100)]
        [Display(Name = "Ship To")]
        [Required]
        public string ShipToName { get; set; } = null!;

        [StringLength(150)]
        [Display(Name = "Village")]
        [Required]
        public string ShipVillage { get; set; } = null!;

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [Display(Name = "Ship Date")]
        [Required]
        public DateTime ShipDate { get; set; }
        public int ShipperId { get; set; }
    }

    public class ProductMetadata
    {
        public int ProductId { get; set; }

        [Display(Name = "Product")]
        [StringLength(150)]
        [Required]
        public string ProductName { get; set; } = null!;

        [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:c}")]
        [Display(Name = "Price")]
        [Range(0, (double)decimal.MaxValue)]
        [Required]
        public decimal ProductPrice { get; set; }

        [Display(Name = "Description")]
        [StringLength(500)]
        [UIHint("MultilineText")]
        [DataType(DataType.MultilineText)]
        public string? ProductDescription { get; set; }

        [Display(Name = "In Stock")]
        [Range(0, short.MaxValue)]
        [Required]
        public short ItemsInStock { get; set; }

        [Display(Name = "On Order")]
        [Range(0, short.MaxValue)]
        [Required]
        public short ItemsOnOrder { get; set; }
        public int? CategoryId { get; set; }
        public int? SupplierId { get; set; }

        [Display(Name ="Discontinued?")]
        public bool IsDiscontinued { get; set; }

        [StringLength(75)]
        [Display(Name = "Image")]
        public string? ProductImage { get; set; }
        public int? NatureId { get; set; }
        public int? ProductStatusId { get; set; }
    }

    public class ShipperMetadata
    {
        public int ShipperId { get; set; }

        [Display(Name = "Shipper")]
        [StringLength(150)]
        [Required]
        public string ShipperName { get; set; } = null!;

        [Display(Name = "Village")]
        [StringLength(150)]
        [Required]
        public string ShipperVillage { get; set; } = null!;
    }

    public class SupplierMetadata
    {
        public int SupplierId { get; set; }

        [Display(Name = "Supplier")]
        [StringLength(100)]
        [Required]
        public string SupplierName { get; set; } = null!;

        [Display(Name = "Village")]
        [StringLength (150)]
        [Required]
        public string SupplierVillage { get; set; } = null!;
    }

    public class UserDetailMetadata
    {
        public string UserId { get; set; } = null!;

        [Display(Name = "First Name")]
        [StringLength(50)]
        [DataType(DataType.Text)]
        [Required]
        public string FirstName { get; set; } = null!;

        [Display(Name = "Last Name")]
        [StringLength(50)]
        [DataType(DataType.Text)]
        [Required]
        public string? LastName { get; set; }

        [Display(Name = "Village")]
        [StringLength(150)]
        [Required]
        public string Village { get; set; } = null!;
    }

}
