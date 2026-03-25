using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderManagementAPI.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public int CustomerId {  get; set; }
        public Customer? Customer { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal TotalAmount {  get; set; }
        [Required, StringLength(20)]
        public string Status { get; set; } = "Pending";
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
