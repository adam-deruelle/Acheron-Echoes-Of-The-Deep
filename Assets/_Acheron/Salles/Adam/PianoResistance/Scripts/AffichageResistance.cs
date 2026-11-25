using UnityEngine;

namespace Acheron.Enigmes.Piano
{
    /// <summary>
    /// Peint les 4 anneaux de la résistance géante selon une valeur en ohms.
    /// Ordre des anneaux : 1er chiffre, 2e chiffre, multiplicateur, tolérance (l'anneau un peu à l'écart).
    /// Quand on remplacera la résistance par un vrai modèle 3D, il suffira de glisser
    /// les 4 Renderers des anneaux dans le tableau "anneaux".
    /// </summary>
    public class AffichageResistance : MonoBehaviour
    {
        [Tooltip("Les 4 anneaux, dans l'ordre de lecture (le 4e = tolérance)")]
        [SerializeField] private Renderer[] anneaux = new Renderer[4];

        /// <summary>Valeur actuellement affichée, en ohms.</summary>
        public float ValeurAffichee { get; private set; }

        /// <summary>Affiche une valeur de résistance (ex : 2200 → Rouge, Rouge, Rouge, Or).</summary>
        public void AfficherValeur(float valeurOhms)
        {
            ValeurAffichee = valeurOhms;
            CouleurAnneau[] couleurs = CodeCouleurResistance.CalculerAnneaux(valeurOhms);

            for (int i = 0; i < anneaux.Length && i < couleurs.Length; i++)
            {
                if (anneaux[i] == null) continue;

                // "material" (et pas "sharedMaterial") crée une copie propre à cet anneau :
                // on ne modifie donc pas le matériau partagé dans le projet.
                Material materiau = anneaux[i].material;
                materiau.color = CodeCouleurResistance.CouleurRendu(couleurs[i]);

                // Les anneaux Or/Argent un peu brillants pour les distinguer de Jaune/Gris
                bool metallique = couleurs[i] == CouleurAnneau.Or || couleurs[i] == CouleurAnneau.Argent;
                if (materiau.HasProperty("_Smoothness"))
                {
                    materiau.SetFloat("_Smoothness", metallique ? 0.85f : 0.3f);
                }
            }
        }
    }
}
