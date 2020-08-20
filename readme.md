# Menithal's Dynamic Action Camera

This is a camera plugin for [LIV ](https://liv.tv/) Avatars,
which allows you to have a more dynamic, varied camera for your audience.

See [LICENSE](LICENSE.txt)

## Table of Contents
- [Installation](#installation)
- [How To Use](#how-to-use)
  * [Available Cameras](#available-cameras)
  * [Controlling Cameras and Command Gestures](#controlling-cameras-and-command-gestures)
- [Configuring](#configuring)
  * [Configurables:](#configurables-)
- [Profile Specific Configuration](#profile-specific-configuration)
  * [Liv Settings](#liv-settings)
- [Contributing, Developing and Building](#contributing--developing-and-building)
- [Bug Reports](#bug-reports)

<small><i><a href='http://ecotrust-canada.github.io/markdown-toc/'>Table of contents generated with markdown-toc</a></i></small>

## Installation

Make sure you are running latest LIV build (2.0.0+)
Install by moving the ActionCamera.dll from Releases into your Liv Plugins CameraBehaviours directory at

`%HOMEPATH%/Documents/Liv/Plugins/CameraBehaviours/`

After putting it in the directory, make sure to check that the dll is not blocked by right clicking it, 
properties, and checking at the bottom of the window and making sure its not being blocked. An option to unblock is will be visible otherwise.

## How To Use

in VR with the LIV compositor active, to Set an Avatar Camera, and make sure to select Plugin > "Menithal' Action Camera" to start using the plugin.

### Available Cameras

- *OverShoulderAction* - Main feature of the plugin, Shows point of view over your shoulder. Looking around corners will always move the camera around to that shoulder towards where you are looking at, allowing your spectators to see
what you will see before you do.  You can reverse this with the `reverseShoulder` config.
- *FullBodyAction* - Front side view of your Liv Avatar. Turning your head moves the point of view similar to the shoulder view. You can reverse this with the `reverseShoulder` config. Can be turned off with `disableFBTCamera`.
- *FirstPerson* - FPS view of the game. Smoothened, and similar to how the game would play, but you can turn on avatar visibility with `removeAvatarInsteadOfHead`.  Can be turned off with `disableFPSCamera`.
- *AimDownSights* - Down Sights view of the game when holding a weapon two handed, and looking down the sights. Shows avatar if 
`removeAvatarInsteadOfHead` is disabled.  Can be turned off with `disableGunCamera`. Occurs only when in First Person
- *Tactical* - Top down view of the game. Can be turned off with `disableFBTCamera`. Default off due to some bugs with liv.

### Controlling Cameras and Command Gestures

You direct the camera direction with head movement (for now) with your controllers behaving as keylocks You must be mostly pointing forwards with your controllers for commands to work.. 

Most of the swapping of the camera directions are done by pointing forwards (where your body is pointing towards), while you turn your head.
Your Head velocity (as of now) controls the camera after.

Passively this works best with games where you are doing alot of aiming, dodging or ducking (PistolWhip / H3VR )
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

When you first start using the plugin, a configuration file is created at

`%HOMEPATH%/Documents/Liv/Plugins/MACPluginDefault.config`

You can find example configurations (Default, Dedicated Action, Static Dancing, Static Dancing without Front) in the [Examples Directory](Examples/)

You can copy the values from the files and transplant them over the MACPluginDefault.config file

### Configurables: 

- `cameraSwapTimeLock`: Time in seconds, before changing cameras again.
- `cameraPositionTimeLock`:  Time in seconds, before changing camera positioning when in action mode
- `reverseFBT`: If `True`, inverses the side for full body camera. defaults false.
- `reverseShoulder`: If `True`, inverses the side of the shoulder camera. defaults false.
- `controlMovementThreshold`:  Numeric value that defines that relative radial velocity of the controllers / hmd before a gesture is accepted. 2.0 is default.
- `controlMovementVerticalThreshold`:  Numeric value that defines that relative radial velocity of the controllers / hmd before a gesture is accepted for up and down movements.. 4.0 is default.
- `removeAvatarInsteadOfHead`: Instead of removing the head of the avatar when in first person mode, when set to true, will remove the avatar. defaults `True`.
- `disableTopCamera`: If `True`, Disables Top Down Camera. defaults true.
- `disableFBTCamera`: If `True`, Disables Full body Front-side Camera. defaults false.
- `disableFPSCamera`:  If `True`, Disables FPS Camera. defaults false.
- `FPSCameraOverride`: If `True`, Fps camera is always prioritized over any other camera.
- `linearCameraMovement`: by default  `False`. if `True`,  camera will move in a Linear Manner between camera sides without trying to orbit. (unless it gets too close avatar during transitions)
- `cameraVerticalLock`: If `True`, locks the camera look at positioning to be between the user head and waist,       
- `cameraShoulderPositioningTime`: Time in seconds how long the camera takes to move to the new side when over shoulder
- `cameraShoulderDistance`: Distance from the avatar the Shoulder Camera should be at
- `cameraShoulderAngle`: The Angle (in Degrees) the camera will be behind the avatar/
- `cameraBodySensitivity`: How fast to detect a turn to swap sides. Smaller the value, the more sensitive.
- `cameraBodyVerticalTargetOffset`: Adjust Body camera position up and down.
- `cameraBodyPositioningTime`: Time in seconds how long the camera takes to move to the new side when showing the front
- `cameraBodyLookAtForward`: The Look at target where the camera looks when showing the front in the relative Z.
- `cameraBodyDistance`:  Distance from the avatar the Body Camera should be at
- `cameraBodyAngle`: The Angle (in Degrees) the camera will be infront of the avatar
- `cameraBodySensitivity`: How fast to detect a turn to swap sides. Smaller the value, the more sensitive.
- `disableGunCamera`: Disable Gun / Scope Camera
- `cameraGunHeadAlignAngleTrigger`: the amount of degrees your head must be aligned with the path your weapons to count as looking down the sights. by default it is 25 degrees
- `cameraGunHeadDistanceTrigger`: Distance at which the gun camera starts to be triggered.
- `cameraGunEyeVerticalOffset`:  the amount of vertical offset from your dominant eye to down the ironsight raycast.
- `cameraGunMaxTwoHandedDistance`: Max Distance between hands when still counting as using the gun camera.
- `cameraGunMinTwoHandedDistance`: Minimum Distance between hands when still counting as using the gun camera.
- `cameraGunSmoothing`: Time it takes to get to new point
- `rightHandDominant` Sets either your right hand dominant or left hand dominant. This effects which eye and hand is used to measure two handness. If right handed, the right hand must be close to the right eye, if left handed, vice versa. by default assumes righthandness
- `alwaysHaveAvatarInFrame`: by default `True`. When camera is transitioning between camera positions, try to keep avatar in frame.
- `cameraBodyUseRoomOriginCenter`: by default `False`. if `True` Body camera will Use the center of the room as the anchor point instead of moving and rotating avatar, pointing towards the roomspace forward
- `cameraShoulderUseRoomOriginCenter`: by default `False`. if `True` Shoulder camera will Use the center of the room as the anchor point, instead of the player head. Will however use the player height.
- `cameraShoulderFollowGaze`: by default `True`. if `False` Shoulder camera will look towards where the player is looking at, instead using the common roomspace forward.
- `minimumCameraDistance`: by default `0.2`. Determines the distance the camera will circle around the avatar when passing by, instead of phasing right through them.
- `linearCameraMovement`: by default  `False`. if `True`,  camera will move using circular paths between sides and avoid going in straight lines unless swapping from front/back/top/fps

By default the setting are configured forPS Combat (ala pistolwhip) But with enough configuration You can tune how it works for other types of games separatedly.


## Profile Specific Configuration

Sometimes you want to have different configurations per each liv profile / game you want to play, you will then have to modify the LIV settings to point to a separate configuration file.

You can find the profile settings on LIV at
`%LOCALAPPDATA%/Liv/App/<LivVersion>.json`

### Liv Settings

Find a line under the profile you want to edit and modify a `configurationName`

```
 "pluginCameraBehaviourSettings": {
        "selectedPluginCameraBehaviourID": "ActionCamera",
        "pluginSettings": {
            "ActionCamera": {
                "configurationName": "MACPluginDefault.config",
                "cameraDefaultFov": 80,
                "cameraGunFoV": 80
            }
        }
    }
```

In the above example `configurationName` is pointing to a file name of `MACPluginDefault.config` which it will look for in the `%HOMEPATH%/Documents/Liv/Plugins/` directory. You can also use this to modify any FoV values what you want to use.


## Contributing, Developing and Building

Note if building using DEBUG, a textfile will be output to `%HOMEPATH%/Documents/Liv/Output` 
and written into with debug messages. 

When building a release, make sure NOT to have the DEBUG flag set, otherwise the debug file will be filled to brim. We do not want to flood end users disks with logs.

## Bug Reports 

You can comments, suggestions, bug reports to me over Discord Malactus#3957 or just leave them as Github Issues
