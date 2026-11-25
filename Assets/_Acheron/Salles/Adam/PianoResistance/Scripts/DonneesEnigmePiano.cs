using System;
using System.Collections.Generic;
using UnityEngine;

namespace Acheron.Enigmes.Piano
{
    /// <summary>
    /// Une ligne du tableau : une valeur de résistance et ses 4 séquences (une par couleur du voyant).
    /// </summary>
    [Serializable]
    public class LigneResistance
    {
        [Tooltip("Valeur de la résistance en ohms (ex : 2200 pour 2,2 kΩ). Doit s'écrire avec 2 chiffres significatifs.")]
        public float valeurOhms = 100f;

        [Tooltip("Séquence à jouer si le voyant est ROUGE")]
        public NoteBlanche[] sequenceRouge = new NoteBlanche[DonneesEnigmePiano.LongueurSequence];

        [Tooltip("Séquence à jouer si le voyant est JAUNE")]
        public NoteBlanche[] sequenceJaune = new NoteBlanche[DonneesEnigmePiano.LongueurSequence];

        [Tooltip("Séquence à jouer si le voyant est VERT")]
        public NoteBlanche[] sequenceVert = new NoteBlanche[DonneesEnigmePiano.LongueurSequence];

        [Tooltip("Séquence à jouer si le voyant est BLEU")]
        public NoteBlanche[] sequenceBleu = new NoteBlanche[DonneesEnigmePiano.LongueurSequence];

        /// <summary>Renvoie la séquence correspondant à la couleur du voyant.</summary>
        public NoteBlanche[] GetSequence(CouleurVoyant couleur)
        {
            switch (couleur)
            {
                case CouleurVoyant.Rouge: return sequenceRouge;
                case CouleurVoyant.Jaune: return sequenceJaune;
                case CouleurVoyant.Vert: return sequenceVert;
                default: return sequenceBleu;
            }
        }
    }

    /// <summary>
    /// Tableau de l'énigme : 7 valeurs de résistance × 4 couleurs de voyant = 28 séquences de notes.
    /// C'est LA source de vérité : la salle et le carnet papier (menu Acheron > Générer Carnet Piano)
    /// sont construits à partir de cet asset.
    /// </summary>
    [CreateAssetMenu(fileName = "DonneesEnigmePiano", menuName = "Acheron/Énigme Piano/Données de l'énigme")]
    public class DonneesEnigmePiano : ScriptableObject
    {
        /// <summary>Nombre de notes dans chaque séquence.</summary>
        public const int LongueurSequence = 4;

        [Tooltip("Une ligne par valeur de résistance possible")]
        [SerializeField] private List<LigneResistance> lignes = new List<LigneResistance>();

        /// <summary>Nombre de valeurs de résistance dans le tableau.</summary>
        public int NombreDeValeurs => lignes.Count;

        /// <summary>Renvoie la ligne n° index (valeur + 4 séquences).</summary>
        public LigneResistance GetLigne(int index)
        {
            return lignes[index];
        }

        /// <summary>Renvoie la séquence à jouer pour une valeur de résistance et une couleur de voyant.</summary>
        public NoteBlanche[] GetSequence(int indexValeur, CouleurVoyant couleur)
        {
            return lignes[indexValeur].GetSequence(couleur);
        }

        /// <summary>
        /// Vérifie que le tableau est utilisable. Renvoie false et un message si un problème est trouvé.
        /// </summary>
        public bool EstValide(out string erreur)
        {
            if (lignes == null || lignes.Count == 0)
            {
                erreur = "Le tableau est vide.";
                return false;
            }

            for (int i = 0; i < lignes.Count; i++)
            {
                LigneResistance ligne = lignes[i];

                // Valeur dans le code couleur ?
                try
                {
                    CodeCouleurResistance.CalculerAnneaux(ligne.valeurOhms);
                }
                catch (ArgumentException e)
                {
                    erreur = "Ligne " + i + " : " + e.Message;
                    return false;
                }

                // Deux lignes avec la même valeur → ambigu pour le joueur carnet
                for (int j = 0; j < i; j++)
                {
                    if (Mathf.Approximately(lignes[j].valeurOhms, ligne.valeurOhms))
                    {
                        erreur = "Les lignes " + j + " et " + i + " ont la même valeur (" + ligne.valeurOhms + " Ω).";
                        return false;
                    }
                }

                // Chaque séquence doit contenir exactement LongueurSequence notes
                for (int c = 0; c < OutilsPiano.NombreDeCouleurs; c++)
                {
                    CouleurVoyant couleur = (CouleurVoyant)c;
                    NoteBlanche[] sequence = ligne.GetSequence(couleur);
                    if (sequence == null || sequence.Length != LongueurSequence)
                    {
                        erreur = "Ligne " + i + " (" + CodeCouleurResistance.FormaterValeur(ligne.valeurOhms) + "), couleur "
                                 + OutilsPiano.NomCouleur(couleur) + " : la séquence doit contenir " + LongueurSequence + " notes.";
                        return false;
                    }
                }
            }

            erreur = "";
            return true;
        }

