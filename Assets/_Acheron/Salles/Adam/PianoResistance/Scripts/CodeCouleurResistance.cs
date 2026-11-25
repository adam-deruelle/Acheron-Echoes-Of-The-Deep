using System;
using UnityEngine;

namespace Acheron.Enigmes.Piano
{
    /// <summary>
    /// Les couleurs du code des résistances (norme CEI 60062).
    /// La valeur numérique de l'enum = le chiffre associé (Noir = 0 ... Blanc = 9).
    /// </summary>
    public enum CouleurAnneau
    {
        Noir = 0,
        Marron = 1,
        Rouge = 2,
        Orange = 3,
        Jaune = 4,
        Vert = 5,
        Bleu = 6,
        Violet = 7,
        Gris = 8,
        Blanc = 9,
        Or = 10,
        Argent = 11
    }

    /// <summary>
    /// Conversion "valeur en ohms" vers "4 anneaux de couleur" (2 chiffres + multiplicateur + tolérance).
    /// Cette classe est utilisée à la fois par la salle (affichage) et par le carnet (documentation),
    /// ce qui garantit que les deux restent cohérents.
    /// </summary>
    public static class CodeCouleurResistance
    {
        /// <summary>Tolérance utilisée pour toutes les résistances de l'énigme (Or = ±5 %).</summary>
        public const CouleurAnneau AnneauTolerance = CouleurAnneau.Or;

        /// <summary>
        /// Calcule les 4 anneaux d'une résistance.
        /// Exemple : 4700 Ω = 47 × 100 → Jaune, Violet, Rouge, Or.
        /// </summary>
        public static CouleurAnneau[] CalculerAnneaux(float valeurOhms)
        {
            if (valeurOhms < 0.1f)
            {
                throw new ArgumentException("La valeur de la résistance doit être >= 0,1 Ω");
            }

            // On écrit la valeur sous la forme : (2 chiffres) × 10^exposant
            // On calcule en double pour éviter les erreurs d'arrondi des float (ex : log10(1000) = 2,9999).
            int exposant = (int)Math.Floor(Math.Log10(valeurOhms)) - 1;
            int deuxChiffres = (int)Math.Round(valeurOhms / Math.Pow(10, exposant));

            // Corrections si l'arrondi a fait "déborder" (ex : 99,99 → 100)
            if (deuxChiffres >= 100) { deuxChiffres /= 10; exposant++; }
            if (deuxChiffres < 10) { deuxChiffres *= 10; exposant--; }

            CouleurAnneau[] anneaux = new CouleurAnneau[4];
            anneaux[0] = (CouleurAnneau)(deuxChiffres / 10);  // 1er chiffre
            anneaux[1] = (CouleurAnneau)(deuxChiffres % 10);  // 2e chiffre
            anneaux[2] = CouleurMultiplicateur(exposant);      // multiplicateur
            anneaux[3] = AnneauTolerance;                     // tolérance
            return anneaux;
        }

        /// <summary>Couleur de l'anneau multiplicateur pour 10^exposant.</summary>
        private static CouleurAnneau CouleurMultiplicateur(int exposant)
        {
            if (exposant == -1) return CouleurAnneau.Or;      // × 0,1
            if (exposant == -2) return CouleurAnneau.Argent;  // × 0,01
            if (exposant >= 0 && exposant <= 9) return (CouleurAnneau)exposant;

            throw new ArgumentException("Valeur de résistance hors du code couleur (exposant " + exposant + ")");
        }

        /// <summary>Nom affiché de la couleur (avec accents si besoin).</summary>
        public static string NomCouleur(CouleurAnneau couleur)
        {
            return couleur.ToString();
        }

