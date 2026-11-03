# Acheron: Echoes of the Deep

> Two-player VR escape game set in a sunken underwater facility: one player in the headset, the other outside with the logbook — they can only escape by talking to each other.

**Status:** 🚧 In development — tutorial target for the IUT open day (Feb. 2027)
**Context:** University project, BUT GEII — IUT de Rennes
**Team:** Adam Deruelle, Lucas, Baptiste, Ewen
**Stack:** Unity 6 · URP · XR Interaction Toolkit 3.4 · OpenXR · Meta Quest 3


<!-- TODO: add a screenshot or GIF from the headset -->

---

## Concept

An asymmetric co-op escape game inspired by the atmosphere of *BioShock*. The player in VR explores the facility and interacts with machines; the second player, without a headset, holds the technical logbook with schematics and data. Neither can solve the puzzles alone.

Puzzles are built around **electrical and industrial engineering** concepts from our degree, for example:

- **Flooding room** — balance three valves (main inlet, bypass, drain) using pressure gauges and the pump's maximum pressure from the logbook
- **Morse lights** — six blinking lamps to decode through several encoding steps

## Tech

- Unity 6 (6000.0) with the Universal Render Pipeline
- XR Interaction Toolkit 3.4 + XR Hands (hand tracking)
- OpenXR / Meta OpenXR, targeting **Meta Quest 3** standalone (PC link during development)

## Roadmap

- [x] Unity VR project set up and versioned on GitHub
- [ ] Tutorial (~5 min) playable for the open day
- [ ] First puzzles implemented
- [ ] Full game

## Run it

1. Open the project with **Unity 6000.0.82f1** (or a compatible 6000.0 version).
2. Open `Assets/Scenes/Scene.unity`.
3. Connect a Quest 3 via Link, or build for Android.
