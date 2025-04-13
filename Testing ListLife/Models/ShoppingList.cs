namespace ListLife.Models
{
    public class ShoppingList
    {
        public int Id { get; set; }

        // ConnectionTable
        public string UserId { get; set; }
        public UserList UserList { get; set; }

        // Name of List
        public string? Title { get; set; }

        //Navigation property
        public ICollection<SharedList> SharedWith { get; set; } = new List<SharedList>();

        // List of product in the shopping list
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}