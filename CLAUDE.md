# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

"The Square" is a 2D Unity game (Unity 2022.3.21f1, Universal Render Pipeline). It's a Unity project, not a
standalone app: there is no CLI build/test workflow — all building, running, and testing happens through the
Unity Editor or its command-line batch mode.

## Encoding — read before editing/writing files

Most scripts were authored in Visual Studio with **UTF-8 + LF** line endings. If a read/write produces mangled
output or you suspect an encoding mismatch, **stop and ask the user to fix it rather than working around it**.
Do not use `cat` or other shell text dumps on source files — use the Read/Edit/Write tools.

## Commands

There is no npm/make/cargo-style build here. Common workflows:

- **Open/build**: via Unity Editor (`Unity.exe -projectPath .`) — there's no separate CLI build script in this repo.
- **Tests**: uses `com.unity.test-framework` (Unity Test Runner, EditMode/PlayMode). Run via Editor's Test Runner
  window; there is no headless test command configured in-repo.
- **Solution files**: `The-Square-Final.sln`, `Assembly-CSharp.csproj`, `Assembly-CSharp-Editor.csproj`, `Game.csproj`
  are Unity-generated — regenerate via the Editor rather than hand-editing.
- Git LFS is configured (`.gitattributes`) for large audio assets (e.g. `Assets/Resources/Musics/*.wav`).

## Architecture

### Manager singletons (`Assets/Scripts/GameManager/`)
Game-wide systems are implemented as MonoBehaviour singletons: `public static <Class> instance;` assigned in
`Awake()`, accessed elsewhere as `XManager.instance.field`. Key ones: `PlayerManager`, `CameraManager`,
`SaveManager`, `SoundManager`, `QuestManager`, `MapManager`, `InventoryManager`, `CraftingManager`,
`StatsManager`, `MonsterSpawnManager`, `EventGeneratorManager`, `LocalizationManager`, `SceneManager`-related
classes, etc. Only some persist across scenes via `DontDestroyOnLoad` (e.g. `CameraManager`, `SoundManager`,
`LocalizationManager`, `EventGeneratorManager`, `MonsterSpawnManager`) — most managers are scene-scoped and
re-instantiated per scene, so don't assume a manager instance survives a scene change unless it explicitly
calls `DontDestroyOnLoad`.

### Entity/monster composition (`Assets/Scripts/Game/Entities/`, `Assets/Scripts/Monster/`)
Player and monster GameObjects are built from sibling components fetched via `GetComponent<T>()` rather than
a central state object. Common companions on an entity:
- `Stats` — HP, invulnerability (`isVulnerable`), attack strength, `entityType` (e.g. `EntityType.Monster`).
- `NewMonsterMovement` — movement + player detection (`IsInDetectionZone`); has `EnableAnimations` toggle that
  must be turned off during attack animations and back on afterward.
- `ObjectAnimation` — animation triggers (`PlayAnimation("Idle")`, or coroutine variants).
- `SoundContainer` — per-entity sound effects keyed by action name.
- `LifeManager` — HP/damage on the player, reached via `PlayerManager.instance.player.GetComponent<LifeManager>()`.
- `EntityEffects` — status effects (poison, etc.), set via `EntityEffects.SetState(...)` on collision.

Monster behavior scripts (`*Behiavor.cs` — note the project's consistent misspelling, don't "fix" it) are
structured as coroutines for anything that unfolds over time (appear → attack → cooldown), guarded by boolean
state flags (`isAttacking`, etc.) that must be reset on interruption/death — always pair a `StartCoroutine`
with proper `StopCoroutine` cleanup. See `Documentation/GuideCreationMonstre.md` for the full pattern this
codebase expects new monster scripts to follow, including damage delivery patterns (projectiles, AoE hitbox
zones via `DamageZoneBehiavor`, direct `TakeDamage` calls) and visual feedback conventions (sprite flip toward
player, fade coroutines, `CameraManager.instance` shake/filter calls).

