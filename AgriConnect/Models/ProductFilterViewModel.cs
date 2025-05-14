using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgriConnect.Models
{
    public class ProductFilterViewModel
    {
        public string SelectedFarmerId { get; set; }
        public string ProductCategory { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<SelectListItem> FarmerOptions { get; set; } = new();
        public IEnumerable<ProductEntity> FilteredProducts { get; set; } = new List<ProductEntity>();
        public Dictionary<string, string> FarmerUsernames { get; set; } = new Dictionary<string, string>();
    }
}
