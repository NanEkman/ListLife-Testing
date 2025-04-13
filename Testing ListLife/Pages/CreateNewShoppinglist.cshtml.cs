using ListLife.Data;
using ListLife.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ListLife.Pages
{

    public class CreateNewShoppingList : PageModel
    {
        // instance of the database to store lists
        private readonly ApplicationDbContext Dbcontext;

        // Manages users via ASP.NET Identity (UserManager) to retrieve and manage logged in users
        private readonly UserManager<UserList> userManager;

        // Constructor
        public CreateNewShoppingList(ApplicationDbContext context, UserManager<UserList> userManager)
        {
            this.Dbcontext = context;
            this.userManager = userManager;
        }

        [BindProperty]
        // Property to hold users shopping lists 
        public ShoppingList ShoppingList { get; set; }

        public string UserListName { get; set; }

        [BindProperty]  
        public List<Product> Products { get; set; } = new List<Product>();

        public List<ShoppingList> ShoppingLists { get; set; } = new List<ShoppingList>();

        public List<Product> RecentlyPurchasedProducts { get; set; } = new List<Product>();



        public async Task OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User); // Get the logged in user
            if (user != null)
            {
                UserListName = user.ListName;

                ShoppingLists = await Dbcontext.ShoppingLists
                    .Include(s => s.Products)
                    .Where(s => s.UserId == user.Id)
                    .ToListAsync();

                // Retrieve recently purchased products (last 10 products from previous shopping lists)
                RecentlyPurchasedProducts = await Dbcontext.Products
                    .Where(p => p.ShoppingList.UserId == user.Id)
                    .OrderByDescending(p => p.Id) // Latest first 
                    .Take(10)
                    .ToListAsync();
            }
        }

        public virtual async Task<IActionResult> OnPostAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                var newShoppingList = new ShoppingList
                {
                    UserId = user.Id,
                    UserList = user,
                    Title = ShoppingList.Title ?? "New Shopping List",
                    Products = new List<Product>()
                };

                Dbcontext.ShoppingLists.Add(newShoppingList);
                await Dbcontext.SaveChangesAsync(); // Saves shopping list and gets ID

                // Add products to the connected shopping list
                foreach (var product in Products)
                {
                    product.ShoppingListId = newShoppingList.Id; // Link the product to the correct list
                    Dbcontext.Products.Add(product);
                }

                await Dbcontext.SaveChangesAsync(); // Save products to database 

                await Dbcontext.SaveChangesAsync(); // Save products in DB

                // Rensa product input fält
                ModelState.Remove("Products");
                Products = new List<Product>();                
            }

            return RedirectToPage("/MyPage");
        }
    }
}
    
