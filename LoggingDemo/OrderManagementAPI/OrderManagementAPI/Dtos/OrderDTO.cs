namespace OrderManagementAPI.Dtos
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public string CustomerName {  get; set; }=string.Empty;
        public DateTime OrderDate {get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItemDTO> Items { get; set; } = new List<OrderItemDTO>();
    }
}
