using System;
using System.Collections.Generic;

namespace StoreFront.DATA.EF.Models
{
    public partial class Shipper
    {
        public Shipper()
        {
            Orders = new HashSet<Order>();
        }

        public int ShipperId { get; set; }
        public string ShipperName { get; set; } = null!;
        public string ShipperVillage { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; }
    }
}
