# Unity client for Open Spatial Computing Platform

This is a Unity client for [Open AR Cloud](https://www.openarcloud.org/)'s Open Spatial Computing Platform, originally created by [Augmented City](https://augmented.city/) company based on their AC Viewer app. It was extended in a [project](https://medium.com/openarcloud/our-projects-and-achievements-in-2022-5baaa541cce1) funded by NGI Atlantic.

References:
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
Unity version 2021.3.0f1

### Packages
- ARFoundation 4.2.2
- ARCore XR Plugin 4.2.2
- ARKit XR Plugin 4.2.2
- XR Plugin Managment 4.2.1
- TextMeshPro 3.0.6
- Draco 3D Data Compression 4.0.2
- glTFast 4.6.0
- KTX/Basis Texture 2.1.2
- meshoptimizer decompression for Unity(experimental) 0.1.0-preview.5
- XR Interaction Toolkit Pre-Release 1.0.0-pre.8
- Newtonsoft Json 2.0.2

#### Set project settings for Android and ARcore:
- Allow 'unsafe' Code - true
- Minimum API Level: Android 9.0
- Target Architectures - ARM64
- Delete VulcanAPI from Graphics API (ARCore doesn’t support).

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

For contributions by [soeroesg](https://github.com/soeroesg): 
Copyright 2022 Nokia,
Licensed under the MIT License,
SPDX-License-Identifier: MIT


This project also uses the following open source libraries:
- H3Net, https://github.com/RichardVasquez/h3net, Apache License v2
- DecimalMath, https://github.com/nathanpjones/DecimalMath, MIT license

