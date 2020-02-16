# Menithal's Dynamic Action Camera

This allows you LIV Avatars to express your self via some gesture and camera automation to avoid having to have a separate person or
a game pad strapped to control specifically the camera.

See License.

## Installation
Install by moving the ActionCamera.dll from Releases into your Liv Plugins directory at

`%HOMEPATH%/Documents/Liv/Plugins`

## Use
When in Liv, Set an Avatar Camera, and make sure to select Plugin > "Menithal' Action Camera" to start using the plugin.
Closing Liv now wil update your settings file.

### Available Cameras

- *OverShoulderAction* - Main feature of the plugin, Shows point of view over your shoulder. Looking around corners will always move the camera around to that shoulder towards where you are looking at, allowing your spectators to see
what you will see before you do.  You can reverse this with the `reverseShoulder` config.
- *FullBodyAction* - Front side view of your Liv Avatar. Turning your head moves the point of view similar to the shoulder view. You can reverse this with the `reverseShoulder` config. Can be turned off with `disableFBTCamera`.
- *FirstPerson* - FPS view of the game. Smoothened, and similar to how the game would play, but you can turn on avatar visibility with `removeAvatarInsteadOfHead`.  Can be turned off with `disableFBTCamera`.
- *Tactical* - Top down view of the game. Can be turned off with `disableFBTCamera`. Default off due to some bugs with liv.

### Controlling Cameras and Gestures

You direct the camera direction with head movement (for now) with your controllers behaving as keylocks You must be mostly pointing forwards with your controllers for commands to work.. 

Most of the swapping of the camera directions are done by pointing forwards (where your body is pointing towards), while you turn your head.
Your Head velocity (as of now).

Passively this works best with games where you are doing alot of aiming, dodging or ducking (PistolWhip / H3 )
into cover than games where you can mostly stand still (like BeatSaber): Audica works partially,
 but you have to really exaggerate your movements to get the camera to work in your favor, but its all about practice.


- If you Point your fingers forward (controller forward) and turn your head left or right You **direct the camera to go *over your shoulders.*


- If you **Angle the controllers slightly down before you look down**, your gesture direct to
show something on your body, *view will swap to either FPS or Dynamic Full body camera*


- If you **Angle the controllers slighly up before looking up**, you direct the camera to show an up comming action OR the surroundings:
*view will swap to either a Dynamic Full body camera, or rarely, a tactical perspective from top down (default disabled).*

When in a front side view, or a shoulder view, turning your head alternates between sides.

Please note that having a visible viewport in your hud may be considered cheating as technically over the shoulder / backwards camera 
gives you a small edge in some multiplayer games that support LIV. The intention of this is for streaming; not for being a naughty cheater.


## Configuring

Configurable after setting as a plugin for a camera, and closing Liv Composer: You can find the settings at
`%LOCALAPPDATA%/Liv/App/<LivVersion>.json`

## Default Setting Example: 
```
[...]
"pluginCameraBehaviourSettings": {
    "selectedPluginCameraBehaviourID": "ActionCamera",
    "pluginSettings": {
        "ActionCamera": {
	    "generalCameraSwapClamp": 8,
	    "actionCameraSwapClamp": 0.8,
	    "shoulderCameraPositioningTime": 0.9f,
	    "bodyCameraPositioningTime": 0.9f,
            "actionCameraDistance": 1.4f,

            "reverseFBT": false,
            "reverseShoulder": false,
            "controlMovementThreshold": 4.0,

            "forwardVerticalOffset": 0.0,
            "forwardHorizontalOffset": 5.0,
            "forwardDistance": 10.0,
            "removeAvatarInsteadOfHead": true,
            "disableTopCamera": true,
            "disableFBTCamera": false,
            "disableFPSCamera": false,

            "averageHandsWithHead": true,
            "useDominantHand": false,
            "rightHandDominant": true,
            "inBetweenCameraEnabled": false
        }
    }
}
[...]
```

### Configurables: 

- `generalCameraSwapClamp`: Time in seconds, before changing cameras again.
- `actionCameraSwapClamp`: Time in seconds, before changing camera sides
- `shoulderCameraPositioningTime`: Time in seconds how long the camera takes to move to the new side when over shoulder
- `bodyCameraPositioningTime`: Time in seconds how long the camera takes to move to the new side when Showing the Body.
- `actionCameraDistance`: In Meters how far back or front the camera should be from the avatar. 
- `inBetweenCameraEnabled`: If true, enables experimental inbetween cameras when swapping direction in an Action Camera (shoulder or body). Tries to keep an arc.

- `reverseFBT`: If true, inverses the side for full body camera. defaults false.
- `reverseShoulder`: If true, inverses the side of the shoulder camera. defaults false.
- `removeAvatarInsteadOfHead`: Instead of removing the avatar, when set to false, will remove only the head of the avatar, when in first person mode. defaults true.
- `disableTopCamera`: If true, Disables Top Down Camera. defaults false.
- `disableFBTCamera`: If true, Disables Full body Front-side Camera. defaults false.
- `disableFPSCamera`: If true, Disables FPS Camera. defaults false.
- `disableFPSCamera`: If true, Disables FPS Camera. defaults false.
- `forwardVerticalOffset`: Defaults to 0, Adds vertical offset to gesture detection points. Increase value if you tend to have your pointing higher or lower than normal when looking at first person view.

#### Notice:

Most of the values are already tuned for the Offsets and movement treshold. Modify these at your own peril.

`controlMovementThreshold`: Numeric value that defines that relative radial velocity of the controllers / hmd before a gesture is accepted. 4.0 is default.
`forwardHorizontalOffset`: Offset for gesture detection points from center of your view.
`forwardDistance`: Forward distance from where the radial velocity is measured from

#### Not yet in use

These have not yet been set to be used, but will be related to any gestures with the controllers
`averageHandsWithHead`
`useDominantHand`
`rightHandDominant`

Additionally, there might be later some behavior chaining, so that transitions between different types of cameras would be smoother, but this is underworks.


### Contributing, Developing and Building

Note if building using DEBUG, a textfile will be output to `%HOMEPATH%/Documents/Liv/Output` 
and written into with debug messages. If releasing, make sure DEBUG is not set. 
When building a release, make sure NOT to have the DEBUG flag set, otherwise the debug file will be filled to brim.

