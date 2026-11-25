# À faire : Acheron

> Liste établie le 25/11/2026, après la fusion d'`adam` (PR #10) et de `Lucas` (PR #11) dans `main`.
> Les points de rangement sont détaillés dans [`Rangement_A_Discuter.md`](Rangement_A_Discuter.md) : ici, on ne garde que l'action à faire et qui s'en occupe.

Légende : 🔴 bloquant · 🟠 important · 🟢 confort

---

## 1. Git / équipe

- [ ] 🟠 **Fusionner la PR `ewen` → `main`** (Ewen ou Adam)
  - La branche `ewen` fusionne sans conflit avec `main` à jour (vérifié le 25/11).
  - Après la fusion, chaque membre fait un `git pull` de `main`, puis fusionne `main` dans sa propre branche.
  - Ce qu'elle apporte : `Salle_Centrale.prefab`, `AfficheurPression.cs`, la police DSEG7 (`Commun/Font/`), et le déplacement d'`Interaction_Fusibles.cs` vers `Commun/Scripts/`.
- [ ] 🟢 Après la fusion, supprimer le dossier `Assets/Script/` devenu vide, **depuis Unity**.
- [ ] 🟢 Règle d'équipe à rappeler : **déplacer ou renommer uniquement depuis la fenêtre Project d'Unity**, jamais dans l'explorateur Windows, pour garder les `.meta` et les GUID.

## 2. Build

- [ ] 🔴 **Mettre la bonne scène dans le build** (une seule personne, car cela modifie les ProjectSettings)
  - Aujourd'hui, la liste ne contient que `Assets/_Acheron/Salles/Ewen/SampleScene.unity`.
  - Dans *File > Build Profiles > Scene List*, la remplacer par `Assets/_Acheron/Scenes/Scene.unity`.
- [ ] 🟠 Faire un build Android et le tester **sur le Quest 3 en autonome**, pas seulement via Link.
- [ ] 🟠 Mesurer les performances au casque (OVR Metrics Tool) : objectif 72 images/s stables.

## 3. Salle du piano (Adam)

Rappel : la salle se modifie dans le **prefab** `Commun/Prefabs/Salles/Salle_Piano.prefab`, pas dans les scènes.

### Éclairage
- [ ] 🟠 **Recalculer l'éclairage** (*Window > Rendering > Lighting > Generate Lighting*)
  - Le décor art déco, ses lumières baked (3 coupoles et la lumière de la fenêtre sur l'océan) et la nouvelle porte n'ont pas encore été pris en compte dans le calcul.
  - Vérifier dans quelle scène calculer l'éclairage : celui de `Scene.unity` couvre toutes les salles.
- [ ] 🟢 Ajouter une petite applique côté couloir : la porte d'entrée est presque invisible quand on arrive.

### Tests au casque
- [ ] 🟠 Énigme complète : lire la résistance et le voyant, jouer la séquence au doigt, vérifier le buzzer en cas d'erreur et le « clac » du coffre.
- [ ] 🟠 **Porte d'entrée** : l'ouvrir avec la poignée des deux côtés (pousser depuis la salle, tirer depuis le couloir), à la manette puis à la main.
- [ ] 🟠 **Coffre-fort** : ouvrir la porte avec la poignée, puis prendre la demi-plaque.
- [ ] 🟢 Vérifier qu'on ne peut pas se téléporter sous le piano, l'établi ou les meubles du décor.
- [ ] 🟢 Vérifier que les touches ne grésillent plus (anti-rebond).

### Contenu à finaliser
- [ ] 🟠 **Demi-plaque de verre** : remplacer la texture provisoire `T_DemiPlaque_Provisoire.png` (« 4 7 ») par les vrais chiffres, une fois le code final décidé avec l'équipe.
- [ ] 🟠 **Seconde moitié de la plaque** : décider dans quelle salle on la trouve. Elle doit avoir le même `identifiantPlaque` (`Plaque_Code_1`) et la moitié `Droite`.
- [ ] 🟠 **Carnet papier** du joueur sans casque : il est fait à part. Vérifier qu'il reprend **exactement** le tableau de `DonneesEnigmePiano.asset` (7 valeurs × 4 couleurs) et le code couleur des résistances.
- [ ] 🟢 Ajouter un petit son de grincement à l'ouverture de la porte d'entrée.

## 4. Rangement (détail dans `Rangement_A_Discuter.md`)

| Qui | Action |
|---|---|
| Lucas | Ranger `Disjoncteur_résistances.glb` et `hello_neighbor_2_fuse.glb` dans `Commun/Models/`. **Vérifier la licence du fusible**, qui semble venir du jeu *Hello Neighbor 2*. |
| Lucas | Déplacer `modular_pipes/` et `samsung_galaxy_s3/` de `Commun/Prefabs/` vers `Commun/Models/`. Remplacer ou supprimer le téléphone, qui porte une marque et n'est pas de l'époque. |
| Lucas | Renommer `Interaction_Fusibles.cs` en `InteractionFusible.cs`, comme la classe. |
| Baptiste | Fusionner les 12 matériaux `Tiles133C_*` en un seul `M_Carrelage`. |
| Baptiste / Ewen | Recréer `Commun/Textures/` et y ranger les images actuellement dans `Commun/Materials/`. |
| Ewen | Remplacer les textures 4K `.exr` de `diamond plate` (~80 Mo, dont 4 fichiers inutilisés) par une version 1K/2K en JPG. |
| Ewen | Renommer `New Material.mat`, `Cylinder.00x` et `obturator 1`, et supprimer les prefabs inutilisés (`Pipe_2` à `Pipe_6`…). |
| Lucas / Ewen | Supprimer `SampleScene` et `BasicScene`, **après** avoir corrigé la liste du build. |
| Adam | Supprimer `Temporaire.cs` et remplacer `A supprimer.cs` et `A supprimer.mixer` par des `.gitkeep`. |
| Adam | README : corriger le chemin de la scène en `Assets/_Acheron/Scenes/Scene.unity` (versions EN et FR). |

## 5. Plus tard

- [ ] 🟢 Fusionner la logique de porte de `CoffreFort.cs` et de `PorteBattante.cs`, qui font presque la même chose, en un seul composant réutilisable dans `Commun/Scripts/`.
- [ ] 🟢 Remplacer les sons générés par code (notes, buzzer, verrou) par de vrais enregistrements, si le temps le permet.
- [ ] 🟢 Remplacer les primitives du décor par des modèles 3D, en gardant les mêmes prefabs pour que les salles se mettent à jour toutes seules.
