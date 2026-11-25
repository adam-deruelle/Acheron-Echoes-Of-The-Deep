# Rangement du dossier Assets : points à discuter

> Rapport du 25/11/2026. **Rien n'a été déplacé ni supprimé** parmi les éléments ci-dessous : ce sont des propositions à valider avec le membre concerné.
> « Référencé par » = recherche du GUID de l'asset dans toutes les scènes, prefabs, matériaux et assets du projet (branche `adam` après fusion de `main`).
> Le propriétaire est déduit de l'auteur des commits : **L962616** a fait les commits de la salle de Lucas (`SandBox_Lucas`), **Zeyrox1425** ceux de Baptiste (`SandBox_Baptiste`), **Ewen Morel--Hay** ceux d'Ewen. À confirmer.

Rappel de la convention :

| Dossier | Contenu |
|---|---|
| `Assets/_Acheron/Commun/` | ce qui est partagé entre les salles |
| `Assets/_Acheron/Salles/<Prénom>/` | le travail de chacun |
| `Assets/_Acheron/Scenes/` | la scène principale |

Règle pour tout rangement : **déplacer uniquement depuis Unity** (glisser dans la fenêtre Project, ou `AssetDatabase.MoveAsset`), jamais dans l'explorateur Windows, sinon les `.meta` et donc les liens entre assets sont perdus.

---

## ⚠️ Priorité : la scène principale n'est pas dans le build

`ProjectSettings/EditorBuildSettings.asset` ne contient qu'une seule scène : `Assets/_Acheron/Salles/Ewen/SampleScene.unity`. Ce n'est **pas** `Assets/_Acheron/Scenes/Scene.unity`, donc un build Android lance la mauvaise scène.

**Proposition** : dans *File > Build Profiles > Scene List*, remplacer `SampleScene` par `Assets/_Acheron/Scenes/Scene.unity`. Cela modifie les ProjectSettings, à faire par une seule personne pour éviter les conflits.

---

## 1. `Assets/Script/` (au singulier) : `Interaction_Fusibles.cs`

| | |
|---|---|
| Référencé par | `Commun/Prefabs/DisjoncteurEtFusible.prefab`, utilisé dans `Salles/Lucas/SandBox_Lucas.unity` |
| Propriétaire probable | Lucas (L962616) |
| Problème | Un dossier `Script` isolé à la racine. Note : `Assets/Scripts/` n'existe plus, la phase 1 l'a vidé et supprimé. La classe s'appelle `InteractionFusible` mais le fichier `Interaction_Fusibles.cs` : Unity accepte ce décalage pour un composant déjà posé, mais pas pour en ajouter un nouveau (« script class cannot be found »). |

**Proposition** : déplacer dans `Salles/Lucas/Scripts/` (ou dans `Commun/Scripts/` si le disjoncteur sert à plusieurs salles), puis renommer le fichier en `InteractionFusible.cs` pour qu'il corresponde à la classe. Le GUID est conservé, donc le prefab reste lié. Supprimer ensuite le dossier `Assets/Script/` vide.

## 2. Modèles `.glb` à la racine d'`Assets`

| Fichier | Taille | Référencé par | Propriétaire probable |
|---|---|---|---|
| `Disjoncteur_résistances.glb` | 3,0 Mo | `Commun/Prefabs/DisjoncteurEtFusible.prefab` | Lucas (L962616) |
| `hello_neighbor_2_fuse.glb` | 2,0 Mo | `Commun/Prefabs/DisjoncteurEtFusible.prefab` | Lucas (L962616) |

Problèmes :
- ils sont rangés hors d'`_Acheron` ;
- le nom `hello_neighbor_2_fuse` laisse penser à un modèle **extrait du jeu commercial *Hello Neighbor 2***, à vérifier car le projet est diffusé (JPO, GitHub) ;
- `Disjoncteur_résistances` contient un accent, ce qui peut poser souci sur certains outils et en ligne de commande.

**Proposition** : déplacer dans `Commun/Models/Disjoncteur/`, renommer sans accent (`Disjoncteur_Resistances.glb`, `Fusible.glb`), et vérifier la licence du modèle de fusible. S'il vient du jeu, le remplacer par un modèle libre ou fait maison.

## 3. `Commun/Materials` : doublons, textures mélangées, textures 4K

### 3a. Doublons « Tiles133C_2K-JPG »

| Fichier | Référencé par | Propriétaire probable |
|---|---|---|
| `Tiles133C_2K-JPG_Color 1.mat` … `Color 9.mat` (9 fichiers quasi identiques) | `Salles/Baptiste/SandBox_Baptiste.unity`, tous les 9 | Baptiste (Zeyrox1425) |
| `Tiles133C_2K-JPG_Color.mat` | **non référencé** | Baptiste |
| `Tiles133C_2K-JPG_AmbientOcclusion.mat` | `Salles/Lucas/SandBox_Lucas.unity` | Baptiste |
| `Tiles133C_2K-JPG_AmbientOcclusion 1.mat` | **non référencé** | Baptiste |
| `Tiles133C_2K-JPG_NormalGL.mat` | **non référencé** | Baptiste |

