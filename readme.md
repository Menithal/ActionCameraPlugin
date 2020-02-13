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


### Gestures

Most of the swapping of the camera directions are done by pointing forwards, while you turn your head.
Your Head velocity (as of now).

Passively this works best with games where you are doing alot of aiming, dodging or ducking (PistolWhip / H3 / Space Pirate)
into cover than games where you can mostly stand still (like BeatSaber / Audica). 


- If you Angle your controllers forward and turn your head left or right** directs the camera to go *over your shoulder.*

Looking around corners will always move the camera around to that shoulder towards where you are looking at, allowing your spectators to see
what you will see before you do. 


- If you **Angle the controllers slightly down before you look down**, your gesture direct to
show something on your body, *view will swap to either FPS or a front side view.*


- If you **Angle the controllers slighly up before looking up**, you direct the camera to show an up comming action OR the surroundings:
*view will swap to either a front side view, or rarely, a tactical perspective from top down (default disabled).*

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
            "removeAvatarInsteadOfHead": true,
	    "actionSwapTime": 0.8f,
	    "bodyCameraSwapTime": 0.8f,
            "disableTopCamera": true,
            "disableFBTCamera": false,
            "disableFPSCamera": false,
            "reverseFBT": false,
            "reverseShoulder": false,
            "controlMovementThreshold": 4.0,
            "forwardVerticalOffset": 0.0,
            "forwardHorizontalOffset": 5.0,
            "forwardDistance": 10.0,
            "averageHandsWithHead": true,
            "useDominantHand": false,
            "rightHandDominant": true
        }
    }
}
[...]


### Configurables: 

`removeAvatarInsteadOfHead`: Instead of removing the avatar, when set to false, will remove only the head of the avatar, when in first person mode. defaults true.
`disableTopCamera`: If true, Disables Top Down Camera. defaults false.
`disableFBTCamera`: If true, Disables Full body Front-side Camera. defaults false.
`disableFPSCamera`: If true, Disables FPS Camera. defaults false.
`disableFPSCamera`: If true, Disables FPS Camera. defaults false.
`reverseFBT`: If true, inverses the side for full body camera. defaults false.
`reverseShoulder`: If true, inverses the side of the shoulder camera. defaults false.
`forwardVerticalOffset`: Defaults to 0, Adds vertical offset to gesture detection points. Increase value if you tend to have your headset higher than average.

#### Notice:

Most of the values are already tuned for the Offsets and movement treshold. Modify these at your own peril.

`controlMovementThreshold`: Numeric value that defines that relative radial velocity of the controllers / hmd before a gesture is accepted. 4.0 is default.
`forwardHorizontalOffset`: Offset for gesture detection points from center of your view.
`forwardDistance`: Forward distance from where the radial velocity is measured from

#### Future

These have not yet been set to be used, but will be related to any gestures with the controllers
`averageHandsWithHead`
`useDominantHand`
`rightHandDominant`

Additionally, there might be later some behavior chaining, so that transitions between different types of cameras would be smoother, but this is underworks.


### Developing and Building

Note if building using DEBUG, a textfile will be output to `%HOMEPATH%/Documents/Liv/Output` 
and written into with debug messages. If releasing, make sure DEBUG is not set.

