# GlobalHotkeyAudioPlayer
Basically bind global shortcuts (works in any/every game), set sounds, set device to stream the sound to (works nicelly with VoiceMeeter Input Device if wanted to redirect music/sounds to game through mic without streaming game sounds itself)

Style - mostly MVVM, yet as this was put together in 4hours some refactoring to improve style needed (factories for file dialogs, audio service, etc)

Feel free to reuse any code (putting this on git mostly because of input handling, as InputSimulator lib I tried using does not work inside some games for reasons unknown to me, my guess - sending virtual key instead of scan code)