Problèmes :
- les « … 1 » à « … 9 » sont des copies créées par Unity, sans doute à chaque glisser-déposer de la texture sur un objet ;
- les matériaux nommés d'après une seule texture (`_AmbientOcclusion.mat`, `_NormalGL.mat`) n'ont pas de sens : il faut **un seul** matériau carrelage qui utilise les trois textures.

**Proposition** (Baptiste) :
1. créer un seul `M_Carrelage.mat` avec la couleur, la normal map et l'occlusion ;
2. dans `SandBox_Baptiste` et `SandBox_Lucas`, remplacer les matériaux par celui-ci ;
3. supprimer les 12 autres `.mat`.

### 3b. `New Material.mat`

| | |
|---|---|
| Référencé par | `Commun/Prefabs/toit.prefab`, utilisé dans `SandBox_EwEN` |
| Propriétaire probable | Ewen |

**Proposition** : le renommer d'après son rôle (par ex. `M_Toit.mat`). Un renommage par Unity garde le lien.

### 3c. Textures mélangées aux matériaux

`Tiles133C_2K-JPG_*.jpg` (3 fichiers) et le dossier `diamond plate/` (images + un sous-dossier `Materials/`) sont dans `Commun/Materials/`. Le commit de fusion a d'ailleurs **renommé** l'ancien dossier `Commun/Textures/` en `Commun/Materials/diamond plate/`, ce qui est probablement un déplacement involontaire.

**Proposition** : recréer `Commun/Textures/` et y ranger les images, par ex. `Commun/Textures/Carrelage/` et `Commun/Textures/TolesStriees/`. Garder les `.mat` dans `Commun/Materials/`. Pas de nom de dossier avec espace (`diamond plate` → `TolesStriees`).

### 3d. Textures 4K « diamond plate » (Ewen)

| Fichier | Taille sur disque | Utilisé |
|---|---|---|
| `metal_plate_diff_4k.jpg` | 9,7 Mo | oui, par `metal_plate_diff_4k.mat` → `SandBox_EwEN` |
| `metal_plate_disp_4k.png` | 16,2 Mo | **non** |
| `metal_plate_metal_4k.exr` | 22,3 Mo | **non** |
| `metal_plate_nor_gl_4k.exr` | 13,2 Mo | **non** |
| `metal_plate_rough_4k.exr` | 19,2 Mo | **non** |

Une bonne nouvelle : l'import est déjà limité à 2048 px, y compris pour Android, donc le build Quest n'embarque pas du 4K. En revanche, ces **~80 Mo** alourdissent le dépôt GitHub de toute l'équipe, et 4 fichiers sur 5 ne servent à rien. Les `.exr` sont du HDR 32 bits, inadaptés à des textures de matériau sur Quest. La displacement map n'est pas utilisée par URP Lit.

**Proposition** (Ewen) :
- télécharger la version **1K ou 2K en JPG/PNG** sur le site d'origine (Poly Haven) ;
- garder la couleur, la normal map et éventuellement la carte métal/rugosité combinée ;
- supprimer les `.exr`, la `disp` et la version 4K.

Si on garde la normale, la brancher dans le matériau, ce qu'il ne fait pas aujourd'hui.

## 4. `Commun/Prefabs` : noms et modèles bruts

### 4a. Prefabs mal nommés (Ewen)

| Prefab | Référencé par |
|---|---|
| `Cylinder.prefab`, `Cylinder.001`, `.002`, `.003`, `.005` | **aucun** |
| `obturator 1.prefab` | **aucun** |
| `Pipe_2` … `Pipe_6` | **aucun** (seul `Pipe_1` est utilisé, dans `SandBox_EwEN`) |

Les noms `Cylinder.00x` viennent de l'export Blender et ne disent pas ce que c'est.

**Proposition** : Ewen identifie chaque pièce et la renomme (par ex. `Tuyau_Coude`, `Tuyau_Droit`, `Obturateur`). Les doublons inutiles sont supprimés. Les autres sont rangés dans `Commun/Prefabs/Tuyauterie/`.

### 4b. Modèles bruts rangés dans Prefabs (Lucas)

| Dossier | Contenu | Référencé par |
|---|---|---|
| `modular_pipes/` | `scene.gltf` + `.bin` + textures + `license.txt` | `SandBox_Lucas.unity` |
| `samsung_galaxy_s3/` | `scene.gltf` + `.bin` + textures | `Commun/Prefabs/Portable.prefab`, lui-même **non référencé** |

