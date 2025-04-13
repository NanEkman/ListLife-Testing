using System;
using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Xunit;
using SpecFlow;
using Newtonsoft.Json.Linq;
using ListLife.Models;
using Microsoft.Playwright.Xunit;

namespace E2ETesting.Steps
{
    [Binding]
    public class RegistrationAndLoginStepDefinitions
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

        [Given(@"I am on the registration page")]
        public async Task GivenIAmOnTheRegistrationPage()
        {
            await _page.GotoAsync("http://localhost:5240/Identity/Account/Register");
        }

        [When(@"I register with the email address ""([^""]*)""")]
        public async Task WhenIRegisterWithTheEmailAddress(string email)
        {
            await _page.FillAsync("input[name='Input.Email']", "testingRegistration@listlife.com");
        }

        [When(@"the password is ""([^""]*)""")]
        public async Task WhenThePasswordIs(string password)
        {
            await _page.FillAsync("input[name='Input.Password']", "testUser123!");
            await _page.FillAsync("input[name='Input.ConfirmPassword']", "testUser123!");
        }

        [When(@"I click the register button")]
        public async Task WhenIClickTheRegisterButton()
        {
            await _page.ClickAsync("#registerSubmit");
        }

        [Then(@"I should be redirected to ""([^""]*)""")]
        public async Task ThenIShouldBeRedirectedTo(string expectedPage)
        {
            await _page.WaitForURLAsync($"**/{expectedPage}");
        }



        // Scenario Outline Test för inloggning
        [Given(@"I have a registered user with the email ""([^""]*)"" and password ""([^""]*)""")]
        public async Task GivenIHaveARegisteredUserWithTheEmailAndPassword(string email, string password)
        {
            await _page.GotoAsync("http://localhost:5240/Identity/Account/Login");
        }

        [When(@"I try to log in with the email ""([^""]*)"" and the password ""([^""]*)""")]
        public async Task WhenITryToLogInWithTheEmailAndThePassword(string email, string password)
        {
            await _page.FillAsync("input[name='Input.Email']", email);
            await _page.FillAsync("input[name='Input.Password']", password);            
        }

        [Then(@"I should see the login (.*)")]
        public async Task ThenIShouldOutcome(string loggedIn)
        {
            await _page.ClickAsync("#login-submit");

            if (loggedIn == "succeed")
            {
                await _page.WaitForURLAsync("**/MyPage");
                var currentUrl = _page.Url;
                Assert.Contains("MyPage", currentUrl);
            }
            else if (loggedIn == "fail")
            {
                var currentUrl = _page.Url;
                Assert.Equal("http://localhost:5240/Identity/Account/Login", currentUrl);
            }
        }
    }
}
