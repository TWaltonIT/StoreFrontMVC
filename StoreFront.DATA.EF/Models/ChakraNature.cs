using System;
using System.Collections.Generic;

namespace StoreFront.DATA.EF.Models
{
    public partial class ChakraNature
    {
        public ChakraNature()
        {
            Products = new HashSet<Product>();
        }

        public int NatureId { get; set; }
        public string NatureName { get; set; } = null!;
        public string? NatureDescription { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
