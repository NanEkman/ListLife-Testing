using System;
using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Xunit;
using SpecFlow;
using Newtonsoft.Json.Linq;
using ListLife.Models;

namespace E2ETesting.Steps
{
    [Binding, Scope(Feature = "CreateNewShoppinglist")]
    public class CreateNewShoppinglistStepDefinitions
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IBrowserContext _context;
        private IPage _page;

        [BeforeScenario]
        public async Task Setup()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 1000 });
            _context = await _browser.NewContextAsync();
            _page = await _context.NewPageAsync();
        }

        [AfterScenario]
        public async Task Teardown()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }


        [Given(@"I am logged in")]
        public async Task GivenIAmLoggedIn()
        {
            await _page.GotoAsync("http://localhost:5240/Identity/Account/Login");

            await _page.FillAsync("input[name='Input.Email']", "user@listlife.se");
            await _page.FillAsync("input[name='Input.Password']", "User123!");

            await _page.ClickAsync("#login-submit");

            await _page.WaitForURLAsync("**/MyPage");
        }

        // Hjälpfunktion för att skapa en tom shoppinglista med enbart titel
        private async Task CreateShoppingList(string name)
        {
            await _page.GotoAsync("http://localhost:5240/CreateNewShoppinglist?");

            await _page.FillAsync("#listTitleInput", name);
            await _page.ClickAsync("#createListButton");
        }

        // Hjälpfunktion för att komma till MyPage efter listan har skapats/sparats
        private async Task<bool> IsShoppingListVisibleOnMyPage(string listName)
        {
            await _page.GotoAsync("http://localhost:5240/MyPage");

            await _page.WaitForSelectorAsync("#h2YourLists");

            var bodyText = await _page.InnerTextAsync("body");

            return bodyText.ToLower().Contains(listName.ToLower());
        }

        // Hjälpfunktion för att skapa en shoppinglista med produkt(er)
        private async Task CreateShoppingListWithProducts(string listName, List<(string category, string product, string amount)> products)
        {
            await _page.GotoAsync("http://localhost:5240/CreateNewShoppinglist?");

            // Skapa shoppinglistan
            await _page.FillAsync("#listTitleInput", listName);   
            
            foreach (var product in products)
            {
                var category = product.category;
                var productName = product.product;
                var amount = product.amount;

                var categorySelector = await _page.QuerySelectorAsync("#category");

                // Väljer kategori, fyller i produkt och mängd
                await _page.SelectOptionAsync("#category", new SelectOptionValue { Label = category });
                await _page.FillAsync("#product", productName);
                await _page.FillAsync("#amount", amount);
                await _page.ClickAsync("#addToList");
            }          
        }

        [Given(@"I am on the ""CreateNewShoppinglist"" page")]
        public async Task GivenIAmOnTheCreateNewShoppinglistPage()
        {
            await _page.GotoAsync("http://localhost:5240/CreateNewShoppinglist?");
        }

        // Steg för att skapa en shoppinglista som är tom
        [When(@"I create a shopping list called ""(.*)""")]
        public async Task WhenICreateAShoppingListCalled(string listName)
        {
            await CreateShoppingList(listName); // Anropar hjälpfunktionen
        }

        [Then(@"I should see ""(.*)"" in my shopping lists")]
        public async Task ThenIShouldSeeInMyShoppingLists(string expectedListName)
        {
            var isVisible = await IsShoppingListVisibleOnMyPage(expectedListName);
            Assert.True(isVisible, $"Expected to see '{expectedListName}' in the shopping lists on MyPage, but it was not found.");
        }

        [When(@"I create multiple shopping lists called")]
        public async Task WhenICreateMultipleShoppingListsCalled(Table table)
        {
            foreach (var row in table.Rows)
            {
                var listName = row["listName"];

                await CreateShoppingList(listName);

                await _page.GotoAsync("http://localhost:5240/CreateNewShoppinglist?");
            }
        }

        [Then(@"I should see multiple shopping lists")]
        public async Task ThenIShouldSeeMultipleShoppingLists(Table table)
        {
            foreach (var row in table.Rows)
            {
                var expectedListName = row["listName"];
                var isVisible = await IsShoppingListVisibleOnMyPage(expectedListName);
                Assert.True(isVisible, $"Expected to see '{expectedListName}' in the shopping lists, but it was not found.");
            }
        }

        [When(@"I create a new shopping list called ""([^""]*)"" with 2 products")]
        public async Task WhenICreateANewShoppingListCalledWithProducts(string listName, Table productTable)
        {
            List<(string category, string product, string amount)> products = new List<(string category, string product, string amount)>();

            foreach (var row in productTable.Rows)
            {
                var category = row["Category"];
                var product = row["Product"];
                var amount = row["Amount"];               
                products.Add((category, product, amount));
            }

            await CreateShoppingListWithProducts(listName, products);
        }

        [Then(@"I should see the list ""(.*)"" in my shopping lists")]
        public async Task ThenIWillSeeMyNewShoppingList(string expectedListName)
        {
            var isVisible = await IsShoppingListVisibleOnMyPage(expectedListName);
            Assert.True(isVisible, $"Expected to see '{expectedListName}' in the shopping lists on MyPage, but it was not found.");
        }

        [Given(@"I am on the page ""CreateNewShoppinglist""")]
        public async Task GivenIAmOnThePageCreateNewShoppinglist()
        {
            await _page.GotoAsync("http://localhost:5240/CreateNewShoppinglist");
        }

        [When(@"I create a shopping list with the title ""(.*)""")]
        public async Task WhenICreateShoppingListCalled(string listName)
        {
            await _page.FillAsync("#listTitleInput", listName);
        }

        [When(@"I select the category ""(.*)""")]
        public async Task WhenISelectTheCategory(string category)
        {
            await _page.SelectOptionAsync("#category", new SelectOptionValue { Label = category });
        }

        [When(@"I write ""(.*)"" as the product")]
        public async Task WhenIWriteAsTheProduct(string product)
        {
            await _page.FillAsync("#product", product);
        }

        [When(@"I choose an amount")]
        public async Task WhenIChooseAnAmount()
        {
            await _page.FillAsync("#amount", "1");
        }

        [When(@"I click ""Add to list""")]
        public async Task WhenIClickAddToList()
        {
            var addToList = await _page.QuerySelectorAsync("#addToList");
            await addToList.ClickAsync();
        }

        [Then(@"an error message should appear saying ""(.*)""")]
        public async Task ThenAnErrorMessageShouldAppearSaying(string expectedMessage)
        {
            // Vänta på att felmeddelandet ska visas
            var errorMessageElement = await _page.QuerySelectorAsync("#errorMessage");

            if (errorMessageElement != null)
            {
                var errorMessageText = await errorMessageElement.InnerTextAsync();
                Assert.Equal(expectedMessage, errorMessageText);
            }
            else
            {
                // Om inget felmeddelande syns, så visar det på en brist
                Assert.Fail("Expected error message 'The product does not belong to the selected category' did not appear.");
            }
        }



    }
}

