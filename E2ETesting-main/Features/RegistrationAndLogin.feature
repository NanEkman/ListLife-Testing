Feature: RegistrationAndLogin

User registration, validation and log in


Scenario: Users can register without email verification (current implementation)
	Given I am on the registration page
	When I register with the email address "testingRegistration@listlife.com"
	And the password is "testUser123!"
	And I click the register button
	Then I should be redirected to "MyPage"

Scenario Outline: Login with different combinations of email and password
  Given I have a registered user with the email "<email>" and password "<password>"
  When I try to log in with the email "<email>" and the password "<password>"
  Then I should see the login <outcome>

Examples:
	| email                | password			| loggedIn  |
	| user@listlife.se     | User123!			| succeed |
	| user@listlife.se     | WrongPass123		| fail    |
	| testUser@testUser.se | testUser123!		| succeed |
	| testUser@testUser.se | User123!			| fail    |
	| user@user.com        | WrongPass123		| fail    |



#
#Scenario: Users should be required to verify their email before logging in (desired implementation) 
#	Given I am on the registration page
#	When I register with the email address "user@listlife.se"
#	And the password is "User123!"
#	Then I should receive an verification link in my email
#	When I try to log in without veryfying my email
#	Then I should see an error message "User not verified. Please verify your email before logging in."
#	And I should not be logged in
#
#Scenario: Input fields should display instructions before interaction
#	Given I am on the registration page
#	When I look at the registration form
#	Then I should see instructions in the input fields for email, password and confirm password
#
#Scenario: Handling incorrect login credentials
#	Given I have a registered user with the email "user@listlife.se" 
#	And the password "User123!"
#	When I try to log in with the email "user@user.com"
#	And log in with the incorrect password "UserUser123!
#	Then I should see an error message "Invalid login attempt."
#	And I should not be able to login

