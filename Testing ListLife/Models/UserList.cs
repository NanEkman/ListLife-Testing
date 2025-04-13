using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ListLife.Models
{
    public class UserList : IdentityUser
    {
        ////Name of the list
        public string? ListName { get; set; }

        //FK to user

        public List<ShoppingList> ShoppingLists { get; set; } = new();

    }
}
