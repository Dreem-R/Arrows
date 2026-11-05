# Arrows

A 2D puzzle game made in **Unity 6** as an interview assignment. Each coloured line has an arrowhead at both ends. Tap one and it travels along its line to the other end, removing the line. Clear every line to win, but if two moving arrowheads collide, you lose a life and both lines reset.

## Gameplay

- **Tap an arrowhead** to send it along its line. The part it has covered disappears as it goes.
- **Start as many lines as you like** at the same time. The opposite head of a moving line is disabled until that line finishes or resets.
- **Only arrowheads collide.** Lines can cross, merge or overlap freely.
- **You have 3 lives** per level. Losing them all ends the level.
- **Hint** picks a random line that's safe to start right now, and only the head that's safe to tap stays bright. Hints are unlimited.

## Levels

| Level | Lines | Idea |
|---|---|---|
| 1 | 14 | The layout from the assignment |
| 2 | 6 | Every line shares one street, so direction matters |
| 3 | 8 | Six lines spiral onto one ring, with a small X in the middle |

The assignment asked for one level. I added two more to show that new levels need no code changes. Because of that, the win popup's button shows Next between levels and Restart after the last one, which goes back to level 1. Losing always shows Retry.

## Technical highlights

- **Data-driven levels.** A level is a `LevelSO` asset holding each line's colour and spline. `LevelSpawner` builds the board from it at runtime.
- **Shared tuning.** Lives, speed, collision distance and animation timings are in one `GameSettingsSO` asset.
- **Event-driven.** Gameplay raises C# events, and visuals and UI subscribe to them, so `Line` and `LineVisual` stay separate.
- **Collisions without physics.** `CollisionSystem` checks the closest distance between two heads across each frame, so a slow frame can't let them pass through each other.
- **Hints that predict collisions.** `HintSystem` plays the board forward in a simulation using the same collision check. It re-checks while a hint is showing, in case a head stops being safe.
- **Camera framing.** `BoardCamera` measures where the UI is and fits any level in the remaining space, at any window size.

## Adding a level

1. Create a level asset: **Create > Arrows > Level**.
2. Open `Assets/Scenes/LevelAuthoring.unity`, select `Level`, assign your asset and click **Load Splines From Level**.
3. Draw each line with the **Draw Splines Tool**, keep it under `Level`, and add a `LineAuthoring` component to set its colour. The first and last points are where the arrowheads go.
4. Select `Level` and click **Bake Splines To Level**.
5. In `GameScene`, add the asset to **Level List** on `GameManager`.

## Running the project

1. Open the project in **Unity 6000.0.71f1** or a later 6000.0 version. It needs internet the first time, because PrimeTween downloads from NPM.
2. Open `Assets/Scenes/GameScene.unity` and press Play.

## Project structure

```
Assets/
├── Scripts/
│   ├── Core/        # GameManager, LevelSpawner, BoardCamera
│   ├── Lines/       # Line, LinePath, ArrowHead and their visuals
│   ├── Systems/     # CollisionSystem, HintSystem
│   ├── Input/       # GameInput, LineSelector
│   ├── UI/          # Lives, level label, hint and popup UI
│   ├── Data/        # LevelSO, GameSettingsSO
│   ├── Authoring/   # Components used to draw levels
│   └── Editor/      # LevelBaker
├── Data/            # Game settings and level assets
├── Prefabs/
├── Art/
└── Scenes/          # GameScene, LevelAuthoring
```

## Built with

- Unity 6 (6000.0.71f1), Universal Render Pipeline
- [Splines](https://docs.unity3d.com/Packages/com.unity.splines@latest)
- Unity Input System
- [PrimeTween](https://github.com/KyryloKuzyk/PrimeTween) by Kyrylo Kuzyk

## Author

**Dreem-R**: [github.com/Dreem-R](https://github.com/Dreem-R)
