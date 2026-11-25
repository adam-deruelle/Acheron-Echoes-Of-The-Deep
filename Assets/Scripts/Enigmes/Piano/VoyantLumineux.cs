using System.Collections;
using UnityEngine;

namespace Acheron.Enigmes.Piano
{
    /// <summary>
    /// Le voyant au-dessus du piano : affiche une couleur (Rouge, Jaune, Vert, Bleu)
    /// et peut clignoter en rouge pour signaler une erreur.
    ///
    /// La couleur passe par l'émission du matériau (visible partout, même sur Quest en autonome).
    /// La Light optionnelle ajoute un halo, mais elle ne s'affiche que sur PC :
    /// le profil URP "Performance" du Quest désactive les lumières additionnelles.
    /// </summary>
    public class VoyantLumineux : MonoBehaviour
    {
        [Tooltip("La partie lumineuse du voyant (son matériau doit être URP/Lit)")]
        [SerializeField] private Renderer ampoule;

        [Tooltip("Lumière optionnelle (halo autour du voyant, visible sur PC uniquement)")]
        [SerializeField] private Light lumiere;

        [Tooltip("Force de l'émission du matériau")]
        [SerializeField] private float intensiteEmission = 2f;

        [Header("Flash d'erreur")]
        [SerializeField] private int nombreDeClignotements = 3;
        [SerializeField] private float dureeClignotement = 0.12f;

        /// <summary>Couleur actuelle du voyant.</summary>
        public CouleurVoyant CouleurActuelle { get; private set; }

        private Material materiauAmpoule;
        private Coroutine flashEnCours;

        private static readonly int IdCouleurEmission = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            if (ampoule != null)
            {
                // Copie du matériau propre à ce voyant
                materiauAmpoule = ampoule.material;
                materiauAmpoule.EnableKeyword("_EMISSION");
            }
        }

        /// <summary>Allume le voyant dans la couleur demandée.</summary>
        public void AfficherCouleur(CouleurVoyant couleur)
        {
            CouleurActuelle = couleur;

            // Si un flash était en cours, on l'arrête pour ne pas écraser la nouvelle couleur
            if (flashEnCours != null)
            {
                StopCoroutine(flashEnCours);
                flashEnCours = null;
            }

            AppliquerCouleur(OutilsPiano.CouleurRendu(couleur), true);
        }

        /// <summary>Fait clignoter le voyant en rouge (erreur), puis revient à sa couleur.</summary>
        public void FlashErreur()
        {
            if (flashEnCours != null) StopCoroutine(flashEnCours);
            flashEnCours = StartCoroutine(RoutineFlash());
        }

        private IEnumerator RoutineFlash()
        {
            Color rouge = OutilsPiano.CouleurRendu(CouleurVoyant.Rouge);

            for (int i = 0; i < nombreDeClignotements; i++)
            {
                AppliquerCouleur(rouge * 1.5f, true);   // rouge vif
                yield return new WaitForSeconds(dureeClignotement);
                AppliquerCouleur(Color.black, false);   // éteint
                yield return new WaitForSeconds(dureeClignotement);
            }

            // Retour à la couleur de l'énigme
            AppliquerCouleur(OutilsPiano.CouleurRendu(CouleurActuelle), true);
            flashEnCours = null;
        }

        private void AppliquerCouleur(Color couleur, bool allume)
        {
            if (materiauAmpoule != null)
            {
                materiauAmpoule.color = allume ? couleur : Color.black;
                materiauAmpoule.SetColor(IdCouleurEmission, allume ? couleur * intensiteEmission : Color.black);
            }

            if (lumiere != null)
            {
                lumiere.color = couleur;
                lumiere.enabled = allume;
            }
        }
    }
}