        // Appelée par Unity à chaque modification dans l'Inspector : on prévient tout de suite en cas d'erreur.
        private void OnValidate()
        {
            if (!EstValide(out string erreur))
            {
                Debug.LogWarning("[Énigme Piano] Données invalides dans '" + name + "' : " + erreur, this);
            }
        }

        // Appelée par Unity quand on crée l'asset ou qu'on fait "Reset" dans l'Inspector.
        private void Reset()
        {
            RemplirParDefaut();
        }

        /// <summary>
        /// Remplit le tableau avec les 28 séquences d'origine.
        /// Accessible par clic droit sur l'asset dans l'Inspector (menu "⋮").
        /// </summary>
        [ContextMenu("Remplir avec les séquences par défaut")]
        public void RemplirParDefaut()
        {
            const NoteBlanche Do = NoteBlanche.Do, Re = NoteBlanche.Re, Mi = NoteBlanche.Mi, Fa = NoteBlanche.Fa,
                              Sol = NoteBlanche.Sol, La = NoteBlanche.La, Si = NoteBlanche.Si;

            lignes = new List<LigneResistance>
            {
                //          valeur   Rouge                      Jaune                     Vert                      Bleu
                CreerLigne(100f,  new[] { Sol, Re, Sol, Mi }, new[] { Mi, Sol, Fa, Re }, new[] { Fa, Re, Fa, La }, new[] { Mi, Do, Sol, Re }),
                CreerLigne(220f,  new[] { Si, La, Do, Fa },   new[] { Do, Si, Fa, Mi },  new[] { Sol, Do, Mi, Re }, new[] { Sol, La, Do, Fa }),
                CreerLigne(330f,  new[] { La, Fa, Re, Si },   new[] { Sol, Mi, Re, Mi }, new[] { Sol, Fa, Sol, Re }, new[] { Do, La, Fa, Re }),
                CreerLigne(470f,  new[] { La, Si, Do, Si },   new[] { Fa, Mi, Si, Fa },  new[] { Si, Sol, Mi, Si }, new[] { Mi, La, Sol, Fa }),
                CreerLigne(1000f, new[] { Do, Mi, Fa, Si },   new[] { Do, Sol, Fa, Mi }, new[] { Si, Mi, Si, Do },  new[] { Re, Sol, Re, Mi }),
                CreerLigne(2200f, new[] { La, Do, Si, Fa },   new[] { Fa, Si, Re, Fa },  new[] { Re, Si, Fa, Sol }, new[] { Mi, Fa, Sol, Fa }),
                CreerLigne(4700f, new[] { Fa, Sol, Re, Mi },  new[] { Mi, Re, Si, La },  new[] { Re, Fa, Si, Do },  new[] { Do, Re, Fa, Re }),
            };

#if UNITY_EDITOR
            // Indique à Unity que l'asset a changé (pour qu'il soit bien sauvegardé)
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private static LigneResistance CreerLigne(float valeurOhms, NoteBlanche[] rouge, NoteBlanche[] jaune,
                                                  NoteBlanche[] vert, NoteBlanche[] bleu)
        {
            return new LigneResistance
            {
                valeurOhms = valeurOhms,
                sequenceRouge = rouge,
                sequenceJaune = jaune,
                sequenceVert = vert,
                sequenceBleu = bleu
            };
        }
    }
}
