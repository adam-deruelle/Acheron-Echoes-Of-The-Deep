using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Acheron.Enigmes.Piano
{
    /// <summary>
    /// Porte battante qu'on ouvre en attrapant une de ses poignées (grip / pincement) et en tirant ou poussant :
    /// la porte tourne autour de sa charnière en suivant la main, entre "angleMin" et "angleMax".
    /// Lâchée, elle reste où elle est.
    ///
    /// Organisation attendue :
    ///   Porte                      ← ce script
    ///     ├─ Cadre ...             ← encadrement fixe (des deux côtés du mur)
    ///     └─ Charniere             ← pivot, placé SUR l'axe de la charnière
    ///          └─ Battant          ← XRSimpleInteractable (colliders = ceux des poignées uniquement)
    ///               ├─ Panneau     ← la porte visible + collider qui bloque le passage
    ///               ├─ Poignee_Salle   ← BoxCollider (on l'attrape côté salle)
    ///               └─ Poignee_Couloir ← BoxCollider (on l'attrape côté couloir)
    /// </summary>
    public class PorteBattante : MonoBehaviour
    {
        [Header("Porte")]
        [Tooltip("Pivot de la porte (objet vide placé sur l'axe de la charnière)")]
        [SerializeField] private Transform charniere;

        [Tooltip("L'interactable des poignées (sur le battant, avec les colliders des deux poignées)")]
        [SerializeField] private XRSimpleInteractable poignees;

        [Tooltip("Angle minimal en degrés (0 = fermée)")]
        [SerializeField] private float angleMin = 0f;

        [Tooltip("Angle maximal en degrés (sens positif = rotation +Y de la charnière)")]
        [SerializeField] private float angleMax = 100f;

        [Tooltip("Vitesse à laquelle la porte rejoint la main (plus grand = plus réactif)")]
        [SerializeField] private float vitesseSuivi = 12f;

        [Header("Verrou")]
        [Tooltip("Verrouillée = les poignées ne réagissent pas")]
        [SerializeField] private bool verrouilleeAuDemarrage = false;

        [Header("Événements")]
        [Tooltip("La porte vient de quitter la position fermée (une seule fois)")]
        public UnityEvent OnPremiereOuverture = new UnityEvent();

        /// <summary>Angle actuel de la porte, en degrés (0 = fermée).</summary>
        public float AngleActuel { get; private set; }

        /// <summary>Vrai si les poignées sont bloquées.</summary>
        public bool EstVerrouillee { get; private set; }

        private Quaternion rotationFermee;   // rotation locale de la charnière porte fermée
        private Transform mainQuiTient;      // point d'attache de la main/manette qui tient la poignée
        private float angleMainAuDepart;     // angle de la main au moment où elle a attrapé la poignée
        private float anglePorteAuDepart;    // angle de la porte au même moment
        private float angleCible;
        private bool dejaOuverte;

        private void Awake()
        {
            if (charniere != null) rotationFermee = charniere.localRotation;
        }

        private void OnEnable()
        {
            if (poignees == null) return;
            poignees.selectEntered.AddListener(QuandPoigneeAttrapee);
            poignees.selectExited.AddListener(QuandPoigneeLachee);
        }

        private void OnDisable()
        {
            if (poignees == null) return;
            poignees.selectEntered.RemoveListener(QuandPoigneeAttrapee);
            poignees.selectExited.RemoveListener(QuandPoigneeLachee);
        }

        private void Start()
        {
            Verrouiller(verrouilleeAuDemarrage);
        }

        private void Update()
        {
            if (charniere == null) return;

            // Pendant qu'on tient une poignée : la porte tourne d'autant que la main a tourné autour de la charnière
            if (mainQuiTient != null)
            {
                float rotationMain = Mathf.DeltaAngle(angleMainAuDepart, AngleDeLaMain());
                angleCible = Mathf.Clamp(anglePorteAuDepart + rotationMain, angleMin, angleMax);
            }

            // La porte rejoint la cible en douceur
            AngleActuel = Mathf.Lerp(AngleActuel, angleCible, vitesseSuivi * Time.deltaTime);
            charniere.localRotation = rotationFermee * Quaternion.Euler(0f, AngleActuel, 0f);

            if (!dejaOuverte && Mathf.Abs(AngleActuel) > 5f)
            {
                dejaOuverte = true;
                OnPremiereOuverture.Invoke();
            }
        }

        /// <summary>Verrouille (true) ou libère (false) les poignées.</summary>
        public void Verrouiller(bool verrouillee)
        {
            EstVerrouillee = verrouillee;
            if (poignees != null) poignees.enabled = !verrouillee;
            if (verrouillee) mainQuiTient = null;
        }

        /// <summary>Raccourci pratique à brancher sur un UnityEvent.</summary>
        public void Deverrouiller()
        {
            Verrouiller(false);
        }

        private void QuandPoigneeAttrapee(SelectEnterEventArgs args)
        {
            CommencerASuivre(args.interactorObject.GetAttachTransform(poignees));
        }

        private void QuandPoigneeLachee(SelectExitEventArgs args)
        {
            // Si une autre main tient encore une poignée, on continue avec elle
            if (poignees.isSelected)
            {
                CommencerASuivre(poignees.interactorsSelecting[0].GetAttachTransform(poignees));
                return;
            }

            mainQuiTient = null;
            angleCible = AngleActuel; // la porte reste là où on l'a lâchée
        }

        private void CommencerASuivre(Transform main)
        {
            mainQuiTient = main;
            angleMainAuDepart = AngleDeLaMain();
            anglePorteAuDepart = AngleActuel;
        }

        /// <summary>
        /// Angle (en degrés) de la main autour de l'axe vertical de la charnière, dans le repère "porte fermée".
        /// Même convention que la rotation Y d'Unity : quand la porte tourne de +a, ce nombre augmente de a.
        /// </summary>
        private float AngleDeLaMain()
        {
            Transform parent = charniere.parent;
            Vector3 mainLocale = parent.InverseTransformPoint(mainQuiTient.position) - charniere.localPosition;
            Vector3 v = Quaternion.Inverse(rotationFermee) * mainLocale;
            return Mathf.Atan2(-v.z, v.x) * Mathf.Rad2Deg;
        }

        // Test sans casque : en Play, clic droit sur le composant
        [ContextMenu("Test : ouvrir la porte")]
        private void TestOuvrir()
        {
            if (Application.isPlaying) angleCible = angleMax;
            else Debug.LogWarning("Lance le mode Play pour tester la porte.");
        }

        [ContextMenu("Test : fermer la porte")]
        private void TestFermer()
        {
            if (Application.isPlaying) angleCible = 0f;
            else Debug.LogWarning("Lance le mode Play pour tester la porte.");
        }
    }
}
