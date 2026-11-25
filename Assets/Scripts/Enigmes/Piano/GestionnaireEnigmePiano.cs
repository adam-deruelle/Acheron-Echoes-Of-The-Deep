using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Acheron.Enigmes.Piano
{
    /// <summary>
    /// Cerveau de l'énigme "piano + résistance".
    ///
    /// Déroulement d'une manche :
    ///   1. Tirage aléatoire d'une valeur de résistance et d'une couleur de voyant.
    ///   2. La résistance et le voyant s'affichent en conséquence.
    ///   3. Le joueur VR joue des notes : chaque bonne note fait avancer la séquence,
    ///      une mauvaise note la remet à zéro (flash rouge + buzzer).
    ///   4. Séquence complète → manche réussie. Après la dernière manche → OnReussite (ouvre le coffre-fort).
    ///
    /// Pour brancher d'autres effets (sons, particules, autres salles...), utiliser les UnityEvents
    /// dans l'Inspector : OnNoteJouee, OnErreur, OnMancheReussie, OnReussite.
    /// </summary>
    public class GestionnaireEnigmePiano : MonoBehaviour
    {
        [Header("Données")]
        [Tooltip("Le tableau des 28 séquences (ScriptableObject)")]
        [SerializeField] private DonneesEnigmePiano donnees;

        [Tooltip("Nombre de manches à réussir d'affilée pour ouvrir la porte")]
        [Min(1)]
        [SerializeField] private int nombreDeManches = 1;

        [Header("Éléments de la salle")]
        [SerializeField] private TouchePiano[] touches = new TouchePiano[0];
        [SerializeField] private AffichageResistance affichageResistance;
        [SerializeField] private VoyantLumineux voyant;
        [Tooltip("Optionnel : porte ouverte à la réussite (laisser vide si la récompense est seulement le coffre)")]
        [SerializeField] private PorteSortie porte;

        [Header("Sons")]
        [Tooltip("Source audio pour le buzzer d'erreur et le son de réussite")]
        [SerializeField] private AudioSource sourceAudio;

        [Header("Réglages")]
        [Tooltip("Temps pendant lequel les touches sont ignorées après une erreur (évite les doubles appuis)")]
        [SerializeField] private float delaiBlocageApresErreur = 0.8f;

        [Tooltip("Pause entre deux manches (s'il y en a plusieurs)")]
        [SerializeField] private float pauseEntreManches = 1.5f;

        [Header("Événements")]
        [Tooltip("À chaque note jouée (bonne ou mauvaise)")]
        public UnityEvent<NoteBlanche> OnNoteJouee = new UnityEvent<NoteBlanche>();

        [Tooltip("Mauvaise note : la séquence repart de zéro")]
        public UnityEvent OnErreur = new UnityEvent();

        [Tooltip("Une manche est réussie (utile s'il y a plusieurs manches)")]
        public UnityEvent OnMancheReussie = new UnityEvent();

        [Tooltip("Toutes les manches sont réussies : l'énigme est résolue")]
        public UnityEvent OnReussite = new UnityEvent();

        // --- État de la partie (lecture seule depuis l'extérieur) ---

        /// <summary>Index (dans le tableau) de la valeur de résistance tirée.</summary>
        public int IndexValeurTiree { get; private set; } = -1;

        /// <summary>Couleur du voyant tirée.</summary>
        public CouleurVoyant CouleurTiree { get; private set; }

        /// <summary>Séquence de notes à jouer pour la manche en cours.</summary>
        public NoteBlanche[] SequenceAttendue { get; private set; }

        /// <summary>Nombre de bonnes notes déjà jouées dans la séquence en cours.</summary>
        public int Progression { get; private set; }

        /// <summary>Numéro de la manche en cours (commence à 1).</summary>
        public int MancheActuelle { get; private set; }

        /// <summary>Vrai quand toutes les manches ont été réussies.</summary>
        public bool EstResolue { get; private set; }

        private bool entreeBloquee;

        private void OnEnable()
        {
            // On écoute toutes les touches du piano
            foreach (TouchePiano touche in touches)
            {
                if (touche != null) touche.OnToucheAppuyee.AddListener(QuandNoteJouee);
            }
        }

        private void OnDisable()
        {
            foreach (TouchePiano touche in touches)
            {
                if (touche != null) touche.OnToucheAppuyee.RemoveListener(QuandNoteJouee);
            }
        }

        private void Start()
        {
            if (donnees == null)
            {
                Debug.LogError("[Énigme Piano] Aucune donnée assignée au gestionnaire !", this);
                enabled = false;
                return;
            }

            if (!donnees.EstValide(out string erreur))
            {
                Debug.LogError("[Énigme Piano] Données invalides : " + erreur, donnees);
                enabled = false;
                return;
            }

            MancheActuelle = 1;
            LancerManche();
        }

        /// <summary>
        /// Tire au sort une nouvelle combinaison (valeur + couleur) et met à jour la salle.
        /// </summary>
        public void LancerManche()
        {
            int ancienIndex = IndexValeurTiree;
            CouleurVoyant ancienneCouleur = CouleurTiree;

            // Tirage aléatoire. Si plusieurs manches : on évite de retomber sur la même combinaison.
            int essais = 0;
            do
            {
                IndexValeurTiree = Random.Range(0, donnees.NombreDeValeurs);
                CouleurTiree = (CouleurVoyant)Random.Range(0, OutilsPiano.NombreDeCouleurs);
                essais++;
            }
            while (IndexValeurTiree == ancienIndex && CouleurTiree == ancienneCouleur && essais < 20);

            SequenceAttendue = donnees.GetSequence(IndexValeurTiree, CouleurTiree);
            Progression = 0;
            entreeBloquee = false;

            // Mise à jour de la salle
            float valeurOhms = donnees.GetLigne(IndexValeurTiree).valeurOhms;
            if (affichageResistance != null) affichageResistance.AfficherValeur(valeurOhms);
            if (voyant != null) voyant.AfficherCouleur(CouleurTiree);

            LoguerSolution(valeurOhms);
        }

        /// <summary>Appelée à chaque appui sur une touche du piano.</summary>
        private void QuandNoteJouee(NoteBlanche note)
        {
            if (EstResolue || entreeBloquee || SequenceAttendue == null) return;

            OnNoteJouee.Invoke(note);

            if (note == SequenceAttendue[Progression])
            {
                // Bonne note : on avance
                Progression++;

                if (Progression >= SequenceAttendue.Length)
                {
                    MancheReussie();
                }
            }
            else
            {
                // Mauvaise note : tout est à refaire
                Erreur();
            }
        }

        private void Erreur()
        {
            Progression = 0;

            if (voyant != null) voyant.FlashErreur();
            JouerSon(GenerateurSon.Buzzer());
            OnErreur.Invoke();

            StartCoroutine(BloquerEntreeTemporairement(delaiBlocageApresErreur));
        }

        private void MancheReussie()
        {
            OnMancheReussie.Invoke();

            if (MancheActuelle >= nombreDeManches)
            {
                // Dernière manche : énigme résolue !
                EstResolue = true;
                JouerSon(GenerateurSon.Reussite());
                if (porte != null) porte.Ouvrir();
                OnReussite.Invoke();
                Debug.Log("[Énigme Piano] Énigme résolue !");
            }
            else
            {
                // Il reste des manches : on enchaîne après une petite pause
                MancheActuelle++;
                JouerSon(GenerateurSon.Reussite());
                StartCoroutine(MancheSuivanteApresPause());
            }
        }

        private IEnumerator MancheSuivanteApresPause()
        {
            entreeBloquee = true;
            yield return new WaitForSeconds(pauseEntreManches);
            LancerManche();
        }

        private IEnumerator BloquerEntreeTemporairement(float duree)
        {
            entreeBloquee = true;
            yield return new WaitForSeconds(duree);
            entreeBloquee = false;
        }

        private void JouerSon(AudioClip clip)
        {
            if (sourceAudio != null && clip != null) sourceAudio.PlayOneShot(clip);
        }

        /// <summary>
        /// Affiche la solution dans la Console. UNIQUEMENT dans l'éditeur Unity :
        /// ce code n'existe pas dans le build Quest (#if UNITY_EDITOR).
        /// </summary>
        private void LoguerSolution(float valeurOhms)
        {
#if UNITY_EDITOR
            CouleurAnneau[] anneaux = CodeCouleurResistance.CalculerAnneaux(valeurOhms);
            Debug.Log(string.Format(
                "[Énigme Piano] (éditeur) Manche {0}/{1} — Résistance {2} ({3}, {4}, {5}, {6}) — Voyant {7} — Solution : {8}",
                MancheActuelle, nombreDeManches,
                CodeCouleurResistance.FormaterValeur(valeurOhms),
                anneaux[0], anneaux[1], anneaux[2], anneaux[3],
                OutilsPiano.NomCouleur(CouleurTiree),
                OutilsPiano.SequenceEnTexte(SequenceAttendue)), this);
#endif
        }

        // Test sans casque : en Play, clic droit sur le composant > "Test : jouer la solution"
        [ContextMenu("Test : jouer la solution")]
        private void TestJouerSolution()
        {
            if (!Application.isPlaying || SequenceAttendue == null)
            {
                Debug.LogWarning("Lance le mode Play pour tester.");
                return;
            }

            StartCoroutine(RoutineJouerSolution());
        }

        private IEnumerator RoutineJouerSolution()
        {
            // Copie : la séquence peut changer en cours de route s'il y a plusieurs manches
            NoteBlanche[] solution = SequenceAttendue;
            foreach (NoteBlanche note in solution)
            {
                foreach (TouchePiano touche in touches)
                {
                    if (touche != null && touche.Note == note)
                    {
                        touche.Appuyer();
                        yield return new WaitForSeconds(0.25f);
                        touche.Relacher();
                        break;
                    }
                }
                yield return new WaitForSeconds(0.25f);
            }
        }
    }
}