Problèmes :
- ce sont des **modèles** (importés via glTFast), pas des prefabs ;
- `samsung_galaxy_s3` reprend une **marque déposée**, et un smartphone de 2012 détonne dans un univers années 50 ;
- le `license.txt` n'existe que pour `modular_pipes`, il faut vérifier la licence du téléphone.

**Proposition** :
- déplacer dans `Commun/Models/TuyauxModulaires/` et `Commun/Models/Telephone/` (ou dans `Salles/Lucas/Models/` s'ils ne servent qu'à Lucas), en gardant le fichier de licence à côté ;
- si `Portable.prefab` est un vestige, le supprimer avec le modèle du téléphone, sinon remplacer le modèle par un objet d'époque sans marque.

### 4c. Remarque : `Porte.prefab`

Il vient d'Adam et est utilisé par les 3 bacs à sable et par `Scene.unity`. Il est bien rangé, rien à faire.

## 5. Fichiers « A supprimer » / « Temporaire » (Adam)

| Fichier | Contenu | Référencé par |
|---|---|---|
| `_Acheron/Autres/A supprimer.cs` | script vide (`class Asupprimer`) | aucun |
| `_Acheron/Commun/Audio/A supprimer.mixer` | mixer vide | aucun |
| `_Acheron/Commun/Scripts/Temporaire.cs` | script vide (`class Temporaire`) | aucun |

Ils ont été ajoutés le 19/11 (commit « Ajout de fichier "A supprimer" ») : Git ne versionne pas les dossiers vides, et ces fichiers servaient à garder l'arborescence.

**Proposition** :
- `Commun/Scripts/Temporaire.cs` : à supprimer, le dossier contient maintenant de vrais scripts (`GestionVannes`, `GestionValeurManometre`).
- `Commun/Audio/A supprimer.mixer` et `Autres/A supprimer.cs` : les remplacer par un fichier `.gitkeep`, qu'Unity ignore (pas de script vide qui compile, pas de mixer dans les menus). Autre option : supprimer le dossier `Autres` s'il n'a pas d'usage prévu.

## 6. Scènes de test dans `Salles/Ewen`

| Scène | Taille | Dernier auteur | Remarque |
|---|---|---|---|
| `SampleScene.unity` (+ dossier `SampleScene/`) | 540 Ko | L962616 (Lucas) | **C'est la seule scène du build** (voir la priorité en haut) |
| `BasicScene.unity` (+ dossier `BasicScene/`) | 18 Ko | L962616 (Lucas) | non utilisée |
| `SandBox_EwEN.unity` | 71 Ko | Ewen | le vrai bac à sable d'Ewen |

Problème : les deux scènes du template sont rangées dans le dossier d'Ewen alors qu'elles ont été commitées par Lucas. Elles proviennent sans doute de l'ancien `Assets/Scenes/` lors de la réorganisation de l'arborescence.

**Proposition** : après avoir corrigé la liste du build, vérifier avec Ewen et Lucas que rien n'y est utile, puis les supprimer avec leurs dossiers d'éclairage. S'il faut les garder, les déplacer dans le dossier de leur auteur.

## 7. README

Les deux versions (EN, ligne 45, et FR, ligne 87) indiquent d'ouvrir `Assets/Scenes/Scene.unity`. Le bon chemin est **`Assets/_Acheron/Scenes/Scene.unity`**.

**Proposition** : corriger les deux lignes. Une section « Arborescence » reprenant la convention ci-dessus serait utile. Propriétaire : Adam.

---

## Récapitulatif

| # | Élément | Qui | Action proposée |
|---|---|---|---|
| ⚠️ | Liste des scènes du build | équipe | mettre `_Acheron/Scenes/Scene.unity` |
| 1 | `Assets/Script/Interaction_Fusibles.cs` | Lucas | déplacer + renommer en `InteractionFusible.cs` |
| 2 | `.glb` à la racine | Lucas | déplacer dans `Commun/Models/`, vérifier la licence du fusible |
| 3a | 12 matériaux `Tiles133C_*` | Baptiste | fusionner en un seul `M_Carrelage` |
| 3b | `New Material.mat` | Ewen | renommer |
| 3c | Textures dans `Materials/` | Baptiste / Ewen | recréer `Commun/Textures/` |
| 3d | Textures 4K `.exr` (~80 Mo) | Ewen | passer en 1K/2K JPG, supprimer l'inutilisé |
| 4a | `Cylinder.00x`, `obturator 1`, `Pipe_2..6` | Ewen | renommer ou supprimer |
| 4b | `modular_pipes`, `samsung_galaxy_s3` | Lucas | déplacer dans `Models/`, revoir le téléphone |
| 5 | `A supprimer.*`, `Temporaire.cs` | Adam | supprimer / remplacer par `.gitkeep` |
| 6 | `SampleScene`, `BasicScene` | Lucas / Ewen | supprimer après correction du build |
| 7 | README | Adam | corriger le chemin de la scène |