        /// <summary>Couleur Unity utilisée pour peindre l'anneau dans la salle.</summary>
        public static Color CouleurRendu(CouleurAnneau couleur)
        {
            switch (couleur)
            {
                case CouleurAnneau.Noir: return new Color(0.02f, 0.02f, 0.02f);
                case CouleurAnneau.Marron: return new Color(0.36f, 0.18f, 0.06f);
                case CouleurAnneau.Rouge: return new Color(0.85f, 0.05f, 0.05f);
                case CouleurAnneau.Orange: return new Color(1f, 0.45f, 0f);
                case CouleurAnneau.Jaune: return new Color(1f, 0.9f, 0f);
                case CouleurAnneau.Vert: return new Color(0f, 0.6f, 0.15f);
                case CouleurAnneau.Bleu: return new Color(0.05f, 0.2f, 0.9f);
                case CouleurAnneau.Violet: return new Color(0.5f, 0.1f, 0.75f);
                case CouleurAnneau.Gris: return new Color(0.5f, 0.5f, 0.5f);
                case CouleurAnneau.Blanc: return new Color(0.95f, 0.95f, 0.95f);
                case CouleurAnneau.Or: return new Color(0.83f, 0.66f, 0.22f);
                default: return new Color(0.75f, 0.75f, 0.78f); // Argent
            }
        }

        /// <summary>
        /// Texte "chiffre" de la couleur pour le tableau du carnet ("—" si la couleur n'a pas de chiffre).
        /// </summary>
        public static string TexteChiffre(CouleurAnneau couleur)
        {
            int valeur = (int)couleur;
            return valeur <= 9 ? valeur.ToString() : "—";
        }

        /// <summary>Texte "multiplicateur" de la couleur pour le tableau du carnet.</summary>
        public static string TexteMultiplicateur(CouleurAnneau couleur)
        {
            switch (couleur)
            {
                case CouleurAnneau.Noir: return "× 1";
                case CouleurAnneau.Marron: return "× 10";
                case CouleurAnneau.Rouge: return "× 100";
                case CouleurAnneau.Orange: return "× 1 000";
                case CouleurAnneau.Jaune: return "× 10 000";
                case CouleurAnneau.Vert: return "× 100 000";
                case CouleurAnneau.Bleu: return "× 1 000 000";
                case CouleurAnneau.Violet: return "× 10 000 000";
                case CouleurAnneau.Gris: return "× 100 000 000";
                case CouleurAnneau.Blanc: return "× 1 000 000 000";
                case CouleurAnneau.Or: return "× 0,1";
                default: return "× 0,01";
            }
        }

        /// <summary>Texte "tolérance" de la couleur pour le tableau du carnet ("—" si non utilisée).</summary>
        public static string TexteTolerance(CouleurAnneau couleur)
        {
            switch (couleur)
            {
                case CouleurAnneau.Marron: return "± 1 %";
                case CouleurAnneau.Rouge: return "± 2 %";
                case CouleurAnneau.Vert: return "± 0,5 %";
                case CouleurAnneau.Bleu: return "± 0,25 %";
                case CouleurAnneau.Violet: return "± 0,1 %";
                case CouleurAnneau.Gris: return "± 0,05 %";
                case CouleurAnneau.Or: return "± 5 %";
                case CouleurAnneau.Argent: return "± 10 %";
                default: return "—";
            }
        }

        /// <summary>
        /// Écrit une valeur en ohms de façon lisible, à la française.
        /// Exemples : 470 → "470 Ω", 1000 → "1 kΩ", 2200 → "2,2 kΩ".
        /// </summary>
        public static string FormaterValeur(float valeurOhms)
        {
            if (valeurOhms >= 1000000f) return FormaterNombre(valeurOhms / 1000000f) + " MΩ";
            if (valeurOhms >= 1000f) return FormaterNombre(valeurOhms / 1000f) + " kΩ";
            return FormaterNombre(valeurOhms) + " Ω";
        }

        private static string FormaterNombre(float nombre)
        {
            // "0.##" = au plus 2 décimales, puis virgule française
            return nombre.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture).Replace('.', ',');
        }
    }
}
