# Unity Career Development: advanced stealth player camera system

## Scenario
Starter project is provided with a 3D character from the adventure game, that is up on the Asset Store by Unity. The character uses the standard animations from Standard Assets, with a new extra featured added, whenever the character goes intro cover, it will spin around, creep along the cover and stop at edges, using the proper animations for each differnt state.

## Challange
Tweak the camera to have the standard move-to-look-down-halls behaviour that is seen in other stealth games like **Metal Gear Solid**, or **Final Fantasy VII**.
### Bonus Challenge
When the camera is in a near mode and looking in the -Z direction, the left and right arrows should move the Player in the opposite direction that they usually do. However, it's more subtle than that. If the camera is in far mode, and the player is inCover and holding left, when the camera switches directions, the controls should not switch directions until the player releases the key.

## Achievements
Implemented a sophisticated camera system to track the player in a near cover prespective
### Learning Objectives
1. Identify methods to implement inputs and controls.
1. Identify methods to implement camera views and movement.

## Demo
<br>
<img src="in-cover-stealth.gif" alt="show-off-incover">

## Disclaimer
All the code added/moddified present in the commits, it is entirely mine, exept of course, the first commit, which it's part of the Starter Project delivered as part of the course "Unity Certified Programmer Exam Preparation Specialization" by Coursera.