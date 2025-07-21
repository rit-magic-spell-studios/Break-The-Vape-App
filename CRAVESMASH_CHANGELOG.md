# Crave Smash Changelog

## v0.5.4 - July 21st, 2025
* The player now needs to manually click to advance the tutorial
* The crave monster now has a random chance to be one of four possible monsters

## v0.5.3 - July 7th, 2025
* Player now gets between 10-15 points per click, depending on how fast they are clicking the crave monster
* The crave monster how takes 40 total clicks to win the game
* The crave monster will start to heal back health when the player has not clicked on it for a couple of seconds

## v0.5.2 - June 20th, 2025
* Player now gets 20 points per click, for a total of 400 points if they play through the entire game
* Added tutorial animation that plays before the game starts
	* Increased the length the tutorial screen at the start of the game to 5 seconds so the entire tutorial animation plays

## v0.5.1 - June 13th, 2025
* Points now add to a cumulative total points value that saves across all games
* Added total points label on win screen
* Fixed visual bugs with the Crave Monster that sometimes appear

## v0.5.0 - June 9th, 2025
* Fixed typo in tutorial text
* Crave Monster now takes 20 hits instead of 10
* Player now scores 10 points every time they tap the Crave Monster
* Updated the win screen with new layout and buttons
* Updated all UI screens with transitions
* Added a delay between when the Crave Monster gets defeated and the win screen to prevent accidental button presses

## v0.4.0 - June 6th, 2025
* Added first pass of UI implementation to the game
  * Tutorial screen, pause screen, win screen, and game screen
* Added first pass of art implementation for the Crave Monster
  * 3 stages of the Crave Monster based on its health as you tap it
* General code optimizations and improvements

## v0.3.0 - June 4th, 2025
* Removed form at the start of the game
  * This will instead be put on the main app home page
* Moved "Main Menu" button to a different location so you don't accidentally tap it when clicking the monster
* Monster now decreases in size with each tap instead of a health bar being displayed
  * The game is now won once the monster is no longer visible on the screen

## v0.2.0 - June 2nd, 2025
* Remade UI to use the UI Toolkit instead of Unity UI

## v0.1.0 - May 29th, 2025
* Set up Unity project (2022.3.46f1)
* Created basic Crave Smash graybox prototype
  * Basic form function to collect user data
  * Monster is clickable and allows the player to do damage to it