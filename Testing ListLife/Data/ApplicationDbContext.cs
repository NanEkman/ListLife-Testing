using ListLife.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace ListLife.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserList>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        // Create table ShoppingLists in Database DbSet
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<SharedList> SharedLists { get; set; }

        public DbSet<Product> Products { get; set; }
    }
}
