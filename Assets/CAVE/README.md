# HTW CAVE Unity Package

Framework for creating CAVE Automatic Virtual Environments with Kinect v2 support.

⚠️ A backup of the previous repo structure can be found in branch `master-backup`.

## Quickstart
Open or create a Unity project with version 2020.3.21f1.
Click on `Window` > `Package Manager` > `+` > `Add package from disk...`
and install the cave package by selecting this repository.

To clone the repository type:
```
git clone https://gitlab.rz.htw-berlin.de/cave/cave.git
```

Click `Window` > `Virtual Calibration` to open the inbuild calibrator.
To enable CAVE support for your project, simply create a new `Virtual Environment` in the scene hierarchy.

ℹ️ In the current HTW-CAVE setup, you can safely delete the automatically generated `Virtual Camera`s for Back R/L and Top R/L to make calibration easier.

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
  
Ready to use test project / tools:  
https://gitlab.rz.htw-berlin.de/cave/cave-tools


## Github Mirror
An (outdated) mirror of this repo can be found at:
https://github.com/dn9090/de.htw.cave

Date: 29.09.2022
