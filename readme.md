# Menithal's Dynamic Action Camera

This allows you LIV Avatars to express your self via some gesture and camera automation to avoid having to have a separate person or
a game pad strapped to control specifically the camera.

See License.

## Installation
Install by moving the ActionCamera.dll from Releases into your Liv Plugins directory at

`%HOMEPATH%/Documents/Liv/Plugins`

## Use
When in Liv, Set an Avatar Camera, and make sure to select Plugin > "Menithal' Action Camera" to start using the plugin.
Closing Liv now wil update your settings file. You can then create and configure multiple profiles with different configurations in Liv and modifying the json file. See Configuration for more detail


### Available Cameras

- *OverShoulderAction* - Main feature of the plugin, Shows point of view over your shoulder. Looking around corners will always move the camera around to that shoulder towards where you are looking at, allowing your spectators to see
what you will see before you do.  You can reverse this with the `reverseShoulder` config.
- *FullBodyAction* - Front side view of your Liv Avatar. Turning your head moves the point of view similar to the shoulder view. You can reverse this with the `reverseShoulder` config. Can be turned off with `disableFBTCamera`.
- *FirstPerson* - FPS view of the game. Smoothened, and similar to how the game would play, but you can turn on avatar visibility with `removeAvatarInsteadOfHead`.  Can be turned off with `disableFPSCamera`.
- *Tactical* - Top down view of the game. Can be turned off with `disableFBTCamera`. Default off due to some bugs with liv.

### Controlling Cameras and Gestures

You direct the camera direction with head movement (for now) with your controllers behaving as keylocks You must be mostly pointing forwards with your controllers for commands to work.. 

Most of the swapping of the camera directions are done by pointing forwards (where your body is pointing towards), while you turn your head.
Your Head velocity (as of now) controls the camera after.

Passively this works best with games where you are doing alot of aiming, dodging or ducking (PistolWhip / H3 )
into cover than games where you can mostly stand still (like BeatSaber):
It can work, in Beatsaber, but you have to really exaggerate your head movements to get the camera to work in your favor, but its all about practice.


- If you Point your fingers forward (controller forward) and turn your head left or right You **direct the camera to go over your shoulders.**

- If you **Angle the controllers slightly down before you look down**, your gesture direct to
show something on your body, *view will swap to either FPS or Dynamic Full body camera*


- If you **Angle the controllers slighly up before looking up**, you direct the camera to show an up comming action OR the surroundings:
*view will swap to either a Dynamic Full body camera, or rarely, a tactical perspective from top down (default disabled).*

When in a front side view, or a shoulder view, turning your head alternates between sides.

