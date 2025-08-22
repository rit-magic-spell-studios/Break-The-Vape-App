# Puff Dodge Changelog

## v0.2.0 - Unreleased
* Added puff particle effect when a vape item is destroyed
* Added points popups when destroying a vape item
* The player now gets 30 points per destroyed vape item
* The player now gets double points if they destroy a vape item close to the lung character
* Vape items will become 5 points less valuable each time the lung character is hit
	* The minimum possible value of a vape item is 5 points (if the lung character gets hit 5 times)
* The slice is now more forgiving about its positioning, making it easier to slice vape items
* Added sound effects
* Added confetti particle effect when winning the game

## v0.1.1 - August 18th, 2025
* Replaced placeholder assets with new assets
* Added improved win screen with main menu header and check in feature

## v0.1.0 - July 27th, 2025
* Added basic version of gameplay
  * Player uses their finger to swipe across the screen and clear vape items that launch in from the sides of the screen
  * The player scores 50 points per vape item destroyed
  * If a vape item hits the lung character at the bottom of the screen, the lungs appear damaged
  * The lung character will regain health overtime as long as no more vape items hit them
  * The player needs to clear 25 vape items in order to win the game