using System.ComponentModel.DataAnnotations;

namespace OrderManagementAPI.Dtos
{
    public class CreateOrderItemDTO
    {
        [Required(ErrorMessage ="ProductId is required.")]
        [Range(2,int.MaxValue,ErrorMessage ="ProductId must be a positive number.")]
        public int ProductId {  get; set; }
        [Range(1,int.MaxValue,ErrorMessage ="Quantity must be at least 1.")]
        public int Quantity {  get; set; }
    }
}
