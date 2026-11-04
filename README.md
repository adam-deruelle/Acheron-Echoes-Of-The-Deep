<a id="top"></a>
# Acheron: Echoes of the Deep

**[🇬🇧 English](#english) · [🇫🇷 Français](#francais)**

<!-- TODO: add a screenshot or GIF from the headset -->

---

<a id="english"></a>
## 🇬🇧 English

> Two-player VR escape game set in a sunken underwater facility: one player in the headset, the other outside with the logbook — they can only escape by talking to each other.

**Status:** 🚧 In development — tutorial target for the IUT open day (Feb. 2027)
**Context:** University project, BUT GEII — IUT de Rennes
**Team:** Adam Deruelle, Lucas, Baptiste, Ewen
**Stack:** Unity 6 · URP · XR Interaction Toolkit 3.4 · OpenXR · Meta Quest 3

### Concept

An asymmetric co-op escape game inspired by the atmosphere of *BioShock*. The player in VR explores the facility and interacts with machines; the second player, without a headset, holds the technical logbook with schematics and data. Neither can solve the puzzles alone.

Puzzles are built around **electrical and industrial engineering** concepts from our degree, for example:

- **Flooding room** — balance three valves (main inlet, bypass, drain) using pressure gauges and the pump's maximum pressure from the logbook
- **Morse lights** — six blinking lamps to decode through several encoding steps

### Tech

- Unity 6 (6000.0) with the Universal Render Pipeline
- XR Interaction Toolkit 3.4 + XR Hands (hand tracking)
- OpenXR / Meta OpenXR, targeting **Meta Quest 3** standalone (PC link during development)

### Roadmap

- [x] Unity VR project set up and versioned on GitHub
- [ ] Tutorial (~5 min) playable for the open day
- [ ] First puzzles implemented
- [ ] Full game

### Run it

1. Open the project with **Unity 6000.0.82f1** (or a compatible 6000.0 version).
2. Open `Assets/Scenes/Scene.unity`.
3. Connect a Quest 3 via Link, or build for Android.

[⬆ Back to top](#top)

---

<a id="francais"></a>
## 🇫🇷 Français

> Escape game VR à deux joueurs dans une base sous-marine engloutie : un joueur dans le casque, l'autre à l'extérieur avec le carnet — ils ne peuvent s'échapper qu'en communiquant.

**Statut :** 🚧 En développement — tutoriel visé pour la JPO de l'IUT (février 2027)
**Contexte :** Projet universitaire, BUT GEII — IUT de Rennes
**Équipe :** Adam Deruelle, Lucas, Baptiste, Ewen
**Stack :** Unity 6 · URP · XR Interaction Toolkit 3.4 · OpenXR · Meta Quest 3

### Concept

Un escape game coopératif asymétrique inspiré de l'ambiance de *BioShock*. Le joueur en VR explore la base et interagit avec les machines ; le second joueur, sans casque, tient le carnet technique avec les schémas et les données. Aucun des deux ne peut résoudre les énigmes seul.

Les énigmes s'appuient sur des notions de **génie électrique et d'informatique industrielle** de notre formation, par exemple :

- **Salle inondée** — équilibrer trois vannes (arrivée principale, dérivation, évacuation) à l'aide des manomètres et de la pression maximale de la pompe indiquée dans le carnet
- **Lampes en morse** — six lampes clignotantes à décoder en plusieurs étapes

### Technique

- Unity 6 (6000.0) avec l'Universal Render Pipeline
- XR Interaction Toolkit 3.4 + XR Hands (suivi des mains)
- OpenXR / Meta OpenXR, cible **Meta Quest 3** en autonome (relié au PC pendant le développement)

### Roadmap

- [x] Projet Unity VR créé et versionné sur GitHub
- [ ] Tutoriel (~5 min) jouable pour la JPO
- [ ] Premières énigmes implémentées
- [ ] Jeu complet

### Lancer le projet

1. Ouvrir le projet avec **Unity 6000.0.82f1** (ou une version 6000.0 compatible).
2. Ouvrir `Assets/Scenes/Scene.unity`.
3. Connecter un Quest 3 via Link, ou compiler pour Android.

[⬆ Retour en haut](#top)
