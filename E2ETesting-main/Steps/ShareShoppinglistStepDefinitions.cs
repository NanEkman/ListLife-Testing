using Xunit;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;


namespace E2ETesting.Steps
{
    [Binding]
    public class ShareShoppinglistStepDefinitions
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

        [Given(@"I am logged in with existing user")]
        public async Task GivenIAmLoggedIn()
        {
            await _page.GotoAsync("http://localhost:5240/Identity/Account/Login");

            await _page.FillAsync("input[name='Input.Email']", "user@listlife.se");
            await _page.FillAsync("input[name='Input.Password']", "User123!");

            await _page.ClickAsync("#login-submit");

            await _page.WaitForURLAsync("**/MyPage");
        }

        [When(@"I try to share the shopping list ""([^""]*)"" with an unregistered user with the email ""([^""]*)""")]
        public async Task WhenITryToShareAShoppingListWithAnUnregisteredUser(string listName, string email)
        {
            // Klickar på "Share"-knappen för shoppinglistan med namnet '{listName}' med användning av CSS-Selector för att integrera med HTML-element
            await _page.ClickAsync($"li .list-title:has-text('{listName}') + .button-group form button[title='Share']");

            // Använder XPath för att navigera och på så sätt nå rätt form (som var hidden) baserat på listName
            var emailInputSelector = $"xpath=//span[contains(text(), '{listName}')]/ancestor::li//form[@id[contains(., 'emailForm-')]]//input[@name='UserEmail']";

            await _page.Locator(emailInputSelector).WaitForAsync(new() { State = WaitForSelectorState.Visible });

            // Fyller i e-postadressen
            await _page.FillAsync(emailInputSelector, email);

            // Klickar på 'Send'-knappen i samma form
            var sendButtonSelector = $"xpath=//span[contains(text(), '{listName}')]/ancestor::li//form[@id[contains(., 'emailForm-')]]//button[@type='submit']";
            await _page.ClickAsync(sendButtonSelector);
        }

        [Then(@"I should see the share list error message ""([^""]*)""")]
        public async Task ThenIShouldSeeTheMessage(string expectedMessage)
        {
            var messageLocator = _page.Locator(".alert"); 
            await messageLocator.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            var actualMessage = await messageLocator.InnerTextAsync();
            Assert.Contains(expectedMessage, actualMessage);
        }
    }
}
