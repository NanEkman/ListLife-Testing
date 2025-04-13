using ListLife.Models;
using ListLife.Pages;
using Microsoft.AspNetCore.Mvc;

namespace E2ETesting.UnitTesting
{

    // Arrange - Förbereder testet med mockad data
    // Act - kör metoden som ska testas
    // Assert - testar om metoden är korrekt

    public class CreateNewShippingListTests
    {
        // Falsk implementation av CreateNewShoppingList för att simulera testbeteende i OnPostAsync-metoden i CreateNewShoppingList page:n
        public class CreateNewShoppingListMock : CreateNewShoppingList
        {
            public List<ShoppingList> ShoppingLists { get; set; } = new List<ShoppingList>();

            public CreateNewShoppingListMock() : base(null, null) { }

            public override async Task<IActionResult> OnPostAsync()
            {
                var newShoppingList = new ShoppingList
                {
                    Title = ShoppingList.Title ?? "New Shopping List",
                    Products = Products
                };

                // merge:a produkter med samma namn
                foreach (var product in Products)
                {
                    var existingProduct = newShoppingList.Products
                        .FirstOrDefault(existingItem => existingItem.Name == product.Name);
                    if (existingProduct != null)
                    {
                        existingProduct.Amount += product.Amount;
                    }
                    else
                    {
                        newShoppingList.Products.Add(product);
                    }
                }

                ShoppingLists.Add(newShoppingList);

                // Simulera att användaren omdirigeras till MyPage
                return new RedirectToPageResult("/MyPage");
            }
        }

        [Fact]
        public async Task OnPostAsync_CreatingAShoppingList()
        {
            // Arrange
            var pageModel = new CreateNewShoppingListMock();
            pageModel.ShoppingList = new ShoppingList { Title = "Friday Shopping" };
            pageModel.Products = new List<Product>
            {
                new Product { Name = "Milk", Amount = 1 },
                new Product { Name = "Bread", Amount = 2 }
            };

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/MyPage", redirectResult.PageName);

            var createdList = pageModel.ShoppingLists.Find(shoppingList => shoppingList.Title == "Friday Shopping");
            Assert.NotNull(createdList);
            Assert.Equal("Friday Shopping", createdList.Title);

            Assert.Equal(2, pageModel.Products.Count);
        }

        [Theory]
        [InlineData("Weekly Groceries")]
        [InlineData("Party Shopping")]
        [InlineData("Taco Friday")]
        public async Task OnPostAsync_CreatingMultipleShoppingLists(string title)
        {
            // Arrange
            var pageModel = new CreateNewShoppingListMock();
            pageModel.ShoppingList = new ShoppingList { Title = title };
            pageModel.Products = new List<Product>
            {
                new Product { Name = "Milk", Amount = 1 },
                new Product { Name = "Bread", Amount = 2 }
            };

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/MyPage", redirectResult.PageName);

            var createdList = pageModel.ShoppingLists.Find(shoppingList => shoppingList.Title == title);
            Assert.NotNull(createdList);
            Assert.Equal(title, createdList.Title);

            Assert.Equal(2, pageModel.Products.Count);
        }

        [Fact]
        public async Task AddingSameProductTwiceShouldMergeQuantities()
        {
            // Arrange
            var pageModel = new CreateNewShoppingListMock();
            pageModel.ShoppingList = new ShoppingList { Title = "Weekend Shopping" };

            pageModel.Products = new List<Product>
            {
                new Product { Name = "Milk", Amount = 1 },
                new Product { Name = "Bread", Amount = 2 },
                new Product { Name = "Milk", Amount = 3 } 
            };

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/MyPage", redirectResult.PageName);

            var createdList = pageModel.ShoppingLists.Find(shoppingList => shoppingList.Title == "Weekend Shopping");
            Assert.NotNull(createdList);
            Assert.Equal("Weekend Shopping", createdList.Title);

            Assert.Equal(2, createdList.Products.Count); // Should only have 2 unique products
            Assert.Equal(4, createdList.Products.First(product => product.Name == "Milk").Amount); // Milk should have merged quantities
            Assert.Equal(2, createdList.Products.First(product => product.Name == "Bread").Amount); // Bread should remain the same
        }
    }
}
