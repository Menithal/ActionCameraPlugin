

1.0.0

------

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
- Updated Camera Update method from Depricated value to new.
- Added New Configurable Values
    - `alwaysHaveAvatarInFrame`: by defualt `True`. When camera is transitioning between camera positions, try to keep avatar in frame.
    - `cameraBodyUseRoomOriginCenter`: by default `False`. if `True` Body camera will Use the center of the room as the anchor point instead of moving and rotating avatar, pointing towards the roomspace forward
    - `cameraShoulderUseRoomOriginCenter`: by default `False`. if `True` Shoulder camera will Use the center of the room as the anchor point, instead of the player head. Will however use the player height.
    - `cameraShoulderFollowGaze`: by default `True`. if `False` Shoulder camera will look towards where the player is looking at, instead using the common roomspace forward.
    - `minimumCameraDistance`: by default `0.2`. Determines the distance the camera will circle around the avatar when passing by, instead of phasing right through them.



- Brought `FPSPlugin` improvements to `ActionCamera`
- Brought `DancePlugin` Experiment improvements to `ActionCamera`