using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ListLife.Data;
using ListLife.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace ListLife.Pages
{
    [Authorize]
    public class MyPageModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserList> _userManager;

        public MyPageModel(ApplicationDbContext context, UserManager<UserList> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //Properties for the sharing list functionality
        [BindProperty]
        public string UserEmail { get; set; }
        public string Message { get; set; }

        //Hold the user's lists
        public IList<UserList> UserList { get; set; }

        // List to hold shopping lists for the logged-in user
        public IList<ShoppingList> ShoppingLists { get; set; }
        public IList<ShoppingList> SharedShoppingLists { get; set; } = new List<ShoppingList>();

        public IList<Product> Products { get; set; }

        [BindProperty]
        public ShoppingList EditList { get; set; }

        [BindProperty]
        public Product AddNewProduct { get; set; }

        public bool IsEditing { get; set; }

        // Get Shoppinglists based on user
        public async Task OnGetAsync()
        {
            // Get user
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {

                //Get user's shopping lists
                ShoppingLists = await _context.ShoppingLists.Where(u => u.UserId == user.Id).ToListAsync();

                //Get lists that are shared with the user
                SharedShoppingLists = await _context.SharedLists
                    .Where(sl => sl.SharedWithUserId == user.Id)
                    .Include(sl => sl.ShoppingList)
                    .Select(sl => sl.ShoppingList)
                    .ToListAsync();
            }
        }

        // Get ShoppingId based on Id and associated products
        public async Task<IActionResult> OnGetEditAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get user
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                //Get user's shopping lists
                ShoppingLists = await _context.ShoppingLists.Where(u => u.UserId == user.Id).ToListAsync();
            }

            if (EditList != null)
            {
                // Get user's shopping lists and include related products
                ShoppingLists = await _context.ShoppingLists
                    .Where(u => u.UserId == user.Id)
                    .Include(sl => sl.Products)  // Include products
                    .ToListAsync();
            }

            if (EditList == null || (EditList.Products != null && !EditList.Products.Any()))
            {
                Message = "No products added yet.";
            }

            return Page();
        }


        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            // Get the logged in User's ID
            var userId = _userManager.GetUserId(User);
            var newList = new UserList
            {
                ListName = Request.Form["ListName"],
                Id = userId
            };

            // Add list to database
            _context.Users.Add(newList);
            await _context.SaveChangesAsync();


            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostShareAsync(int listId)
        {
            if (string.IsNullOrWhiteSpace(UserEmail))
            {
                Message = "Enter a valid email address";
                return RedirectToPage();
            }
            var userToShareWith = await _userManager.FindByEmailAsync(UserEmail);
            if (userToShareWith == null)
            {
                //Message for alert if user not found
                TempData["Message"] = "User not found";
                TempData["MessageType"] = "error";
                return RedirectToPage();
            }
            //Control if the list is already shared with the user
            bool alreadyShared = await _context.SharedLists
                .AnyAsync(sl => sl.ShoppingListId == listId && sl.SharedWithUserId == userToShareWith.Id);

            if (alreadyShared)
            {
                //Message for alert if list is already shared
                TempData["Message"] = "List is already shared with this user";
                TempData["MessageType"] = "error";
                return RedirectToPage();
            }

            var sharedList = new SharedList
            {
                SharedWithUserId = userToShareWith.Id,
                ShoppingListId = listId
            };

            _context.SharedLists.Add(sharedList);
            await _context.SaveChangesAsync();

            //Message for alert if list is shared successfully
            TempData["Message"] = "List shared successfully!";
            TempData["MessageType"] = "success";
            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostDeleteAsync(int listId)
        {
            // Hämta shoppinglistan från databasen baserat på listId
            var deleteList = await _context.ShoppingLists.FindAsync(listId);

            if (deleteList != null)
            {
                _context.ShoppingLists.Remove(deleteList);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
        public async Task<JsonResult> OnGetListDetailsAsync(int id)
        {
            var shoppingList = await _context.ShoppingLists
                .Include(sl => sl.Products)
                .FirstOrDefaultAsync(sl => sl.Id == id);

            if (shoppingList == null)
            {
                return new JsonResult(new { success = false, message = "List not found" });
            }

            var listDetails = new
            {
                Title = shoppingList.Title,
                Products = shoppingList.Products.Select(p => new
                {
                    p.Name,
                    p.Amount,
                    p.Category
                })
            };

            return new JsonResult(new { success = true, data = listDetails });
        }

        public async Task<JsonResult> OnGetSharedListDetailsAsync(int id)
        {
            var sharedList = await _context.SharedLists
                .Include(sl => sl.ShoppingList)
                .ThenInclude(sl => sl.Products)
                .FirstOrDefaultAsync(sl => sl.ShoppingListId == id);

            if (sharedList == null)
            {
                return new JsonResult(new { success = false, message = "Shared list not found" });
            }

            var listDetails = new
            {
                Title = sharedList.ShoppingList.Title,
                Products = sharedList.ShoppingList.Products.Select(p => new
                {
                    p.Name,
                    p.Amount,
                    p.Category
                })
            };

            return new JsonResult(new
            {
                success = true,
                data = listDetails
            });
        }

        public async Task<IActionResult> OnPostEditAsync(int listId)
        {
            // Get list from database based on listId
            var editList = await _context.ShoppingLists
                .Include(sl => sl.Products)  // Include products
                .FirstOrDefaultAsync(sl => sl.Id == listId);

            if (editList == null)
            {
                return NotFound();
            }

            EditList = editList;
            IsEditing = true;

            return Page();
        }


        // POST to add new product to the list and save to the database
        public async Task<IActionResult> OnPostAddProductAsync(int shoppingListId, int? editProductId)
        {
            // Fetch the shopping list including its products
            var shoppingList = await _context.ShoppingLists
                .Include(sl => sl.Products)
                .FirstOrDefaultAsync(sl => sl.Id == shoppingListId);

            if (shoppingList == null)
            {
                return NotFound();
            }

            if (editProductId.HasValue && editProductId.Value > 0)
            {
                // Update an existing product
                var existingProduct = shoppingList.Products.FirstOrDefault(p => p.Id == editProductId.Value);
                if (existingProduct == null)
                {
                    return NotFound();
                }
                existingProduct.Name = AddNewProduct.Name;
                existingProduct.Amount = AddNewProduct.Amount;
                existingProduct.Category = AddNewProduct.Category;
            }
            else
            {
                // Create a new product and add it to the shopping list
                var newProduct = new Product
                {
                    Name = AddNewProduct.Name,
                    Amount = AddNewProduct.Amount,
                    Category = AddNewProduct.Category,
                    ShoppingListId = shoppingListId   // FK relationship
                };

                _context.Products.Add(newProduct);
            }

            await _context.SaveChangesAsync();

            // Reset the model state so that old values are not remembered
            ModelState.Clear();
            AddNewProduct = new Product();

            EditList = shoppingList;
            IsEditing = true;
            return Page();
        }

        public async Task<IActionResult> OnPostEditProductAsync()
        {
            var product = await _context.Products.FindAsync(AddNewProduct.Id);

            if (product == null)
            {
                return NotFound();
            }

            // Update product properties
            product.Name = AddNewProduct.Name;
            product.Amount = AddNewProduct.Amount;
            product.Category = AddNewProduct.Category;

            await _context.SaveChangesAsync();

            // Return to the same page or another page after saving changes
            return RedirectToPage("/MyPage", new { id = product.ShoppingListId });
        }

        public async Task<IActionResult> OnPostDeleteProductAsync(int productId)
        {
            // Get product based on product ID 
            var product = await _context.Products
                .Include(p => p.ShoppingList) // Make sure shoppinglist is included
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product != null)
            {
                // Get the list that the products are connected to
                var shoppingListId = product.ShoppingList.Id;

                // Remove product
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                // Get the updated list and it's products
                EditList = await _context.ShoppingLists
                    .Include(s => s.Products)
                    .FirstOrDefaultAsync(s => s.Id == shoppingListId);

                // Set IsEditing to true to remain in edit mode
                IsEditing = true;
            }

            // Return to the same page and update list
            return Page();
        }

    }
}