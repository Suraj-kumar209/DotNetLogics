using System.ComponentModel.DataAnnotations;

namespace OrderManagementAPI.Dtos
{
    public class CreateOrderDTO
    {
        [Required]
        [Range(1,int.MaxValue,ErrorMessage ="CustomerId must be a positive number.")]
        public int CustomerId { get; set; }
        [Required(ErrorMessage = "At least one item is required.")]
        public List<CreateOrderItemDTO> CreateOrderItemDTOs { get; set; } = new List<CreateOrderItemDTO>();
    }
}
