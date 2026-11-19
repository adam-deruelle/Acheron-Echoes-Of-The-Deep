using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Acheron.Enigmes.Piano
{
    /// <summary>
    /// Une touche blanche du piano, appuyable "au doigt" (poke) avec la main ou la manette (XR Interaction Toolkit).
    ///
    /// Organisation attendue :
    ///   Touche_Do              ← ce script + XRSimpleInteractable + XRPokeFilter + BoxCollider + AudioSource
    ///     └─ Visuel            ← le modèle 3D qui s'enfonce (sans collider)
    ///
    /// Le collider reste immobile (sinon le calcul du poke se dérègle) : seul le "Visuel" bouge.
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    [RequireComponent(typeof(XRPokeFilter))]
    [RequireComponent(typeof(AudioSource))]
    public class TouchePiano : MonoBehaviour
    {
        [Header("Note")]
        [Tooltip("La note jouée par cette touche")]
        [SerializeField] private NoteBlanche note = NoteBlanche.Do;

        [Header("Animation")]
        [Tooltip("Partie visible de la touche qui s'enfonce (enfant sans collider)")]
        [SerializeField] private Transform visuel;

        [Tooltip("De combien la touche s'enfonce, en mètres")]
        [SerializeField] private float profondeurEnfoncement = 0.012f;

        [Tooltip("Vitesse de l'animation d'enfoncement / de remontée")]
        [SerializeField] private float vitesseAnimation = 25f;

        [Header("Interaction")]
        [Tooltip("Si coché, seul un appui au doigt (Poke Interactor) compte. " +
                 "Évite qu'on joue les touches de loin avec le rayon des manettes.")]
        [SerializeField] private bool pokeUniquement = true;

        [Header("Événement")]
        [Tooltip("Déclenché à chaque appui sur la touche (le gestionnaire de l'énigme s'y abonne)")]
        public UnityEvent<NoteBlanche> OnToucheAppuyee = new UnityEvent<NoteBlanche>();

        /// <summary>La note de cette touche.</summary>
        public NoteBlanche Note => note;

        private XRSimpleInteractable interactable;
        private AudioSource sourceAudio;
        private Vector3 positionReposVisuel;
        private bool estEnfoncee;

        private void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            sourceAudio = GetComponent<AudioSource>();

            if (visuel != null)
            {
                positionReposVisuel = visuel.localPosition;
            }

            // Son généré par code (voir GenerateurSon)
            sourceAudio.playOnAwake = false;
            sourceAudio.clip = GenerateurSon.Note(note);
        }

        private void OnEnable()
        {
            // Avec le XRPokeFilter, "select" = la touche a été enfoncée suffisamment profond
            interactable.selectEntered.AddListener(QuandSelectionDebut);
            interactable.selectExited.AddListener(QuandSelectionFin);
        }

        private void OnDisable()
        {
            interactable.selectEntered.RemoveListener(QuandSelectionDebut);
            interactable.selectExited.RemoveListener(QuandSelectionFin);
            estEnfoncee = false;
        }

        private void Update()
        {
            if (visuel == null) return;

            // Position visée : enfoncée ou au repos, puis on s'en rapproche en douceur
            Vector3 cible = estEnfoncee ? positionReposVisuel + Vector3.down * profondeurEnfoncement : positionReposVisuel;
            visuel.localPosition = Vector3.Lerp(visuel.localPosition, cible, vitesseAnimation * Time.deltaTime);
        }

        private void QuandSelectionDebut(SelectEnterEventArgs args)
        {
            if (pokeUniquement && !(args.interactorObject is XRPokeInteractor))
            {
                return; // appui ignoré : ce n'est pas un doigt / une pointe de manette
            }

            Appuyer();
        }

        private void QuandSelectionFin(SelectExitEventArgs args)
        {
            // On ne relâche la touche que si plus rien ne l'appuie
            if (!interactable.isSelected)
            {
                estEnfoncee = false;
            }
        }

        /// <summary>
        /// Appuie sur la touche : animation + son + événement.
        /// Public pour pouvoir être appelée depuis un autre script ou pour les tests.
        /// </summary>
        public void Appuyer()
        {
            estEnfoncee = true;
            sourceAudio.PlayOneShot(sourceAudio.clip);
            OnToucheAppuyee.Invoke(note);
        }

        /// <summary>Relâche la touche (utile pour les tests sans casque).</summary>
        public void Relacher()
        {
            estEnfoncee = false;
        }

        // Test sans casque : en Play, clic droit sur le composant dans l'Inspector > "Test : appuyer"
        [ContextMenu("Test : appuyer sur la touche")]
        private void TestAppuyer()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Lance le mode Play pour tester la touche.");
                return;
            }

            Appuyer();
            Invoke(nameof(Relacher), 0.2f);
        }
    }
}
