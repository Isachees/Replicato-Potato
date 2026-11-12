
# Replicato Potato

A VR game where you try to replicate a face shown on a screen into a potato. This game is inspired by Mr. Potato, where you have to pick up facial parts such as eyes, noses and mouths and put them into a potato to create a head. In this game, you do exactly that, but in VR.

## Authors

- [Asier Ulloa García-Obledo](https://github.com/AsiGamer29)
- [Aniol López Ortega](https://github.com/Aniolobolo)
- [Isaac Ramírez Prieto](https://github.com/Bekun67)
- [Clara Rodríguez Moreno](https://github.com/Kopeke4)
- [Xavier Chaparro Foyo](https://github.com/XaviFast05)
- [Martí Mach Aymerich](https://github.com/0psycada)


## The game

### Face pieces

In this game, there are multiple face pieces that can be attached to the potato to create faces with them. In this case, we have:
- 3 Different eyes
- 3 Different noses
- 3 Different mouths
- 3 Different ears

These pieces can be thrown and placed in whatever position and rotation into the potato, as they snap their position when they collide with the potato.

### Potato

There is a potato in a screen by the wall. This potato has face pieces on it, and the player's job is to replicate the potato on the screen by placing the same face pieces into the potato in the center of the room.

There are three buttons on the ground, these buttons do the following:
- P button: Reset the potato shown on the screen. This will change the face pieces it has.
- R button: Reset the face pieces of your potato. This will launch all face pieces the potato has.
- O button: Validate your potato. This will check if your potato has the same face pieces as the potato on the screen.

### Winning and losing

When the O button is pressed, if your potato has the same face pieces as the potato on the screen, confetti will appear, and the player will win. If the potato doesn't match, all the face pieces will be launched and your potato will now be empty, essentially counting as a loss.


## Features

### Image Tracking

This game uses image tracking to change the player's projectiles to a type that is strong against the enemy that they want to defeat. The game detects the image, and according to the type it's assigned to, it spawns a particle and changes the projectile to that type.

### Occlusion

This game uses occlusion to shoot projectiles with physics. These projectiles have gravity and detect surfaces and enemies, making them defeat enemies if their type is strong against the enemy's type.

### Plane Tracking

This game uses plane tracking to spawn enemies to avoid spawning enemies in wrong places or have weird perspectives. This works by detecting the space that the camera shows in real time. It doesn't work perfectly, but the enemies spawn in places where the player would expect them to do.