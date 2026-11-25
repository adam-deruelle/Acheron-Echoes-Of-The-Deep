# Carnet technique — Salle du piano

> *Acheron : Echoes of the Deep* — document du joueur **sans casque**.
> Fichier généré automatiquement depuis `` (menu **Acheron > Générer Carnet Piano**). Ne pas modifier à la main.

## Mission

1. Demande à ton coéquipier de te décrire **les 4 anneaux de couleur** de la grosse résistance posée sur l'établi.
2. Calcule la **valeur** de la résistance grâce au code couleur (partie 1).
3. Demande-lui la **couleur du voyant** allumé au-dessus du piano.
4. Lis la **séquence de 4 notes** dans le tableau (partie 2).
5. Guide-le sur le clavier (partie 3) : les touches ne portent aucun nom !

Une fausse note et tout est à recommencer depuis la première note.

## 1. Code couleur des résistances (CEI 60062)

Une résistance à 4 anneaux se lit **de gauche à droite**, l'anneau de tolérance
(séparé des trois autres par un espace plus grand) étant placé **à droite** :

```
   ┌──────────────────────────────┐
───┤  ██  ██  ██           ██     ├───
   └──────────────────────────────┘
      1er 2e  multi-       tolé-
      chif.   plicateur    rance
```

**Valeur = (1er chiffre)(2e chiffre) × multiplicateur**

| Couleur | Chiffre | Multiplicateur | Tolérance |
|---|:---:|:---:|:---:|
| Noir | 0 | × 1 | — |
| Brun | 1 | × 10 | ± 1 % |
| Rouge | 2 | × 100 | ± 2 % |
| Orange | 3 | × 1 000 | — |
| Jaune | 4 | × 10 000 | — |
| Vert | 5 | × 100 000 | ± 0,5 % |
| Bleu | 6 | × 1 000 000 | ± 0,25 % |
| Violet | 7 | × 10 000 000 | ± 0,1 % |
| Gris | 8 | × 100 000 000 | ± 0,05 % |
| Blanc | 9 | × 1 000 000 000 | — |
| Or | — | × 0,1 | ± 5 % |
| Argent | — | × 0,01 | ± 10 % |

**Exemple :** Jaune – Violet – Rouge – Or → 4 7 × 100 = 4 700 Ω = **4,7 kΩ** (± 5 %).

## 2. Tableau des séquences

Ligne = valeur de la résistance, colonne = couleur du voyant.

| Résistance | Voyant ROUGE | Voyant JAUNE | Voyant VERT | Voyant BLEU |
|---|:---:|:---:|:---:|:---:|
| **100 Ω** | Sol - Ré - Sol - Mi | Mi - Sol - Fa - Ré | Fa - Ré - Fa - La | Mi - Do - Sol - Ré |
| **220 Ω** | Si - La - Do - Fa | Do - Si - Fa - Mi | Sol - Do - Mi - Ré | Sol - La - Do - Fa |
| **330 Ω** | La - Fa - Ré - Si | Sol - Mi - Ré - Mi | Sol - Fa - Sol - Ré | Do - La - Fa - Ré |
| **470 Ω** | La - Si - Do - Si | Fa - Mi - Si - Fa | Si - Sol - Mi - Si | Mi - La - Sol - Fa |
| **1 kΩ** | Do - Mi - Fa - Si | Do - Sol - Fa - Mi | Si - Mi - Si - Do | Ré - Sol - Ré - Mi |
| **2,2 kΩ** | La - Do - Si - Fa | Fa - Si - Ré - Fa | Ré - Si - Fa - Sol | Mi - Fa - Sol - Fa |
| **4,7 kΩ** | Fa - Sol - Ré - Mi | Mi - Ré - Si - La | Ré - Fa - Si - Do | Do - Ré - Fa - Ré |

## 3. Le clavier

Le piano a une seule octave. **Seules les 7 touches blanches** jouent une note ;
les touches noires sont décoratives (repères : groupe de 2 noires, puis groupe de 3).

```
+-----------------------------------------+
|    ███   ███    |    ███   ███   ███    |
|    ███   ███    |    ███   ███   ███    |
|    ███   ███    |    ███   ███   ███    |
|     |     |     |     |     |     |     |
| Do  | Ré  | Mi  | Fa  | Sol | La  | Si  |
|     |     |     |     |     |     |     |
+-----------------------------------------+
```

Astuce pour guider : le **Do** est la touche blanche juste à gauche du groupe de **2** touches noires,
le **Fa** celle juste à gauche du groupe de **3** touches noires.

---
*Généré le 19/11/2026 à 11:45.*
