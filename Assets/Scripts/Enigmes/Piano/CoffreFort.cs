using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Acheron.Enigmes.Piano
{
    /// <summary>
    /// Petit coffre-fort qu'on ouvre en attrapant sa poignée.
    ///
    /// Déroulement :
    ///   1. Fermé et verrouillé : la poignée ne réagit pas.
    ///   2. Deverrouiller() (branché sur GestionnaireEnigmePiano.OnReussite) : "clac" du verrou,
    ///      la porte s'entrouvre légèrement pour montrer qu'elle est libre.
    ///   3. Le joueur attrape la poignée (grip / pincement) et tire : la porte suit sa main
    ///      en tournant autour de la charnière, entre 0° et "angleMax".
    ///   4. Dès que la porte est assez ouverte, le contenu (demi-plaque) peut être attrapé.
    ///
    /// Organisation attendue :
    ///   CoffreFort                 ← ce script + AudioSource
    ///     ├─ Caisse ...            ← les parois (fixes)
    ///     ├─ Charniere_Porte       ← pivot de la porte, placé SUR l'axe de la charnière
    ///     │    └─ Porte
    ///     │         ├─ Panneau ... ← la porte visible (avec collider : bloque l'accès tant que c'est fermé)
    ///     │         └─ Poignee     ← BoxCollider + XRSimpleInteractable (ce qu'on attrape)
    ///     └─ DemiPlaque_Piano      ← le contenu
    /// </summary>
    public class CoffreFort : MonoBehaviour
    {
        [Header("Porte")]
        [Tooltip("Pivot de la porte (objet vide placé sur l'axe de la charnière)")]
        [SerializeField] private Transform charnierePorte;

        [Tooltip("La poignée à attraper (XRSimpleInteractable sur l'objet Poignee)")]
        [SerializeField] private XRSimpleInteractable poignee;

        [Tooltip("Ouverture maximale en degrés (positif = vers l'extérieur quand la charnière est à gauche)")]
        [SerializeField] private float angleMax = 110f;

        [Tooltip("Angle dont la porte s'entrouvre toute seule au déverrouillage")]
        [SerializeField] private float angleEntrouvert = 8f;

        [Tooltip("Angle à partir duquel le contenu peut être attrapé")]
        [SerializeField] private float angleContenuAccessible = 45f;

        [Tooltip("Vitesse à laquelle la porte rejoint la main (plus grand = plus réactif)")]
        [SerializeField] private float vitesseSuivi = 12f;

        [Header("Contenu")]
        [Tooltip("Objets débloqués quand la porte est assez ouverte")]
        [SerializeField] private DemiPlaque[] contenu = new DemiPlaque[0];

        [Header("Son")]
        [Tooltip("Source du son de déverrouillage. Sans clip assigné, un son généré par code est utilisé.")]
        [SerializeField] private AudioSource sourceAudio;

        [Header("Événements")]
        [Tooltip("Le verrou vient de sauter (énigme réussie)")]
        public UnityEvent OnCoffreDeverrouille = new UnityEvent();

        [Tooltip("La porte est assez ouverte pour prendre le contenu (une seule fois)")]
        public UnityEvent OnCoffreOuvert = new UnityEvent();

        /// <summary>Vrai une fois le verrou sauté.</summary>
        public bool EstDeverrouille { get; private set; }

        /// <summary>Vrai une fois la porte assez ouverte pour prendre le contenu.</summary>
        public bool EstOuvert { get; private set; }

        /// <summary>Angle actuel de la porte, en degrés (0 = fermée).</summary>
        public float AngleActuel { get; private set; }

        private Quaternion rotationFermee;   // rotation locale de la charnière porte fermée
        private Transform mainQuiTient;      // point d'attache de la main/manette qui tient la poignée
        private float decalageAngle;         // évite que la porte "saute" au moment où on attrape
        private float angleCible;

        private void Awake()
        {
            if (charnierePorte != null) rotationFermee = charnierePorte.localRotation;
        }

        private void OnEnable()
        {
            if (poignee == null) return;
            poignee.selectEntered.AddListener(QuandPoigneeAttrapee);
            poignee.selectExited.AddListener(QuandPoigneeLachee);
        }

        private void OnDisable()
        {
            if (poignee == null) return;
            poignee.selectEntered.RemoveListener(QuandPoigneeAttrapee);
            poignee.selectExited.RemoveListener(QuandPoigneeLachee);
        }

        private void Start()
        {
            // Au départ : verrouillé, poignée inactive, contenu inaccessible
            if (poignee != null) poignee.enabled = false;
            foreach (DemiPlaque plaque in contenu)
            {
                if (plaque != null) plaque.Verrouiller(true);
            }
        }

        private void Update()
        {
            if (charnierePorte == null) return;

            // Pendant qu'on tient la poignée, la cible suit l'angle de la main autour de la charnière
            if (mainQuiTient != null)
            {
                angleCible = Mathf.Clamp(AngleDeLaMain() + decalageAngle, 0f, angleMax);
            }

            // La porte rejoint la cible en douceur
            AngleActuel = Mathf.Lerp(AngleActuel, angleCible, vitesseSuivi * Time.deltaTime);
            charnierePorte.localRotation = rotationFermee * Quaternion.Euler(0f, AngleActuel, 0f);

            if (!EstOuvert && AngleActuel >= angleContenuAccessible)
            {
                LibererContenu();
            }
        }

        /// <summary>
        /// Fait sauter le verrou (à brancher sur OnReussite de l'énigme).
        /// Le joueur doit ensuite ouvrir la porte avec la poignée.
        /// </summary>
        public void Deverrouiller()
        {
            if (EstDeverrouille) return;
            EstDeverrouille = true;

            if (sourceAudio != null)
            {
                if (sourceAudio.clip == null) sourceAudio.clip = GenerateurSon.Deverrouillage();
                sourceAudio.Play();
            }

            if (poignee != null) poignee.enabled = true;
            StartCoroutine(Entrouvrir());
            OnCoffreDeverrouille.Invoke();
        }

        /// <summary>Ancien nom gardé pour les branchements existants : déverrouille le coffre.</summary>
        public void Ouvrir()
        {
            Deverrouiller();
        }

        private IEnumerator Entrouvrir()
        {
            // On attend le "clac" du verrou avant que la porte bouge
            yield return new WaitForSeconds(0.3f);
            if (mainQuiTient == null) angleCible = Mathf.Max(angleCible, angleEntrouvert);
        }

        private void QuandPoigneeAttrapee(SelectEnterEventArgs args)
        {
            mainQuiTient = args.interactorObject.GetAttachTransform(poignee);
            // Décalage = différence entre l'angle de la porte et celui de la main au moment de la prise
            decalageAngle = AngleActuel - AngleDeLaMain();
        }

        private void QuandPoigneeLachee(SelectExitEventArgs args)
        {
            // Si une autre main tient encore la poignée, on continue avec elle
            if (poignee.isSelected)
            {
                mainQuiTient = poignee.interactorsSelecting[0].GetAttachTransform(poignee);
                decalageAngle = AngleActuel - AngleDeLaMain();
                return;
            }

            mainQuiTient = null;
            angleCible = AngleActuel; // la porte reste là où on l'a lâchée
        }

        /// <summary>
        /// Angle (en degrés) de la main autour de l'axe de la charnière, mesuré depuis la position "porte fermée".
        /// </summary>
        private float AngleDeLaMain()
        {
            Transform parent = charnierePorte.parent;
            // Position de la main vue depuis la charnière, dans le repère "porte fermée"
            Vector3 mainLocale = parent.InverseTransformPoint(mainQuiTient.position) - charnierePorte.localPosition;
            Vector3 v = Quaternion.Inverse(rotationFermee) * mainLocale;
            // Une rotation de +a autour de Y envoie l'axe X vers (cos a, 0, -sin a)
            return Mathf.Atan2(-v.z, v.x) * Mathf.Rad2Deg;
        }

        private void LibererContenu()
        {
            EstOuvert = true;
            foreach (DemiPlaque plaque in contenu)
            {
                if (plaque != null) plaque.Deverrouiller();
            }
            OnCoffreOuvert.Invoke();
        }

        // Tests sans casque : en Play, clic droit sur le composant
        [ContextMenu("Test : déverrouiller le coffre")]
        private void TestDeverrouiller()
        {
            if (Application.isPlaying) Deverrouiller();
            else Debug.LogWarning("Lance le mode Play pour tester le coffre.");
        }

        [ContextMenu("Test : ouvrir la porte en grand")]
        private void TestOuvrirEnGrand()
        {
            if (!Application.isPlaying) { Debug.LogWarning("Lance le mode Play pour tester le coffre."); return; }
            Deverrouiller();
            angleCible = angleMax;
        }
    }
}
