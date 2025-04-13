using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using ListLife.Models;
using ListLife.Pages;
using ListLife.Data;
using Microsoft.AspNetCore.Identity;

namespace E2ETesting.UnitTesting
{
    // Arrange - lägger till mockad data i in-memory database
    // Act - kör metoden
    // Assert - testar om metoden är korrekt

    public class MyPageModelTests
    {
        // Mockad objekt för UserManager som hanterar användare 
        private readonly Mock<UserManager<UserList>> userManager;

        public MyPageModelTests()
        {
            // Mockad objekt för UserManager som hanterar användare
            var userStore = new Mock<IUserStore<UserList>>();

            userManager = new Mock<UserManager<UserList>>
            (
                userStore.Object, null, null, null, null, null, null, null, null
            );
        }

        [Fact]
        public async Task OnPostAddProductAsync_AddingProductToShoppingList()
        {
            // Arrange
            // Mocka databaskopplingen med In-Memory-Database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "MockadTestDatabase")
                .Options;

            // Instans av min databas ApplicationDbContext med in-memory databas
            using var context = new ApplicationDbContext(options);

            var shoppingList = new ShoppingList
            {
                Id = 1,
                Title = "Weekly Groceries",
                Products = new List<Product>(),
                UserId = "User@listlife.se"
            };

            context.ShoppingLists.Add(shoppingList);
            await context.SaveChangesAsync(); 
                       
            // Skapa en instans av MyPageModel med den mockade databasenm, dvs. simulera användarens input
            var pageModel = new MyPageModel(context, userManager.Object)
            {
                AddNewProduct = new Product
                {
                    Name = "Milk",
                    Amount = 1,
                    Category = "Fridge"
                }
            };

            // Act
            // Kör metoden för att lägga till produkt och Lägger till produkten i listan
            await pageModel.OnPostAddProductAsync(shoppingListId: 1, editProductId: null);  

            // Assert
            // Kontrollerar att produkten är tillagd i shoppinglistan
            var updatedList = await context.ShoppingLists
                .Include(shoppingList => shoppingList.Products)  // Inkludera produkter i resultatet
                .FirstOrDefaultAsync(shoppingList => shoppingList.Id == 1);  // Hämta shoppinglistan med ID 1       

            // Kontrollerar att den uppdaterade shoppinglistan inte är null, att en produkt har lagts till och
            // att den tillagda produkten har korrekt namn, mängd och kategori
            Assert.NotNull(updatedList);
            Assert.Single(updatedList.Products);  
            Assert.Equal("Milk", updatedList.Products.First().Name);  
            Assert.Equal(1, updatedList.Products.First().Amount); 
            Assert.Equal("Fridge", updatedList.Products.First().Category); 
        }

        /* Om det här testet går igenom – så är det något som saknas i koden, 
         * dvs. att ingen validering hindrar att man lägger till negativt amount.*/
        [Fact]
        public async Task OnPostAddProductAsync_ShouldNotBeAbleToAddProductWithNegativeAmount()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "MockadTestDatabase")
                .Options;

            using var context = new ApplicationDbContext(options);

            var shoppingList = new ShoppingList
            {
                Id = 2,  
                Title = "Test List",
                Products = new List<Product>(),
                UserId = "user@example.com"
            };

            context.ShoppingLists.Add(shoppingList);
            await context.SaveChangesAsync();  

            // Lägger till produkt med negativt amount
            var pageModel = new MyPageModel(context, userManager.Object)
            {
                AddNewProduct = new Product
                {
                    Name = "Milk",
                    Amount = -1,  // Negativ mängd
                    Category = "Fridge"
                }
            };

            // Act
            await pageModel.OnPostAddProductAsync(shoppingListId: 2, editProductId: null);  

            // Assert
            // Kontrollera att shoppinglistan fortfarande finns
            var updatedList = await context.ShoppingLists
                .Include(shoppingList => shoppingList.Products)
                .FirstOrDefaultAsync(shoppingList => shoppingList.Id == 2);  

            Assert.NotNull(updatedList); 

            // Ingen produkt ska ha lagts till
            Assert.Empty(updatedList.Products);  
        }

    }
}
