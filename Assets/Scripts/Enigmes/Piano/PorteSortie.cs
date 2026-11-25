using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Acheron.Enigmes.Piano
{
    /// <summary>
    /// Porte de sortie coulissante : le "battant" glisse d'un déplacement donné quand on l'ouvre.
    /// Réutilisable pour d'autres salles (aucun lien avec le piano).
    /// </summary>
    public class PorteSortie : MonoBehaviour
    {
        [Tooltip("La partie de la porte qui bouge (avec son collider, qui bloque le passage tant que c'est fermé)")]
        [SerializeField] private Transform battant;

        [Tooltip("Déplacement du battant à l'ouverture, en mètres, dans le repère local de la porte")]
        [SerializeField] private Vector3 deplacementOuverture = new Vector3(1.1f, 0f, 0f);

        [Tooltip("Durée de l'ouverture en secondes")]
        [SerializeField] private float dureeOuverture = 2f;

        [Tooltip("Son joué à l'ouverture (optionnel)")]
        [SerializeField] private AudioSource sonOuverture;

        [Tooltip("Déclenché quand la porte a fini de s'ouvrir")]
        public UnityEvent OnPorteOuverte = new UnityEvent();

        /// <summary>Vrai si la porte est ouverte ou en train de s'ouvrir.</summary>
        public bool EstOuverte { get; private set; }

        /// <summary>Ouvre la porte (ne fait rien si elle est déjà ouverte).</summary>
        public void Ouvrir()
        {
            if (EstOuverte || battant == null) return;

            EstOuverte = true;
            if (sonOuverture != null)
            {
                // Pas de son assigné : son provisoire généré par code (le buzzer joué plus grave = grondement)
                if (sonOuverture.clip == null) sonOuverture.clip = GenerateurSon.Buzzer();
                sonOuverture.Play();
            }
            StartCoroutine(RoutineOuverture());
        }

        private IEnumerator RoutineOuverture()
        {
            Vector3 depart = battant.localPosition;
            Vector3 arrivee = depart + deplacementOuverture;
            float temps = 0f;

            while (temps < dureeOuverture)
            {
                temps += Time.deltaTime;
                // SmoothStep : démarre et s'arrête en douceur
                float progression = Mathf.SmoothStep(0f, 1f, temps / dureeOuverture);
                battant.localPosition = Vector3.Lerp(depart, arrivee, progression);
                yield return null;
            }

            battant.localPosition = arrivee;
            OnPorteOuverte.Invoke();
        }

        [ContextMenu("Test : ouvrir la porte")]
        private void TestOuvrir()
        {
            if (Application.isPlaying) Ouvrir();
            else Debug.LogWarning("Lance le mode Play pour tester la porte.");
        }
    }
}
