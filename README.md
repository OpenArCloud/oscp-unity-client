# AC Viewer Unity app for OSCP clients 

This is the basic `AC Viewer` project for OSCP clients based on AC Viewer app
from the Augmented City company.

## Requirements

Unity version 2021.1.19f or higher.
Before importing the package, please do the following in the project:

SDK uses ARFoundation systems, so you have to install packages (Window -> Package manager):
- ARFoundation 4.1.9
- ARCore XR Plugin 4.1.9
- ARKit XR Plugin 4.1.9

Set project settings for Android and ARcore:
- Allow 'unsafe' Code - true
- Minimum API Level: Android 8.0
- Target Architectures - ARM64
- Delete VulcanAPI from Graphics API (ARCore doesn’t support).

Set project settings for iOS and ARKit:
- Allow 'unsafe' Code - true
- other settings standard for ARKit with Unity.


## License

[MIT License](Licence.md)
