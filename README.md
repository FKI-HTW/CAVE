# CAVE

CAVE is a Unity Package that can enable most Unity projects to run in the virtual environment CAVE (CAVE Automatic Virtual Environment).
The CAVE uses steoreoscopic projections surrounding the user to create a perspective view. This package uses the Windows Kinect SDK
to track the user with a Kinect v2 and Nintendo Switch JoyCons.

Download the package through the <strong>Window > Package Manager</strong> in the Unity Editor with <strong>Add package from git URL...</strong>.
Using the URL <strong>https://github.com/FKI-HTW/CAVE.git#upm</strong> for the newest version, or <strong>https://github.com/FKI-HTW/CAVE.git#VERSION_TAG</strong> for a specific one. The available version tags can be found in [Tags](https://github.com/FKI-HTW/CAVE/tags).

Through the Package Manager an optional sample can be downloaded which contains a prefab, which defines an example implementation and usage of a complete CAVE.

For more details on the use and development of Unity package projects see: [https://github.com/FKI-HTW/UnityPackageTemplate](https://github.com/FKI-HTW/UnityPackageTemplate)

## Troubleshooting
Here is a list of common issues with their fixes/workarounds:

Issue | Cause | Solution
--- | --- | ---
Glitchy view; Objects only seen on one eye | Default Skybox bug | Set skybox to 'Solid-Color' in all virtual cams
Tracking is offset; Bad positioning | Bad Kinect floor plane detection | Apply manual kinect position offset in `Virtual Environment` > `Kinect Tracker` > `Position` to around `(0, 0.47, 2.6)`
No tracking; Kinect on but no working | Unknown | Put hand very close before sensor for 2-3 seconds and retry
Jitter; Bad stereoskopic view | Kinect not detecting eyes due to 3d glasses | Wear glasses at nosetip and tilt head down slightly
Display missing; Not all in fullscreen | Unknown | Create a shortcut of executable and check `Launch in fullscreen` in file properties
JoyCons not connecting | Switching between applications | Remove & repair in bluetooth settings (pw=0000)
Displays swap around for no reason | Bug in Windows display settings? | Never open display settings and use calibrator to assign cameras
Bad depth perception | Misalligned IPD | Set correct eye seperation in virtual environment

## Helpful tools

CAVE calibrator with UI and joycon support:  
https://gitlab.rz.htw-berlin.de/cave/cave-standalone-calibrator