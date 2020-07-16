Virtual Human Project (VHP) 0.1.0 beta version (16/07/2020)
Contact: Dr Geoffrey Gorisse, geoffrey.gorisse@gmail.com

The Virtual Human Project (VHP) provides developers with a scalable, easy to integrate and optimized package to procedurally animate realistic virtual humans in Unity.


# Toolkit features overview # 

 - Universal Unity blend shapes preset editor for multi-mesh blend shapes.
 - Universal blend shapes mapper scriptable object saving blend shapes data.
 - Blend shapes preset loading for procedural animation.
 - Procedurally controllable emotions (anger, disgust, fear, happiness, sadness and surprise).
 - Multiple gaze modes from static to probabilistic.
 - Realistic eye micro variations and blinking. 
 - Lip synchronisation based on realtime and pre-recorded tracks.
 - Dynamic prioritization and filtering of blend shapes modifications and transitions.
 - Demo scene with high quality characters and HD rendering presets. 


# Blend shapes preset editor #

 - Unity editor extension tool.
 - Automatic detection of the characters' skinned mesh renderers with blend shapes.
 - Scalable system to manage multi-mesh blend shapes.
 - Selectable facial expression to be edited.
 - Saving function (with data overwrite security) to store the blend shape values in the dedicated preset (scriptable object).
 - Loading function to edit existing presets.
 - Reset function to reinitialize the characters' blend shapes.

# Manager features #

 - Character blend shapes preset management.
 - Custom GUI editor to add procedural controllers (emotions, gaze, etc.)
 - Prioritization and filtering system for blend shapes modifications (emotions, gaze and lip synchronization).
 - Smooth interpolated and optimized (event based system) blend shapes transitions. 

# Emotions features #

 - Dynamic loading of the character emotions blend shape values.
 - Procedurally controllable human emotions (anger, disgust, fear, happiness, sadness, surprise).
 - Mutually exclusive blend shapes modifications based on emotions' intensity.

# Gaze features #

 - Dynamic loading of the character gaze blend shape values.
 - Configurable eye settings to match every character template.
 - Switchable eye orientation strategy based on four models:
  - Probabilistic: automatic probabilistic target ponderation based on distance, sound proximity and volume, and movement velocity. 
  - Random: gaze orientation modification with variable frequency and duration based on random positions in the field of view of the character.
  - Static: static gaze orientation matching the forward direction of the character.
  - Scripted: single scriptable target position. 
 - Agent mode enabling inverse kinematic animations with non linear smooth transitions.
 - Micro variations to add realistic eye movements over the aforementioned gaze targeting strategies.
 - Mutually exclusive blend shapes modifications based on gaze orientation.
 - Blinking with variable frequency blending with other blend shapes modifications.

 # Lip Synchronisation features #

 - Integration of the Oculus Audio SDK to process realtime/pre-recorded audio content to calculate visemes' intensity.
 - Visemes intensity processing and mapping based on characters' blend shapes preset. 
 - Realtime lip synchronization based on a microphone input.
 - Lip synchronisation based on pre-recorded audio tracks.

 # Probabilistic gaze configurator features #

 - Unity editor extension tool.
 - Automatic loading of the gaze target prefab.
 - Selection tool to quickly select potential gaze targets in the scene hierarchy. 
 - Add function to instantiate multiple gaze target prefabs (with duplication avoidance and automatic scaling) that will be processed in the probabilistic gaze model.
 - Remove function (with security) to delete all existing target prefabs in the scene.

# Demo scene content #

 - High quality male character (textures including detail map and subsurface scattering, fully rigged with blend shapes).
 - High quality female character (textures including detail map and subsurface scattering, fully rigged with blend shapes).
 - Demo manager to test the toolkit features. 
 - 3D targets (static, dynamic and sound).
 - HDRP sky and fog volume preset.
 - HDRP post process volume preset. 
 - 3 points lighting configuration.