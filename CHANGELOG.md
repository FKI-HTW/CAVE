# Changelog

## Version 1.1.0
* Added CAVETools project as sample

## Version 1.0.0

Requires: Unity 2022.3

API Changes:
* moved package from Gitlab to Github
* updated API and namespaces
* changed structure to Github Unity package structure
* added samples containing calibration logic

## Version 0.9.1-preview
Requires: Unity 2019.3

### Added Features
* Calibration: The projection and `VirtualCamera` components can be calibrated over the network via the new `VirtualCalibrator` which supports TCP connections.
* Calibration: Use the `VirtualCalibrationWindow` UI in the editor to connect to the `VirtualCalibrator` component.
* Rendering: The `VirtualProjection.ComputeHolographicProjectionMatrix` function is a simpler and faster way of creating a `Camera.projectionMatrix` with a holographic projection.
* Rendering: The `VirtualProjection.ComputeBimberMatrix` now computes a Bimber Matrix using an internal solver which replaces bindings to the OpenCV solver.
* Microsoft Kinect 2.0: The `KinectActor` is now a seperate component which represents a single tracked person. 
* Microsoft Kinect 2.0: The new `KinectTracker` can track multiple `KinectActor`'s. 
* Microsoft Kinect 2.0: The `KinectSkeleton` allows a simple creation and tracking of joints on a `KinectActor`.
* Microsoft Kinect 2.0: Positions and rotations can now be filtered via the `OneEuroFilter`.

### Improvements
* Rendering: The output target of `VirtualCamera` components can now be changed at runtime using `VirtualEnvironment.SetOutputTarget`.
* Microsoft Kinect 2.0: Reduced GC pressure and computation time of joint positions and rotations.

### API Changes
* Rendering: The `ProjectorBrain` is replaced by the `VirtualEnvironment` which can now be calibrated via the new `VirtualCalibrator` component.
* Rendering: `VirtualCamera` replaces the old `ProjectorCamera`.
* Microsoft Kinect 2.0: `KinectActor` is no longer responsible for updating joint `GameObject`'s.
* Microsoft Kinect 2.0: `KinectTrackableObject` and `KinectTrackable` components are now merged to `KinectSkeletonJoint`.
* Microsoft Kinect 2.0: `KinectPlayArea` is replaced by the `KinectTracker`. 

### Fixes
* Microsoft Kinect 2.0: Fixed calculation of floor tilt and joint rotations with `KinectHelper.CalculateFloorRotationCorrection`.
* Microsoft Kinect 2.0: Fixed performance bugs in various `Windows.Kinect.*` API's.
* Microsoft Kinect 2.0: In Unity versions higher than 2019.3 building the standalone puts the Kinect plugin libraries in the wrong subfolder. The fix scans the build directory and moves the libraries to the correct folder.

### Removed
* `HTW.CAVE.Joycons` namespace with all components and the `JoyconLib`. Use the new Unity Input System instead.
* OpenCV bindings and libraries.
* Standard controls and interactions in the `HTW.CAVE.Controls` namespace.

## Version 0.5.2-preview
Requires: Unity 2019.1

### Added Features
* Controls: The `TeleporterControls` can be used to teleport to save locations nearby.
* Nintendo Joy-Cons: You are able to configure different input schemes with the `JoyconBinding` class.
* Speech to Text: The new namespace `SpeechToText` is designed to simplify the access of the built-in Windows Speech Recognition (WSR) feature.
* Controls: The new `FreeLookControls` component allows you to navigate in a scene freely.
* Legacy Components: Added support for old equalization files based on a Bimber matrix.

### Improvements
* Microsoft Kinect 2.0: The `KinectPlayArea` acquires new data automatically at a specified frame rate (30 fps) to improve performance.
* Microsoft Kinect 2.0: Reworked the API structure and introduced a `KinectTrackableObject` which can be configured to automatically update itself with tracking data.
* Microsoft Kinect 2.0: The `KinectTrackableHead` is behaves like the `KinectTrackableObject` but implements some fallbacks for the head tracking capabilities.
* Microsoft Kinect 2.0: The `KinectTrackableHand` is behaves like the `KinectTrackableObject` but is lighter and provides access to the hand state.
* Nintendo Joy-Cons: Rework of the `JoyconHelper` which is now called `JoyconController` with simpler API and caching support.
* Nintendo Joy-Cons: The new `JoyconInput` API takes care of mouse and keyboard axis input in combination with the Joy-Con stick input.
* Automatic Build: The virtual reality SDK can be installed automatically.
* Automatic Build: Exclusive full screen can now be set automatically.

### API Changes
* Editor: `KinectAddin` functionalities are now located in the namespace `HTW.CAVE.Kinect.Addin`.
* Editor: Restructuring of the `KinectAddinHelper` which is now responsible for moving the `Plugins` folder via `MovePluginsToAssets` or `MovePluginsToPackage`.
* Math: `HolographicFastPrecompute` is now called `HolographicPrecompute`.
* Math: `HolographicFast` is now part of the `Holographic` method family.
* Controls: The `Controller` namespace is now called `Controls`.

### Fixes
* Microsoft Kinect 2.0: Fixed an issue where the `Plugins` folder fails to be moved before building.
* Nintendo Joy-Cons: Fixed the issue that Joy-Con controller and keyboard input blocking each other (see `JoyconInput`).
