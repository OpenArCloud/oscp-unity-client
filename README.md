# AC Viewer Unity app for OSCP clients 

This is the basic `AC Viewer` project for OSCP clients based on AC Viewer app
from the Augmented City company.

## Requirements

Unity version 2021.2.13f1

### Packages
- ARFoundation 4.2.2
- ARCore XR Plugin 4.2.2
- ARKit XR Plugin 4.2.2
- XR Plugin Managment 4.2.1
- TextMeshPro 3.0.6
- Draco 3D Data Compression 4.0.2
- glTFast 4.6.0
- KTX/Basis Texture 2.0.1
- meshoptimizer decompression for Unity(experimental) 0.1.0-preview.5
- XR Interaction Toolkit Pre-Release 1.0.0-pre.8
- Newtonsoft Json 2.0.2

#### Set project settings for Android and ARcore:
- Allow 'unsafe' Code - true
- Minimum API Level: Android 10.0
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

This project uses the following open source libraries:
- H3Net, https://github.com/RichardVasquez/h3net, Apache License v2
- DecimalMath, https://github.com/nathanpjones/DecimalMath, MIT license
