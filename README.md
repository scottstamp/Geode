Geode
=====
C# (.NET Standard 2.0) Extension API for the cross-platform packet logger G-Earth

**G-Earth 1.4.1 or later is required**, both Flash/AIR and Unity (G-Chrome) are supported

## Guide for end users
- Connect AIR or Unity client using G-Earth.
- Once connected, open the executable file of the extension (.exe).
- G-Earth will pop a notification: "Extension tries to connect but isn't known to G-Earth, accept this connection?".
- Click Yes.

## Guide for developers
- Use your favorite base code from the Examples folder.
- Once you have the extension ready, distribute the executable along with "Geode.dll".
- When creating Unity extensions `GService.UnityIn` and `GService.UnityOut` are available for message headers.