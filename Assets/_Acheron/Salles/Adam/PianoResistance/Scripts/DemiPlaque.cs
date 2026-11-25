using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Acheron.Enigmes.Piano
{
    /// <summary>Quelle moitié de la plaque de verre (la plaque complète est coupée en deux).</summary>
    public enum MoitiePlaque
    {
        Gauche = 0,
        Droite = 1
    }

    /// <summary>
    /// Une moitié de plaque de verre portant deux chiffres à moitié effacés.
    /// La seconde moitié sera obtenue dans une autre salle : les deux moitiés d'une même plaque
    /// partagent le même "identifiantPlaque" et ont des "moitie" différentes, ce qui permettra
    /// plus tard de vérifier qu'on les assemble correctement.
    ///
    /// Tant qu'elle est verrouillée (coffre fermé), on ne peut pas l'attraper.
    ///
    /// Organisation attendue :
    ///   DemiPlaque            ← ce script + XRGrabInteractable + Rigidbody + BoxCollider
    ///     ├─ Verre_Avant      ← quad visible de face (reçoit la texture des chiffres)
    ///     └─ Verre_Arriere    ← quad retourné : de dos, on voit les chiffres en miroir, comme à travers du verre
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    [RequireComponent(typeof(Rigidbody))]
    public class DemiPlaque : MonoBehaviour
    {
        [Header("Identité")]
        [Tooltip("Identifiant commun aux deux moitiés d'une même plaque")]
        [SerializeField] private string identifiantPlaque = "Plaque_Code_1";

        [Tooltip("Quelle moitié est-ce ?")]
        [SerializeField] private MoitiePlaque moitie = MoitiePlaque.Gauche;

        [Header("Affichage")]
        [Tooltip("Image des chiffres (moitié effacée). Remplacer la texture provisoire par la vraie quand les chiffres seront décidés.")]
        [SerializeField] private Texture textureChiffres;

        [Tooltip("Les Renderers du verre qui affichent la texture des chiffres (face avant + face arrière)")]
        [SerializeField] private Renderer[] rendusVerre = new Renderer[0];

        [Header("État")]
        [Tooltip("Verrouillée = impossible à attraper (le coffre la débloque à l'ouverture)")]
        [SerializeField] private bool verrouilleeAuDemarrage = true;

        [Tooltip("Déclenché la première fois que le joueur attrape la plaque")]
        public UnityEvent OnPremierePrise = new UnityEvent();

        public string IdentifiantPlaque => identifiantPlaque;
        public MoitiePlaque Moitie => moitie;
        public bool EstVerrouillee { get; private set; }

        private XRGrabInteractable grab;
        private Rigidbody corps;
        private bool dejaPrise;

        // Identifiant de la propriété "texture principale" du shader URP/Lit
        private static readonly int IdTexture = Shader.PropertyToID("_BaseMap");

        private void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            corps = GetComponent<Rigidbody>();
            AppliquerTexture();
            Verrouiller(verrouilleeAuDemarrage);
        }

        private void OnEnable()
        {
            grab.selectEntered.AddListener(QuandAttrapee);
        }

        private void OnDisable()
        {
            grab.selectEntered.RemoveListener(QuandAttrapee);
        }

        /// <summary>
        /// Verrouille (true) ou libère (false) la plaque.
        /// Verrouillée : l'interactable est désactivé et la plaque ne bouge pas.
        /// </summary>
        public void Verrouiller(bool verrouillee)
        {
            EstVerrouillee = verrouillee;
            if (grab == null) grab = GetComponent<XRGrabInteractable>();
            if (corps == null) corps = GetComponent<Rigidbody>();

            grab.enabled = !verrouillee;

            // Verrouillée : immobile (kinematic). Libérée : objet physique normal, qui repose dans le coffre.
            // Important : elle doit déjà être non-kinematic AVANT qu'on l'attrape, car le Grab Interactable
            // mémorise cet état et le remet quand on la lâche (sinon il ne peut pas la "lancer" : avertissement).
            corps.isKinematic = verrouillee;
            corps.useGravity = !verrouillee;
        }

        /// <summary>Raccourci pour libérer la plaque (pratique à brancher sur un UnityEvent).</summary>
        public void Deverrouiller()
        {
            Verrouiller(false);
        }

        private void QuandAttrapee(SelectEnterEventArgs args)
        {
            if (!dejaPrise)
            {
                dejaPrise = true;
                OnPremierePrise.Invoke();
            }
        }

        /// <summary>
        /// Envoie la texture des chiffres au matériau du verre.
        /// MaterialPropertyBlock : chaque plaque peut avoir sa propre texture sans dupliquer le matériau.
        /// </summary>
        private void AppliquerTexture()
        {
            if (rendusVerre == null || textureChiffres == null) return;

            foreach (Renderer rendu in rendusVerre)
            {
                if (rendu == null) continue;
                MaterialPropertyBlock bloc = new MaterialPropertyBlock();
                rendu.GetPropertyBlock(bloc);
                bloc.SetTexture(IdTexture, textureChiffres);
                rendu.SetPropertyBlock(bloc);
            }
        }

        // Appelée quand on modifie un champ dans l'Inspector : on voit la nouvelle texture tout de suite
        private void OnValidate()
        {
            AppliquerTexture();
        }
    }
}
