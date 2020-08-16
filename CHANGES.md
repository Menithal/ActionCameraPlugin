

# 1.0.0

------

- Now can be configured to work better with Dancing games.
- Updated Camera Update method for LIV 2.0.0. This does mean this is no longer compatible with the older versions of LIV.
- Refactored Configuration Method, now separate file is used for configuring than Liv settings.
    - New Config files can be found under the Liv/Plugin Folder next to the CameraBehavior Folder. Fov is still in JSON as its the very first thing to be loaded, and to avoid racing issue when opening apps that do not support FOV swapping on the go.
    - Liv Profile settings can still be used to point to another config.
        - ```
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
    - If the file doesnt exist, is simply creates a new file with the default config.
    - Supports # for commenting
    - Simpler key=value without having to know JSON Formating
    - Due to the new configuration method, there is a small delay when starting the plugin when its reading/writing configuration files. During this moment, the camera will be looking from the top down.

- Included Better examples of various configurations in readme. (Tutorial upcomming)
    
- Brought `DancePlugin` (Unreleased Experiment) improvements to `ActionCamera`
    - Support for static camera positioning via `cameraBodyUseRoomOriginCenter`, instead of avatar based position, to stop camera from moving with the avatar's head movements.
        - It will still bounce though.
    - Circular paths for moving between camera sides.
    - Swapping sides is now a global instead of a camera specific value.
        - Swapping sides can now occur in the middle of a camera swap or even if camera doesnt support it, and is all dependent on the player head movement.
    -`alwaysHaveAvatarInFrame` in frame configuration which makes sure the camera always has the avatar in frame.

- Brought `FPSPlugin` improvements to `ActionCamera`
    - Should now align better with sights on most weaponry. Uses eye position to determine angle (spectators see what you see)
    - `useEyePosition` for FPS view uses either left or right eye. uses `rightEyeDominant` to determine which
    - `rightEyeDominant` if `useEyePosition` is true, determines if uses right eye or left eye.
    
- Added New Configurable Values
    - `alwaysHaveAvatarInFrame`: by default `True`. When camera is transitioning between camera positions, try to keep avatar in frame.
    - `cameraBodyUseRoomOriginCenter`: by default `False`. if `True` Body camera will Use the center of the room as the anchor point instead of being always infront of the avatar, pointing towards the roomspace forward
    - `cameraShoulderUseRoomOriginCenter`: by default `False`. if `True` Shoulder camera will Use the center of the room as the anchor point, instead of the player head. Will however use the player height.
    - `cameraShoulderFollowGaze`: by default `True`. if `False` Shoulder camera will look towards where the player is looking at, instead using the common roomspace forward.
    - `minimumCameraDistance`: by default `0.2`. Determines the distance the camera will circle around the avatar when passing by, instead of phasing right through them.
    - `linearCameraMovement`: by default  `False`. if `True`,  camera will move in a Linear Manner between camera sides without trying to orbit. (unless it gets too close avatar during transitions)

- Removed Configurable Values
    - `forwardDistance`, `forwardHorizontalOffset`, `forwardVerticalOffset`, configurations have been removed as these effect other configuration values too (such as sensitivity, etc). They are now static.
    - `inBetweenCameraEnabled` is now replaced. Cameras now will follow a track if `linearCameraMovement` is false when swapping sides instead.
