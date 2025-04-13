Feature: CreateNewShoppinglist

The user should be able to correcly create a new shopping list, add products and manage categories.
The application should also handle duplicate products properly and prevent incorrect product when choosing a specific category.

Background: 
    Given I am logged in

Scenario: Create a new shopping list
    Given I am on the "CreateNewShoppinglist" page
    When I create a shopping list called "Weekend Groceries"
    Then I should see "Weekend Groceries" in my shopping lists

Scenario: Create multiple shopping lists
    Given I am on the "CreateNewShoppinglist" page
    When I create a shopping list called "Party Snacks"
    And I create a shopping list called "BBQ Friday"
    Then I should see "Party Snacks" in my shopping lists
    And I should see "BBQ Friday" in my shopping lists

Scenario: Creating new shopping list with 2 products
	Given I am on the "CreateNewShoppinglist" page
	When I create a new shopping list called "Fruit Deluxe" with 2 products
		| Category				| Product	| Amount |
		| Fruits & Vegetables	| Banana	| 2      |
		| Fruits & Vegetables	| Apple		| 3      |
	Then I should see the list "Fruit Deluxe" in my shopping lists

  Scenario: Adding a product that does not belong to the selected category
    Given I am on the page "CreateNewShoppinglist"
    When I create a shopping list with the title "Monthly Shopping"
    And I select the category "Fruits & Vegetables"
    And I write "milk" as the product
    And I choose an amount
    And I click "Add to list"
    Then an error message should appear saying "The product does not belong to the selected category"



#Scenario: When writing a product I will get recommendations
#	Given I am on the page "CreateNewShoppinglist"
#	When I write "ban" in the product input
#	Then I should see a list of recommended products
#	And I should be able to select a product from the list
#	And the selected product should be added to the shopping list
#
#Scenario: When leaving an input field empty
#	Given I am on the page "CreateNewShoppinglist"
#   When I choose category "Fridge"
#	And I leave the product input empty
#	Then I should see an error message saying "Product name cannot be empty"
#	And I should not be able to add the product to the shopping list
#
#Scenario: Deleting a shoppinglist
#    Given I am on the page "CreateNewShoppinglist"
#    When I delete a shoppinglist
#    Then Then the shoppinglist will be deleted
#




