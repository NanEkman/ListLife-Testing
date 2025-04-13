namespace ListLife.Models
{
    public class Product
    {
        // PK for product
        public int Id { get; set; }  
        public string Name { get; set; }  
        public int Amount { get; set; } 
        public string Category { get; set; } 

        // FK to Shopping list
        public int ShoppingListId { get; set; }
        public ShoppingList ShoppingList { get; set; }  
    }
}
