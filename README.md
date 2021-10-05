# Weirding Haptics

## Paper

**Weirding Haptics: In-Situ Prototyping of Vibrotactile Feedback in Virtual Reality through Vocalization**

**Authors:** Donald Degraen, Bruno Fruchard, Frederik Smolders, Emmanouil Potetsianakis, Seref Güngör, Antonio Krüger, Jürgen Steimle

**Links:**
- [DOI](https://doi.org/10.1145/3472749.3474797)

**Abstract:**
Effective haptic feedback in virtual reality (VR) is an essential element for creating convincing immersive experiences. To design such feedback, state-of-the-art VR setups provide APIs for programmatically generating controller vibration patterns. While tools for designing vibrotactile feedback keep evolving, they often require expert knowledge and rarely support direct manipulation methods for mapping feedback to user interactions within the VR environment. To address these challenges, we contribute a novel concept called Weirding Haptics, that supports fast-prototyping by leveraging the user's voice to design such feedback while manipulating virtual objects in-situ. Through a pilot study (N = 9) focusing on how tactile experiences are vocalized during object manipulation, we identify spatio-temporal mappings and supporting features needed to produce intended vocalizations. Based Weirding Haptics, we built a VR design tool informed by the results of the pilot study. This tool enables users to design tactile experiences using their voice while manipulating objects, provides a set of modifiers for fine-tuning the created experiences in VR, and allows to rapidly compare various experiences by feeling them. Results from an validation study (N = 8) show that novice hapticians can vocalize experiences and fine-tune their designs with the fine-tuning modifiers to match their intentions. We conclude our work by discussing uncovered design implications for direct manipulation and vocalization of vibrotactile feedback in immersive virtual environments.

## Usage

### Requirements

This is built in Unity using the SteamVR framework.

Minimum requirements:
- Unity (2020.3.9f1 LTS)
- SteamVR

Hardware:
- Oculus Rift/HTC Vive
- Microphone

### Overview

Based on the components of each weirding haptics object, you can define vibrotactile feedback on touch or grab actions. See the details below for the required components.

The HapticManager gameobject takes care of all recording and menu activities. The haptic data is placed into the StreamingAssets folder under haptic data. The UID of the UidComponent on the gameobject will define the subfolder. The connection between the recorded audio and the virtual interaction is defined in the json file of the same name.

Audio Calibration is placed under 'StreamingAssets/audioCalibration'. You can change this file based on your vocal preferences.

For positional haptics, during the study we used the [Rubber Band library](https://breakfastquay.com/rubberband/). This code was excluded from the current repository to avoid potential licensing conflicts.

### Weirding Objects

To enable a virtual object for haptic design, provide them with the following components:
- UidComponent: generate a unique UID
- PGVRInteractableObject: to allow for interaction
- Haptic Interactable: to allow for haptics on interaction

To enable haptics on grab:
- PGVRInteractableObject_Grabbable

To enable haptics on touch:
- PGVRTouchable

To enable positional haptics:
- PGVRInteractableObject_Grabbable
- PGVRLinearConstraint

If you want to highlight the object when pointing at it:
- PGVRInteractableObject_Selectable
- PGVRInteractableObjectHighlight_CopyMesh

If you want to define the hand pose when grabbing the object:
- PGVRInteractableObject_GrabPose_SteamVR
- SteamVR_Skeleton_Poser

## Citation

Please use the following bibtex entry:

```
@inproceedings{degraenfruchard2021weirdinghaptics,
  author={Degraen, Donald and Fruchard, Bruno and Smolders, Frederik and Potetsianakis, Emmanouil and G\"{u}ng\"{o}r, Seref and Kr\"{u}ger, Antonio and Steimle, J\"{u}rgen},
  title={Weirding Haptics: In-Situ Prototyping of Vibrotactile Feedback in Virtual Reality through Vocalization},
  year={2021},
  isbn = {},
  publisher = {Association for Computing Machinery},
  address = {New York, NY, USA},
  url = {https://doi.org/10.1145/3472749.3474797},
  doi = {10.1145/3472749.3474797},
  booktitle = {Proceedings of the 34nd Annual ACM Symposium on User Interface Software and Technology},
  pages = {},
  numpages = {},
  keywords = {haptic design, vibrotactile feedback, virtual reality, design tool, voice input, vocalization, direct manipulation},
  location = {Virtual, Online},
  series = {UIST '21}
}
```

## License

(c) Copyright 2021 Donald Degraen (donald.degraen-at-gmail.com) and Bruno Fruchard (contact-at-brunofruchard.com).

This software is made available under the terms of the [GNU Affero General Public License](https://www.gnu.org/licenses/agpl-3.0.html) as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.