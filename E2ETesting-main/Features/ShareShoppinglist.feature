Feature: ShareShoppinglist

The user should be able to share list with other registered users

Scenario: I can share a list with other users
	Given I am logged in with existing user
	When I try to share the shopping list "Testing share list function" with an unregistered user with the email "unregistered@example.com"
	Then I should see the share list error message "User not found"
