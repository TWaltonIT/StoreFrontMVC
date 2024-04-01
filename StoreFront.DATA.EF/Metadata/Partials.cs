using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace StoreFront.DATA.EF.Models
{

    [ModelMetadataType(typeof(CategoryMetadata))]
    public partial class Category { }

    [ModelMetadataType(typeof(OrderMetadata))]
    public partial class Order { }

    [ModelMetadataType(typeof(ProductMetadata))]
    public partial class Product 
    {
        [NotMapped]
        public IFormFile? Image { get; set; }
    }

    [ModelMetadataType(typeof(ShipperMetadata))]
    public partial class Shipper { }

    [ModelMetadataType(typeof(SupplierMetadata))]
    public partial class Supplier { }

    [ModelMetadataType(typeof(UserDetailMetadata))]
    public partial class UserDetail { }

    
}
