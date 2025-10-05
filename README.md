# Virtual Human Project (VHP) - Version 0.2.1 (Beta)
*05/10/2025*  
*Contact: Geoffrey Gorisse, PhD, [geoffrey.gorisse@gmail.com](mailto:geoffrey.gorisse@gmail.com)*

The Virtual Human Project (VHP) offers developers an optimized, scalable, and easy-to-integrate package to procedurally animate realistic virtual humans in Unity.

---

## Toolkit Features Overview

- **Blend Shapes Preset Editor**: Universal Unity editor extension for multi-mesh blend shapes.
- **Blend Shapes Mapper**: Scriptable object to save blend shapes presets.
- **Procedural Emotions**: Controls for emotions such as anger, disgust, fear, happiness, sadness, and surprise.
- **Gaze Modes**: Offers multiple gaze behaviors, including static, random, scripted and probabilistic.
- **Eye Micro-saccades & Blinking**: Realistic eye movements and blinking features.
- **Lip Synchronization**: Supports real-time lip-sync based on microphone input, as well as pre-recorded audio tracks.
- **Dynamic Blend Shape Transition**: Smooth transitions using event-based system for optimized performance.
- **Demo Scene**: High-quality characters and HD rendering presets for showcasing features.

**Watch the videos**:\
[Virtual Human Project | Procedural Emotions, Gaze and Lip Sync](https://youtu.be/mstLuzTw790?si=5IOlBIR9mrzxmKuZ)\
[Virtual Human Project | Lip Sync Update](https://youtu.be/48T4lnm_Kqs?si=9hNPWfa4Gsfyyrou)

---

## Blend Shapes Preset Editor

- **Unity Editor Extension**: Tool for managing and editing blend shapes presets in the Unity Editor.
- **Automatic Retrieval of Multi-Mesh Blend Shapes**: Detects skinned mesh renderers with blend shapes.
- **Facial Expression Editing**: Select and modify various facial expressions.
- **Preset Saving**: Store blend shape values in scriptable objects with overwrite protection.
- **Preset Loading**: Load and edit previously saved presets.
- **Reset Function**: Reinitialize characters' blend shapes to their default state.

---

## Manager Features

- **Blend Shapes Preset Management**: Easily manage and apply blend shape presets for characters.
- **Custom GUI Editor**: Add and configure procedural controllers (emotions, gaze and lip sync).
- **Prioritization & Filtering**: Manage overlapping blend shape modifications with priority handling for emotions, gaze, and lip synchronization.
- **Smooth Transitions**: Event-based system for smooth, optimized blend shape transitions.

---

## Emotions Features

- **Dynamic Emotion Loading**: Load and manage blend shapes for various emotions.
- **Procedural Emotion Control**: Manage emotions such as anger, disgust, fear, happiness, sadness, and surprise.
- **Mutually Exclusive Modifications**: Emotion-based blend shape changes that don’t conflict with each other.

---

## Gaze Features

- **Dynamic Gaze Blend Shapes**: Load and adjust blend shapes based on gaze direction.
- **Configurable Eye Settings**: Adjust eye behaviors to fit different character templates.
- **Gaze Orientation Strategies**: Different models to control gaze behavior.
  - **Static**: Fixed gaze direction based on character's forward axis.
  - **Random**: Randomized gaze orientation with variable frequency.
  - **Scripted**: Gaze focused on a predefined target position.  
  - **Probabilistic**: Gaze statistical ponderation based on distance, sound proximity and volume, and movement velocity. 
- **Inverse Kinematics (IK)**: Agent mode to control upper body movements with IK and non linear smooth transitions.
- **Micro-saccades**: Add subtle, realistic eye movements.
- **Blinking**: Adjustable blinking frequency, blending with other blend shapes.

---

## Probabilistic Gaze Configurator Features

- **Editor Tool**: Unity editor extension for probabilistic gaze behavior.
- **Target Selection Tool**: Quickly select potential gaze targets within the scene.
- **Instantiating Multiple Targets**: Instantiate multiple targets, avoiding duplication, and automatically scale them in the scene.
- **Target Removal**: Safely remove existing target prefabs from the scene.

---

## Lip Synchronization Features

- **Oculus Audio SDK Integration**: Real-time lip sync based on microphone input and pre-recorded audio tracks.
- **Viseme Intensity Processing**: Maps viseme intensities to corresponding blend shapes.
- **Real-Time Lip Sync**: Synch lip movements in real time with microphone input.
- **Pre-recorded Lip Sync**: Sync lip movements with pre-recorded audio clips.

---

## Demo Scene Content

- **High-Quality Male & Female Characters**: Fully rigged characters from Character Creator with high-quality textures including detail map and subsurface scattering.
- **Demo Manager**: A demo manager script to test and visualize the toolkit’s features in the provided scene.
- **3D Gaze Targets**: Includes static, dynamic, and sound-based targets for testing gaze behaviors.
- **HDRP Presets**: Sky, fog, and post-processing volume presets optimized for high-quality rendering.
- **Lighting**: 3-point lighting configuration for the demo scene.

---

## Installation & Setup

1. **Clone the repository** or **download the latest release** from GitHub.
2. Open the project in **Unity** (recommended Unity version: 2020.3 or higher).
4. **Follow the setup guide** in the documentation to get started with the demo scene and begin using the VHP features.

---

## License

This project is licensed under the **GNU GPLv3**. For commercial projects requiring a proprietary license or specific usage conditions, **dual licensing** is available.  
For inquiries regarding a commercial license, please contact **Geoffrey Gorisse, PhD** at [geoffrey.gorisse@gmail.com](mailto:geoffrey.gorisse@gmail.com).  