Please note that having a visible viewport in your hud may be considered cheating as technically second view port
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
            "cameraSwapTimeLock": 8,
            "cameraPositionTimeLock": 0.8f,
            "reverseFBT": false,
            "reverseShoulder": false,
            "controlMovementThreshold": 4,
            "forwardVerticalOffset": 0,
            "forwardHorizontalOffset": 5,
            "forwardDistance": 10,
            
            "removeAvatarInsteadOfHead": true,
            "disableTopCamera": true,
            "disableFBTCamera": false,
            "disableFPSCamera": true,
            "inBetweenCameraEnabled": true,
            "cameraVerticalLock": false,
            
            "cameraShoulderPositioningTime": 0.9,
            "cameraShoulderdistance": 1.4,
            "cameraShoulderAngle": 25,
            "cameraBodyPositioningTime": 2,
            "cameraBodyLookAtForward": 1,
            "cameraBodyDistance": 1.4,
            "cameraBodyAngle": 45,

            "averageHandsWithHead": true,
            "useDominantHand": false,
            "rightHandDominant": true
        }
    }
}
[...]
```

### Configurables: 

- `cameraSwapTimeLock`: Time in seconds, before changing cameras again.
- `cameraPositionTimeLock`:  Time in seconds, before changing camera positioning when in action mode
- `reverseFBT`: If true, inverses the side for full body camera. defaults false.
- `reverseShoulder`: If true, inverses the side of the shoulder camera. defaults false.
- `controlMovementThreshold`:  Numeric value that defines that relative radial velocity of the controllers / hmd before a gesture is accepted. 4.0 is default.
- `forwardVerticalOffset`: Defaults to 0, Adds vertical offset to gesture detection points. Increase value if you tend to have your pointing higher or lower than normal when looking at first person view.
- `forwardHorizontalOffset`:  Offset for gesture detection points from center of your view.
- `forwardDistance`:  Forward distance from where the radial velocity is measured from     
- `removeAvatarInsteadOfHead`: Instead of removing the head of the avatar when in first person mode, when set to true, will remove the avatar. defaults true.
- `disableTopCamera`: If true, Disables Top Down Camera. defaults true.
- `disableFBTCamera`: If true, Disables Full body Front-side Camera. defaults false.
- `disableFPSCamera`:  If true, Disables FPS Camera. defaults false.
- `inBetweenCameraEnabled`: If true, enables experimental inbetween cameras when swapping direction in an Action Camera (shoulder or body). Tries to keep an arc.
- `cameraVerticalLock`: If true, locks the camera look at positioning to be between the user head and waist,       
- `cameraShoulderPositioningTime`: Time in seconds how long the camera takes to move to the new side when over shoulder
- `cameraShoulderDistance`: Distance from the avatar the Shoulder Camera should be at
- `cameraShoulderAngle`: The Angle (in Degrees) the camera will be behind the avatar/
- `cameraBodyPositioningTime`: Time in seconds how long the camera takes to move to the new side when showing the front
- `cameraBodyLookAtForward`: The Look at target where the camera looks when showing the front in the relative Z.
- `cameraBodyDistance`:  Distance from the avatar the Body Camera should be at
- `cameraBodyAngle`: The Angle (in Degrees) the camera will be infront of the avatar


By default the setting are configured for CQC Pistol Combat (ala pistolwhip) but playing aroudn with the values allows you to create entries for other games, such as Beat Saber or Audica.

### Example "Dancing/Sabering" Profile
```
[...]
"pluginCameraBehaviourSettings": {
    "selectedPluginCameraBehaviourID": "ActionCamera",
    "pluginSettings": {
        "ActionCamera": {
	        "shoulderCameraPositioningTime": 3,
	        "bodyCameraPositioningTime": 3,
            "cameraSwapTimeLock": 20,
            "cameraPositionTimeLock": 6,
            "reverseFBT": false,
            "reverseShoulder": false,
            "controlMovementThreshold": 1,
            "forwardVerticalOffset": 0,
            "forwardHorizontalOffset": 5,
            "forwardDistance": 10,
            
            "removeAvatarInsteadOfHead": true,
            "disableTopCamera": true,
            "disableFBTCamera": false,
            "disableFPSCamera": true,
            "inBetweenCameraEnabled": false,
            "cameraVerticalLock": true,
            
            "cameraShoulderPositioningTime": 1.8,
            "cameraShoulderDistance": 2.6,
            "cameraShoulderAngle": 35,
            "cameraBodyPositioningTime": 4,
            "cameraBodyLookAtForward": 2,
            "cameraBodyDistance": 3,
            "cameraBodyAngle": 45,

            "averageHandsWithHead": true,
            "useDominantHand": false,
            "rightHandDominant": true
        }
    }
}
[...]
```



#### Not yet in use

These have not yet been set to be used, but will be related to any other gestures with the controllers
`averageHandsWithHead`
`useDominantHand`
`rightHandDominant`

Additionally, there might be later some behavior chaining, so that transitions between different types of cameras would be smoother, but this is underworks.


### Contributing, Developing and Building

Note if building using DEBUG, a textfile will be output to `%HOMEPATH%/Documents/Liv/Output` 
and written into with debug messages. 

When building a release, make sure NOT to have the DEBUG flag set, otherwise the debug file will be filled to brim. We do not want to flood end users disks with logs.

### Bug Reports 

You can comments, suggestions, bug reports to me over Discord Malactus#3957 or just leave them as Github Issues
