# Audio System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                        AUDIO SYSTEM OVERVIEW                        │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│                         GAME SCRIPTS                                 │
│  (PlayerController, UIController, GameManager, etc.)                 │
└────────────┬─────────────────────────────────────────────────────────┘
             │
             │ Uses Simple API:
             │ - PlayMusic()
             │ - PlaySFX()
             │ - SetVolume()
             ▼
┌──────────────────────────────────────────────────────────────────────┐
│                      AUDIOMANAGER (Singleton)                        │
│  - Centralized controller for all audio                              │
│  - Handles music + SFX                                               │
│  - Volume management                                                 │
│  - Event integration                                                 │
└───┬──────────────────┬──────────────────┬──────────────────┬─────────┘
    │                  │                  │                  │
    │ Uses             │ Uses             │ Uses             │ Listens to
    ▼                  ▼                  ▼                  ▼
┌───────────┐  ┌──────────────┐  ┌──────────────┐  ┌─────────────┐
│AudioSource│  │AudioSourcePool│ │AudioConfig   │  │EventManager │
│(Music)    │  │              │  │              │  │             │
│           │  │ - SFX pooling│  │ - Sound data │  │ - Events    │
│ - Looping │  │ - Performance│  │ - Clips      │  │ - Callbacks │
│ - Persist │  │ - Reusable   │  │ - Volumes    │  │             │
└───────────┘  └──────────────┘  └──────────────┘  └─────────────┘
                       │                  │
                       │ Uses             │ Uses
                       ▼                  ▼
               ┌──────────────┐  ┌──────────────┐
               │AudioSource[] │  │EffectKey Enum│
               │(Pooled)      │  │              │
               │              │  │ - Identifiers│
               │ - SFX only   │  │ - Extensible │
               └──────────────┘  └──────────────┘


┌─────────────────────────────────────────────────────────────────────┐
│                        CONTROL FLOW EXAMPLES                        │
└─────────────────────────────────────────────────────────────────────┘

EXAMPLE 1: Playing Background Music
────────────────────────────────────
GameManager.Start()
    │
    └─> AudioManager.Instance.PlayMusic EffectKey.GameplayMusic)
            │
            ├─> Look up EffectKey in AudioConfiguration
            ├─> Get AudioClip
            ├─> Calculate volume (master * music * clip)
            └─> Play on dedicated music AudioSource


EXAMPLE 2: Playing Positioned SFX
──────────────────────────────────
Asteroid.OnDestroy()
    │
    └─> EventManager.TriggerEntityDestroyed(position, EffectKey.AsteroidExplosion)
            │
            └─> AudioManager.HandleEntityDestroyed()
                    │
                    ├─> Map EffectKey → EffectKey
                    └─> PlaySFXAtPosition EffectKey.AsteroidExplosion, position)
                            │
                            ├─> Look up EffectKey in AudioConfiguration
                            ├─> Get AudioClip
                            ├─> Calculate volume (master * sfx * clip)
                            ├─> Calculate stereo pan from position
                            ├─> Get AudioSource from pool
                            └─> Play on pooled AudioSource


EXAMPLE 3: Volume Adjustment
─────────────────────────────
SettingsMenu.OnSliderChanged(value)
    │
    └─> AudioManager.Instance.SetMusicVolume(value)
            │
            ├─> Update musicVolume field
            └─> UpdateMusicVolume()
                    │
                    └─> Recalculate current music AudioSource volume


┌─────────────────────────────────────────────────────────────────────┐
│                       SOLID PRINCIPLES MAPPING                      │
└─────────────────────────────────────────────────────────────────────┘

SINGLE RESPONSIBILITY
─────────────────────
✓ AudioManager        → Audio playback only
✓ AudioConfiguration  → Audio data storage only
✓ AudioSourcePool     → Object pooling only
✓ EffectKey          → Type definitions only

OPEN/CLOSED
───────────
✓ Add new sounds by extending EffectKey enum
✓ No need to modify AudioManager code
✓ Configure in ScriptableObject (closed to modification)

LISKOV SUBSTITUTION
──────────────────
✓ Not heavily used (no inheritance hierarchy)
✓ All EffectKey values behave consistently

INTERFACE SEGREGATION
────────────────────
✓ Public API is minimal and focused
✓ Clients only depend on what they use

DEPENDENCY INVERSION
───────────────────
✓ Game scripts depend on AudioManager abstraction
✓ Not on concrete AudioSource implementations
✓ Configured via ScriptableObject (DI-friendly)


┌─────────────────────────────────────────────────────────────────────┐
│                     PERFORMANCE CHARACTERISTICS                     │
└─────────────────────────────────────────────────────────────────────┘

MEMORY
──────
✓ Pooled AudioSources (no GC allocation for SFX)
✓ Singleton pattern (one instance only)
✓ Dictionary lookup for O(1) sound retrieval

CPU
───
✓ Minimal overhead per sound
✓ Stereo pan calculation (simple viewport math)
✓ Volume multiplication (fast)

SCALABILITY
──────────
✓ Pool grows dynamically if needed
✓ Supports unlimited simultaneous SFX
✓ One music track at a time (typical for games)

┌─────────────────────────────────────────────────────────────────────┐
│                     ADDING A NEW SOUND EFFECT                       │
└─────────────────────────────────────────────────────────────────────┘

Want to add "PowerUp Collect" sound?

Step 1: Add to EffectKey.cs
───────────────────────────
public enum EffectKey {
    // ...
    PowerUpCollect,  ← Add this
}

Step 2: Configure in Unity
──────────────────────────
AudioConfiguration asset:
  - Add new entry
  - Type: PowerUpCollect
  - Clips: [your clip]
  - Volume: 1.0

Step 3: Use in code
──────────────────
public void OnPowerUpCollected() {
    AudioManager.Instance.PlaySFX EffectKey.PowerUpCollect);
}

✓ No changes to AudioManager
✓ No changes to other systems
✓ Completely isolated addition
