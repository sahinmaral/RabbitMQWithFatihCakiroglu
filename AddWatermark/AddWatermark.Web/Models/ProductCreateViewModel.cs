using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AddWatermark.Web.Models
{
    public class ProductCreateViewModel
    {
        [StringLength(100)]
        public string Name { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        [Range(1, 100)]
        public int Stock { get; set; }
    }
}
