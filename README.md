# Unity client for Open Spatial Computing Platform

This is a Unity client for [Open AR Cloud](https://www.openarcloud.org/)'s Open Spatial Computing Platform.

The app was originally created in 2021 by [Augmented City](https://augmented.city/) based on their AC Viewer app.
It was extended by [3DInteractive](https://3dinteractive.se/en/) and [Nokia](https://www.nokia.com/) in 2022 in a [project](https://medium.com/openarcloud/our-projects-and-achievements-in-2022-5baaa541cce1) funded by [NGI Atlantic](https://ngiatlantic.eu/funded-experiments/deployment-and-evaluation-5g-open-spatial-computing-platform-dense-urban).
In 2025, Unity upgrade and refactoring was done by [Nokia](https://www.nokia.com/) in a project funded by [NGI Search](https://www.ngisearch.eu/view/Events/OC5Searchers).

References:
```
@INPROCEEDINGS{10740111,
  author={Sörös, Gábor and Jackson, James and Vogt, Michael and Salazar, Mikel and Kadlubsky, Alina and Vinje, Jan-Erik},
  booktitle={2024 IEEE International Conference on Metaverse Computing, Networking, and Applications (MetaCom)},
  title={An Open Spatial Computing Platform},
  year={2024},
  volume={},
  number={},
  pages={239-246},
  keywords={Location awareness;Visualization;Cloud computing;Protocols;Metaverse;Collaboration;Cameras;User experience;Spatial computing;Web sites;Mixed/augmented reality;Ubiquitous and mobile computing systems and tools;Location based services},
  doi={10.1109/MetaCom62920.2024.00046}
}
```

```
@INPROCEEDINGS{9974229,
  author={Sörös, Gábor and Nilsson, John and Wu, Nan and Shane, Jennifer and Kadlubsky, Alina},
  booktitle={2022 IEEE International Symposium on Mixed and Augmented Reality Adjunct (ISMAR-Adjunct)},
  title={Demo: End-to-end open-source location-based augmented reality in 5G},
  year={2022},
  volume={},
  number={},
  pages={897-898},
  doi={10.1109/ISMAR-Adjunct57072.2022.00194}}
```
```
@INPROCEEDINGS{9585798,
  author={Jackson, James and Vogt, Michael and Sörös, Gábor and Salazar, Mikel and Fedorenko, Sergey},
  booktitle={2021 IEEE International Symposium on Mixed and Augmented Reality Adjunct (ISMAR-Adjunct)},
  title={Demo: The First Open AR Cloud Testbed},
  year={2021},
  volume={},
  number={},
  pages={495-496},
  doi={10.1109/ISMAR-Adjunct54149.2021.00117}}
```

## Requirements
Unity version 2022.3

### Packages
- ARFoundation
- ARCore XR Plugin
- ARKit XR Plugin
- XR Plugin Managment
- TextMeshPro
- Draco 3D Data Compression
- glTFast
- KTX/Basis Texture
- meshoptimizer decompression for Unity(experimental)
- XR Interaction Toolkit Pre-Release
- Newtonsoft Json

#### Set project settings for Android and ARcore:
- Allow 'unsafe' Code - true
- Minimum API Level: Android 15.0
- Target Architectures - ARM64

#### Set project settings for iOS and ARKit:
- Allow 'unsafe' Code - true
- other settings standard for ARKit with Unity.

# Authentication
To test the login you need to use the Scene Login and add requried url:s and secrets. On object "@Oauth2" in the scene heirarcy.
I tested this using Auth0 but it should support anything that uses OAuth2.0.
The authentication uses PKCE flow and deep linking

## Required information
- Client ID
- Client Secret
- Authorization Endpoint
- Token Endpoint
- UserInfo Endpoint
- Redirect URL

## Deep Link in Unity
- https://docs.unity3d.com/2019.3/Documentation/Manual/enabling-deep-linking.html

### Enable Deep Link Android
For deep link to work on Android you need to follow your login services.
The AndroidManifest.xml also need to add intent and the deep link url

Custom android Manifest and Deep Link
- https://www.youtube.com/watch?v=0DRGmejDNMY

### Enable Deep Link iOS
Follow the Unity Deep Link

### Enable Deep Link Auth0
- https://auth0.com/docs/get-started/applications/enable-android-app-links-support
- https://auth0.com/docs/get-started/applications/enable-universal-links-support-in-apple-xcode



## License
[MIT License](Licence.md)

For contributions by [soeroesg](https://github.com/soeroesg) and [pnok](https://github.com/pnok):
Copyright 2025 Nokia,
Licensed under the MIT License,
SPDX-License-Identifier: MIT


This project also uses the following open source libraries:
- H3Net, https://github.com/RichardVasquez/h3net, Apache License v2
- DecimalMath, https://github.com/nathanpjones/DecimalMath, MIT license

