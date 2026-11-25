using UnityEngine;

namespace Acheron.Enigmes.Piano
{
    /// <summary>
    /// Les 7 notes des touches blanches du piano (une octave, de Do à Si).
    /// L'ordre correspond à la position de la touche de gauche à droite.
    /// (Pas d'accent dans les noms C# pour éviter les soucis d'encodage : "Re" = Ré.)
    /// </summary>
    public enum NoteBlanche
    {
        Do = 0,
        Re = 1,
        Mi = 2,
        Fa = 3,
        Sol = 4,
        La = 5,
        Si = 6
    }

    /// <summary>
    /// Les 4 couleurs possibles du voyant au-dessus du piano.
    /// </summary>
    public enum CouleurVoyant
    {
        Rouge = 0,
        Jaune = 1,
        Vert = 2,
        Bleu = 3
    }

    /// <summary>
    /// Petites fonctions utilitaires sur les notes et les couleurs du voyant.
    /// </summary>
    public static class OutilsPiano
    {
        /// <summary>Nombre de touches blanches sur le clavier.</summary>
        public const int NombreDeNotes = 7;

        /// <summary>Nombre de couleurs possibles pour le voyant.</summary>
        public const int NombreDeCouleurs = 4;

        /// <summary>Nom affiché de la note (avec accent).</summary>
        public static string NomNote(NoteBlanche note)
        {
            switch (note)
            {
                case NoteBlanche.Do: return "Do";
                case NoteBlanche.Re: return "Ré";
                case NoteBlanche.Mi: return "Mi";
                case NoteBlanche.Fa: return "Fa";
                case NoteBlanche.Sol: return "Sol";
                case NoteBlanche.La: return "La";
                default: return "Si";
            }
        }

        /// <summary>Fréquence en Hz de la note (octave 4, La = 440 Hz).</summary>
        public static float Frequence(NoteBlanche note)
        {
            switch (note)
            {
                case NoteBlanche.Do: return 261.63f;
                case NoteBlanche.Re: return 293.66f;
                case NoteBlanche.Mi: return 329.63f;
                case NoteBlanche.Fa: return 349.23f;
                case NoteBlanche.Sol: return 392.00f;
                case NoteBlanche.La: return 440.00f;
                default: return 493.88f;
            }
        }

        /// <summary>Nom affiché de la couleur du voyant.</summary>
        public static string NomCouleur(CouleurVoyant couleur)
        {
            switch (couleur)
            {
                case CouleurVoyant.Rouge: return "Rouge";
                case CouleurVoyant.Jaune: return "Jaune";
                case CouleurVoyant.Vert: return "Vert";
                default: return "Bleu";
            }
        }

        /// <summary>Couleur Unity utilisée pour allumer le voyant.</summary>
        public static Color CouleurRendu(CouleurVoyant couleur)
        {
            switch (couleur)
            {
                case CouleurVoyant.Rouge: return new Color(1f, 0.08f, 0.05f);
                case CouleurVoyant.Jaune: return new Color(1f, 0.85f, 0.1f);
                case CouleurVoyant.Vert: return new Color(0.1f, 1f, 0.25f);
                default: return new Color(0.15f, 0.4f, 1f);
            }
        }

        /// <summary>Transforme une séquence en texte lisible, ex : "Do - Mi - Sol - Ré".</summary>
        public static string SequenceEnTexte(NoteBlanche[] sequence)
        {
            if (sequence == null || sequence.Length == 0) return "(vide)";

            string texte = NomNote(sequence[0]);
            for (int i = 1; i < sequence.Length; i++)
            {
                texte += " - " + NomNote(sequence[i]);
            }
            return texte;
        }
    }
}
