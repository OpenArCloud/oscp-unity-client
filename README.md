# AC Viewer Unity app for OSCP clients 

This is the basic `AC Viewer` project for OSCP clients based on AC Viewer app
from the Augmented City company.

## Requirements

Unity version 2021.1.28f1 or higher.
Before importing the package, please do the following in the project:

SDK uses ARFoundation systems, so you have to install packages (Window -> Package manager):
- ARFoundation 4.1.7
- ARCore XR Plugin 4.1.7
- ARKit XR Plugin 4.1.7

Other Packages
- TextMeshPro 3.0.6

Set project settings for Android and ARcore:
- Allow 'unsafe' Code - true
- Minimum API Level: Android 8.0
- Target Architectures - ARM64
- Delete VulcanAPI from Graphics API (ARCore doesn’t support).

Set project settings for iOS and ARKit:
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
