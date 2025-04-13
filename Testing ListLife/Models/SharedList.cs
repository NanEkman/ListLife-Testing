namespace ListLife.Models
{
    public class SharedList
    {
        //PK
        public int Id { get; set; }
        //FK to ShoppingList
        public int ShoppingListId { get; set; }
        public ShoppingList ShoppingList { get; set; }
        public string SharedWithUserId { get; set; }
        public UserList SharedWithUser { get; set; }

    }
}