### Combat modifier systems
Two significant, currently-active gameplay systems layered on top of base `Stats.cs`:
- **Stances & Runes** (`Assets/Scripts/Mechanics/StancesAndRunes/`, `StanceAndRunicManager`) — see
  `Documentation/Conception_Stances_Focus_Combat.md`. Player cycles a damage-type "Posture" (top-left HUD) and
  an active "Rune" from a 3-slot equipped deck (top-right HUD); final damage = base × posture multiplier ×
  rune multiplier. Runes read/modify `Stats.cs` fields directly.
- **Seals** (`Seal`, `SealManager`, `SealAuraManager`, `SealMomentumManager`, `SealResonanceManager`) — see
  `Documentation/Documentation_Systeme_Sceaux.md`. Alchemy system: 4 `SpecialItems` are averaged by elemental
  `EssenceComposition` (8 Spirits) into a seal; essence percentages scale each Spirit's stat contributions and
  vote toward 4 archetypes (Buff, Resonance, Aura, Momentum), each active once its weighted score crosses a
  threshold. Each archetype's runtime logic lives in its own dedicated manager script (kept out of
  `PlayerController`/`Stats` except for Buff, which applies directly in `Stats.cs`).

### Universe Heart mode (`Assets/Scripts/Mechanics/UniverseHeart/`, `InsideTheSquareManager`)
A separate stealth-survival mode (see `Documentation/UNIVERSE_HEART.md`): collect 5 heart fragments (carry one
at a time) within an 8-minute limit while avoiding patrol enemies ("Veilleurs", vision-based alert trigger) and
pursuers ("Tueurs", wall-clipping chasers at player speed); black-hole safe zones reset alert state.

### Save/load (`Assets/Scripts/GameManager/SaveManager.cs`)
Single `SaveManager.instance` singleton handles all persistence via `JsonUtility.ToJson`/`FromJson` +
`File.WriteAllText`/`ReadAllText` against two flat JSON files at the project root: `equipementSave.json`
(main save data) and `eventIDSave.json` (event/flag state, via the nested `TwoStateContainer` class).
`DeleteSave()` removes both via `File.Delete`. `LocalizationManager` follows the same JsonUtility file-I/O
pattern for a separate localization data file — don't conflate the two.

### Custom Editor tooling (`Assets/Editor/`)
Several hand-written editor extensions drive content authoring and are meaningful to understand before editing
related runtime scripts:
- `EventEditor.cs` — dynamic `CustomEditor` for the `Event` type; draws a different set of Inspector fields
  per `eventType` (BOOK, CINEMATIC, PNJ, BATTLE, CAMERA, WAIT, TEXT, CHANGE_SCENE, SPECIAL_METHODS), with nested
  switches for sub-types (e.g. PNJ: SPAWN/MOVE/EMOTIONS/SPEAK/ANIM/CHANGE_SIZE). When adding a new `eventType`
  or field to `Event`, this file needs matching changes or the new field won't show in the Inspector.
- `EventContainerEditor.cs`, `QuestEditor.cs`, `SkillEditor.cs`, `StatsEditor.cs`, `CraftingGridFillerEditor.cs`,
  `WorldMapGeneratorEditor.cs`, `SceneChangerEditor.cs`, `ScenePreviewWindow.cs`, `GenerateurIDWindow.cs`,
  `MonsterStatsViewer.cs` — similar custom-inspector/editor-window tools for their respective systems (quests,
  skills, stats balancing, crafting grids, world map generation, scene navigation/preview, ID generation).

## Notable packages in use
Universal Render Pipeline (`com.unity.render-pipelines.universal`), new Input System
(`com.unity.inputsystem`), Cinemachine, TextMeshPro, Timeline, ML-Agents (`com.unity.ml-agents`), Unity Test
Framework, Visual Scripting.

## Design documentation
`Documentation/` contains living design docs worth checking before changing related systems:
`Bestiaire.md` (bestiary), `Conception_Stances_Focus_Combat.md`, `Documentation_Systeme_Sceaux.md`,
`EquipmentDocumentation.md`/`.xlsx`, `GuideCreationMonstre.md`, `UNIVERSE_HEART.md`, `ToDoList.md`.
