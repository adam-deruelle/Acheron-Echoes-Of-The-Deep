using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using Object = UnityEngine.Object;

namespace Acheron.Enigmes.Piano.Editeur
{
    /// <summary>
    /// Menu "Acheron > Générer Salle Piano" : construit la salle de l'énigme piano dans une NOUVELLE scène,
    /// uniquement avec des primitives (blockout). Les objets et matériaux sont nommés proprement
    /// pour pouvoir être remplacés plus tard par les vrais modèles 3D.
    ///
    /// Repère de la salle (6 m × 6 m × 3 m, centrée sur l'origine, sol à y = 0) :
    ///   - Entrée (décor) : mur Sud  (z = -3)
    ///   - Sortie          : mur Nord (z = +3)
    ///   - Piano + établi  : mur Ouest (x = -3)
    ///
    /// Ce script ne modifie AUCUNE scène existante et ne touche pas aux ProjectSettings.
    /// </summary>
    public static class GenerateurSallePiano
    {
        // --- Chemins ---------------------------------------------------------
        private const string DossierRacine = "Assets/_Acheron/Salles/Adam/PianoResistance";
        private const string DossierMateriaux = DossierRacine + "/Materiaux";
        private const string DossierDonnees = DossierRacine + "/Donnees";
        private const string DossierEclairage = DossierRacine + "/Eclairage";
        private const string CheminDonnees = DossierDonnees + "/DonneesEnigmePiano.asset";
        private const string CheminReglagesEclairage = DossierEclairage + "/LS_SallePiano.lighting";
        private const string CheminScene = "Assets/_Acheron/Salles/Adam/Salle_Piano.unity";

        // XR Origin à utiliser, par ordre de préférence (le 1er est celui de Scene.unity)
        private static readonly string[] NomsPrefabsXROrigin = { "XR Origin Hands (XR Rig)", "XR Origin (XR Rig)" };

        // --- Dimensions ------------------------------------------------------
        private const float DemiLargeur = 3f;        // salle de 6 m × 6 m
        private const float Hauteur = 3f;            // hauteur sous plafond
        private const float EpaisseurMur = 0.2f;
        private const float LargeurPorte = 1.2f;
        private const float HauteurPorte = 2.2f;

        private const float HauteurClavier = 0.75f;  // dessus des touches blanches
        private const float PasTouche = 0.07f;       // distance entre 2 touches blanches
        private const float LargeurTouche = 0.065f;

        // Matériaux (remplis par CreerMateriaux)
        private static Material matSol, matMur, matPlafond, matLaiton, matBoisPiano, matToucheBlanche, matToucheNoire,
                                matEtabli, matMetal, matResistanceCorps, matResistanceAnneau, matVoyant, matPorte,
                                matApplique, matHublot, matCouloir;

        // ====================================================================
        // Menu
        // ====================================================================

        [MenuItem("Acheron/Générer Salle Piano", priority = 100)]
        private static void GenererSalle()
        {
            // 1. Proposer d'enregistrer la scène ouverte (on ne la modifie pas)
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return; // l'utilisateur a annulé
            }

            // 2. Ne pas écraser la scène sans demander
            if (File.Exists(CheminScene) &&
                !EditorUtility.DisplayDialog("Générer Salle Piano",
                    "La scène " + CheminScene + " existe déjà.\nLa remplacer par une nouvelle génération ?\n\n" +
                    "(Les matériaux et les données de l'énigme existants sont conservés.)",
                    "Remplacer", "Annuler"))
            {
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("Générer Salle Piano", "Préparation des assets...", 0.1f);
                CreerDossier(DossierMateriaux);
                CreerDossier(DossierDonnees);
                CreerDossier(DossierEclairage);
                CreerMateriaux();
                DonneesEnigmePiano donnees = ObtenirDonnees();

                EditorUtility.DisplayProgressBar("Générer Salle Piano", "Construction de la scène...", 0.4f);
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                ConfigurerEnvironnement();

                ConstruireStructure();
                ConstruireEclairage();
                ConstruireDecor();
                PorteSortie porte = ConstruirePorteSortie();
                ConstruireEnigme(donnees, porte);
                AjouterJoueurXR();

                EditorUtility.DisplayProgressBar("Générer Salle Piano", "Enregistrement...", 0.8f);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, CheminScene);
                AssetDatabase.SaveAssets();

                // Le carnet est regénéré à chaque fois pour rester synchronisé avec les données
                GenerateurCarnetPiano.GenererCarnet(donnees);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            Debug.Log("[Salle Piano] Scène générée : " + CheminScene);

            // 3. Calcul de l'éclairage (lumières "Baked" : indispensable pour le Quest)
            if (EditorUtility.DisplayDialog("Générer Salle Piano",
                    "Salle générée !\n\nLancer maintenant le calcul de l'éclairage (bake) ?\n" +
                    "Sans lui, la salle sera noire (les lumières sont en mode Baked).\n" +
                    "Tu pourras aussi le faire plus tard : Window > Rendering > Lighting > Generate Lighting.",
                    "Calculer maintenant", "Plus tard"))
            {
                Lightmapping.BakeAsync();
            }
        }

        // ====================================================================
        // Assets : dossiers, matériaux, données, réglages d'éclairage
        // ====================================================================

        /// <summary>Crée un dossier (et ses parents) dans le projet s'il n'existe pas.</summary>
        private static void CreerDossier(string chemin)
        {
            if (AssetDatabase.IsValidFolder(chemin)) return;

            string parent = Path.GetDirectoryName(chemin).Replace('\\', '/');
            CreerDossier(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(chemin));
        }

        private static void CreerMateriaux()
        {
            matSol = ObtenirMateriau("M_Piano_Sol", new Color(0.12f, 0.16f, 0.17f), 0.45f);
            matMur = ObtenirMateriau("M_Piano_Mur", new Color(0.09f, 0.2f, 0.2f), 0.2f);
            matPlafond = ObtenirMateriau("M_Piano_Plafond", new Color(0.07f, 0.08f, 0.09f), 0.1f);
            matLaiton = ObtenirMateriau("M_Piano_Laiton", new Color(0.72f, 0.55f, 0.25f), 0.7f, 0.6f);
            matBoisPiano = ObtenirMateriau("M_Piano_BoisPiano", new Color(0.12f, 0.06f, 0.04f), 0.75f);
            matToucheBlanche = ObtenirMateriau("M_Piano_ToucheBlanche", new Color(0.93f, 0.91f, 0.85f), 0.6f);
            matToucheNoire = ObtenirMateriau("M_Piano_ToucheNoire", new Color(0.03f, 0.03f, 0.03f), 0.7f);
            matEtabli = ObtenirMateriau("M_Piano_Etabli", new Color(0.35f, 0.22f, 0.12f), 0.25f);
            matMetal = ObtenirMateriau("M_Piano_Metal", new Color(0.45f, 0.46f, 0.48f), 0.6f, 0.8f);
            matResistanceCorps = ObtenirMateriau("M_Piano_ResistanceCorps", new Color(0.86f, 0.77f, 0.58f), 0.35f);
            matResistanceAnneau = ObtenirMateriau("M_Piano_ResistanceAnneau", Color.white, 0.3f); // recoloré en jeu
            matVoyant = ObtenirMateriau("M_Piano_Voyant", Color.white, 0.8f, 0f, Color.white, false);
            matPorte = ObtenirMateriau("M_Piano_Porte", new Color(0.1f, 0.25f, 0.27f), 0.5f, 0.7f);
            matApplique = ObtenirMateriau("M_Piano_Applique", new Color(1f, 0.85f, 0.6f), 0.5f, 0f, new Color(1f, 0.8f, 0.5f) * 2f, true);
            matHublot = ObtenirMateriau("M_Piano_Hublot", new Color(0.05f, 0.3f, 0.35f), 0.9f, 0f, new Color(0.05f, 0.45f, 0.55f), true);
            matCouloir = ObtenirMateriau("M_Piano_Couloir", new Color(0.03f, 0.04f, 0.05f), 0.1f);
        }

        /// <summary>
        /// Charge le matériau s'il existe déjà (pour garder les retouches de l'équipe), sinon le crée (URP/Lit).
        /// </summary>
        private static Material ObtenirMateriau(string nom, Color couleur, float lissage, float metallique = 0f,
                                                Color? emission = null, bool emissionDansEclairage = false)
        {
            string chemin = DossierMateriaux + "/" + nom + ".mat";
            Material existant = AssetDatabase.LoadAssetAtPath<Material>(chemin);
            if (existant != null) return existant;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Material mat = new Material(shader) { name = nom };
            mat.SetColor("_BaseColor", couleur);
            mat.SetFloat("_Smoothness", lissage);
            mat.SetFloat("_Metallic", metallique);

            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission.Value);
                // BakedEmissive : l'objet éclaire un peu la salle dans le calcul de lumière
                mat.globalIlluminationFlags = emissionDansEclairage
                    ? MaterialGlobalIlluminationFlags.BakedEmissive
                    : MaterialGlobalIlluminationFlags.None;
            }

            AssetDatabase.CreateAsset(mat, chemin);
            return mat;
        }

        /// <summary>Charge l'asset de données de l'énigme, ou le crée (rempli par défaut) s'il n'existe pas.</summary>
        private static DonneesEnigmePiano ObtenirDonnees()
        {
            DonneesEnigmePiano donnees = AssetDatabase.LoadAssetAtPath<DonneesEnigmePiano>(CheminDonnees);
            if (donnees != null) return donnees;

            donnees = ScriptableObject.CreateInstance<DonneesEnigmePiano>();
            donnees.RemplirParDefaut();
            AssetDatabase.CreateAsset(donnees, CheminDonnees);
            Debug.Log("[Salle Piano] Données de l'énigme créées : " + CheminDonnees);
            return donnees;
        }

        /// <summary>Ambiance générale : lumière ambiante sombre, brouillard "sous-marin", réglages de bake.</summary>
        private static void ConfigurerEnvironnement()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.04f, 0.07f, 0.09f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.02f, 0.06f, 0.08f);
            RenderSettings.fogDensity = 0.05f;

            // Réglages de calcul de lumière (asset réutilisé s'il existe déjà)
            LightingSettings reglages = AssetDatabase.LoadAssetAtPath<LightingSettings>(CheminReglagesEclairage);
            if (reglages == null)
            {
                reglages = new LightingSettings { name = "LS_SallePiano" };
                reglages.bakedGI = true;
                reglages.realtimeGI = false;
                reglages.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU; // repasse en CPU tout seul si besoin
                reglages.lightmapResolution = 10f;      // texels par mètre : léger pour le Quest
                reglages.lightmapMaxSize = 1024;
                reglages.directionalityMode = LightmapsMode.NonDirectional; // moins coûteux sur mobile
                reglages.maxBounces = 2;
                AssetDatabase.CreateAsset(reglages, CheminReglagesEclairage);
            }
            Lightmapping.lightingSettings = reglages;
        }

        // ====================================================================
        // Structure : sol, murs, plafond
        // ====================================================================

        private static void ConstruireStructure()
        {
            Transform structure = Vide("Structure", null, Vector3.zero).transform;
            float d = DemiLargeur, e = EpaisseurMur, h = Hauteur;
            float longueurTotale = 2f * d + 2f * e;

            GameObject sol = Cube("Sol", structure, new Vector3(0, -e / 2f, 0), new Vector3(longueurTotale, e, longueurTotale), matSol);
            Cube("Plafond", structure, new Vector3(0, h + e / 2f, 0), new Vector3(longueurTotale, e, longueurTotale), matPlafond);

            Cube("Mur_Ouest", structure, new Vector3(-d - e / 2f, h / 2f, 0), new Vector3(e, h, longueurTotale), matMur);
            Cube("Mur_Est", structure, new Vector3(d + e / 2f, h / 2f, 0), new Vector3(e, h, longueurTotale), matMur);
            Cube("Mur_Sud", structure, new Vector3(0, h / 2f, -d - e / 2f), new Vector3(2f * d, h, e), matMur);

            // Mur Nord : percé pour la porte de sortie (3 morceaux autour de l'ouverture)
            float largeurCote = d - LargeurPorte / 2f;
            float xCote = LargeurPorte / 2f + largeurCote / 2f;
            Cube("Mur_Nord_Gauche", structure, new Vector3(-xCote, h / 2f, d + e / 2f), new Vector3(largeurCote, h, e), matMur);
            Cube("Mur_Nord_Droit", structure, new Vector3(xCote, h / 2f, d + e / 2f), new Vector3(largeurCote, h, e), matMur);
            Cube("Mur_Nord_Linteau", structure, new Vector3(0, (HauteurPorte + h) / 2f, d + e / 2f),
                 new Vector3(LargeurPorte, h - HauteurPorte, e), matMur);

            // Frises en laiton à 2,6 m (touche "art déco")
            const float yFrise = 2.6f;
            Cube("Frise_Ouest", structure, new Vector3(-d + 0.02f, yFrise, 0), new Vector3(0.04f, 0.06f, 2f * d), matLaiton, true, false);
            Cube("Frise_Est", structure, new Vector3(d - 0.02f, yFrise, 0), new Vector3(0.04f, 0.06f, 2f * d), matLaiton, true, false);
            Cube("Frise_Sud", structure, new Vector3(0, yFrise, -d + 0.02f), new Vector3(2f * d, 0.06f, 0.04f), matLaiton, true, false);
            Cube("Frise_Nord", structure, new Vector3(0, yFrise, d - 0.02f), new Vector3(2f * d, 0.06f, 0.04f), matLaiton, true, false);

            // Le sol accepte la téléportation (si le XR Origin la propose)
            TeleportationArea zoneTeleport = sol.AddComponent<TeleportationArea>();
            int coucheTeleport = InteractionLayerMask.GetMask("Teleport");
            if (coucheTeleport != 0) zoneTeleport.interactionLayers = coucheTeleport;
        }

        // ====================================================================
        // Éclairage : lumières "Baked" + sondes de lumière
        // ====================================================================

        private static void ConstruireEclairage()
        {
            Transform eclairage = Vide("Eclairage", null, Vector3.zero).transform;
            Color chaud = new Color(1f, 0.82f, 0.6f);
            Color sousMarin = new Color(0.45f, 0.8f, 1f);

            // Lustre central
            LumiereBaked("Lumiere_Plafond", eclairage, new Vector3(0, 2.6f, 0), chaud, 2.5f, 6f);
            Primitive(PrimitiveType.Sphere, "Applique_Plafond", eclairage, new Vector3(0, 2.9f, 0), Vector3.one * 0.25f, Quaternion.identity, matApplique, true, false);

            // Applique murale entre le piano et l'établi
            LumiereBaked("Lumiere_Piano", eclairage, new Vector3(-2.55f, 2.25f, -0.15f), chaud, 2.5f, 4f);
            Primitive(PrimitiveType.Sphere, "Applique_Piano", eclairage, new Vector3(-2.93f, 2.25f, -0.15f), Vector3.one * 0.18f, Quaternion.identity, matApplique, true, false);

            // Lumières d'ambiance près des portes
            LumiereBaked("Lumiere_Sortie", eclairage, new Vector3(0, 2.5f, 2.3f), sousMarin, 1.5f, 3.5f);
            LumiereBaked("Lumiere_Entree", eclairage, new Vector3(0, 2.5f, -2.3f), chaud, 1.2f, 3.5f);

            // Sondes de lumière : éclairent les objets qui bougent (touches, porte, résistance...)
            GameObject sondes = Vide("Sondes_Lumiere", eclairage, Vector3.zero);
            LightProbeGroup groupe = sondes.AddComponent<LightProbeGroup>();
            float[] xs = { -2.7f, -1.4f, 0f, 1.4f, 2.7f };
            float[] ys = { 0.3f, 1.2f, 2.5f };
            Vector3[] positions = new Vector3[xs.Length * ys.Length * xs.Length + 4];
            int n = 0;
            foreach (float x in xs)
                foreach (float y in ys)
                    foreach (float z in xs)
                        positions[n++] = new Vector3(x, y, z);

            // Quelques sondes en plus près du piano / de l'établi / de la porte
            positions[n++] = new Vector3(-2.3f, 0.85f, 0.5f);
            positions[n++] = new Vector3(-2.3f, 1.05f, -0.6f);
            positions[n++] = new Vector3(0f, 1.2f, 2.7f);
            positions[n] = new Vector3(-2.6f, 1.7f, 0.5f);
            groupe.probePositions = positions;
        }

        private static void LumiereBaked(string nom, Transform parent, Vector3 position, Color couleur, float intensite, float portee)
        {
            Light lumiere = Vide(nom, parent, position).AddComponent<Light>();
            lumiere.type = LightType.Point;
            lumiere.color = couleur;
            lumiere.intensity = intensite;
            lumiere.range = portee;
            lumiere.shadows = LightShadows.Soft;
            // Baked : calculée une fois dans l'éditeur, coût nul en jeu (indispensable pour le Quest)
            lumiere.lightmapBakeType = LightmapBakeType.Baked;
        }

        // ====================================================================
        // Décor : porte d'entrée (fermée), hublot, couloir derrière la sortie
        // ====================================================================

        private static void ConstruireDecor()
        {
            Transform decor = Vide("Decor", null, Vector3.zero).transform;
            float d = DemiLargeur;

            // Porte d'entrée (décorative, fermée) sur le mur Sud
            Transform entree = Vide("Porte_Entree", decor, new Vector3(0, 0, -d)).transform;
            Cube("Panneau", entree, new Vector3(0, HauteurPorte / 2f, 0.04f), new Vector3(LargeurPorte, HauteurPorte, 0.08f), matPorte);
            ConstruireCadre(entree, 0.06f);

            // Hublot sur le mur Est (vue sur les profondeurs)
            Transform hublot = Vide("Hublot", decor, new Vector3(d, 1.6f, 0)).transform;
            Quaternion versSalle = Quaternion.Euler(0, 0, 90); // axe du cylindre le long de X
            Primitive(PrimitiveType.Cylinder, "Vitre", hublot, new Vector3(-0.02f, 0, 0), new Vector3(0.8f, 0.02f, 0.8f), versSalle, matHublot, true, false);
            Primitive(PrimitiveType.Cylinder, "Cerclage", hublot, new Vector3(-0.01f, 0, 0), new Vector3(0.95f, 0.01f, 0.95f), versSalle, matLaiton, true, false);

            // Couloir derrière la porte de sortie (pour ne pas voir le vide quand elle s'ouvre)
            Transform couloir = Vide("Couloir_Sortie", decor, new Vector3(0, 0, d + EpaisseurMur)).transform;
            const float longueur = 2f;
            Cube("Sol", couloir, new Vector3(0, -0.1f, longueur / 2f), new Vector3(1.6f, 0.2f, longueur), matCouloir);
            Cube("Plafond", couloir, new Vector3(0, 2.5f, longueur / 2f), new Vector3(2f, 0.2f, longueur), matCouloir);
            Cube("Mur_Gauche", couloir, new Vector3(-0.9f, 1.2f, longueur / 2f), new Vector3(0.2f, 2.6f, longueur), matCouloir);
            Cube("Mur_Droit", couloir, new Vector3(0.9f, 1.2f, longueur / 2f), new Vector3(0.2f, 2.6f, longueur), matCouloir);
            Cube("Fond_Lumineux", couloir, new Vector3(0, 1.2f, longueur + 0.1f), new Vector3(2f, 2.6f, 0.2f), matHublot);
        }

        /// <summary>Cadre en laiton autour d'une porte (montants + traverse), côté salle.</summary>
        private static void ConstruireCadre(Transform porte, float decalage)
        {
            const float l = 0.08f; // largeur des montants
            float xMontant = LargeurPorte / 2f + l / 2f;
            float zCadre = porte.position.z > 0 ? -decalage : decalage; // toujours côté intérieur de la salle
            Cube("Cadre_Gauche", porte, new Vector3(-xMontant, HauteurPorte / 2f, zCadre), new Vector3(l, HauteurPorte + l, 0.05f), matLaiton, true, false);
            Cube("Cadre_Droit", porte, new Vector3(xMontant, HauteurPorte / 2f, zCadre), new Vector3(l, HauteurPorte + l, 0.05f), matLaiton, true, false);
            Cube("Cadre_Haut", porte, new Vector3(0, HauteurPorte + l / 2f, zCadre), new Vector3(LargeurPorte + 2f * l, l, 0.05f), matLaiton, true, false);
        }

        // ====================================================================
        // Porte de sortie (mur Nord)
        // ====================================================================

        private static PorteSortie ConstruirePorteSortie()
        {
            GameObject racine = Vide("Porte_Sortie", null, new Vector3(0, 0, DemiLargeur));
            ConstruireCadre(racine.transform, 0.03f);

            // Le battant (qui bouge) est un objet vide : ses enfants sont les morceaux visibles
            GameObject battant = Vide("Battant", racine.transform, new Vector3(0, 0, -0.1f));
            Cube("Panneau", battant.transform, new Vector3(0, (HauteurPorte + 0.1f) / 2f, 0),
                 new Vector3(LargeurPorte + 0.1f, HauteurPorte + 0.1f, 0.08f), matPorte, false, true);
            Primitive(PrimitiveType.Cylinder, "Hublot_Porte", battant.transform, new Vector3(0, 1.55f, -0.045f),
                      new Vector3(0.35f, 0.01f, 0.35f), Quaternion.Euler(90, 0, 0), matLaiton, false, false);
            Cube("Poignee", battant.transform, new Vector3(-0.5f, 1.05f, -0.06f), new Vector3(0.04f, 0.3f, 0.04f), matLaiton, false, false);

            // Son d'ouverture : sans clip assigné, PorteSortie joue le buzzer ; pitch 0,5 = grondement grave provisoire
            AudioSource son = racine.AddComponent<AudioSource>();
            son.playOnAwake = false;
            son.spatialBlend = 1f;
            son.pitch = 0.5f;

            PorteSortie porte = racine.AddComponent<PorteSortie>();
            DefinirReference(porte, "battant", battant.transform);
            DefinirVector3(porte, "deplacementOuverture", new Vector3(LargeurPorte + 0.15f, 0, 0)); // glisse vers la droite
            DefinirReference(porte, "sonOuverture", son);
            return porte;
        }

        // ====================================================================
        // L'énigme : piano, voyant, établi, résistance, gestionnaire
        // ====================================================================

        private static void ConstruireEnigme(DonneesEnigmePiano donnees, PorteSortie porte)
        {
            GameObject racine = Vide("Enigme_Piano", null, Vector3.zero);

            TouchePiano[] touches = ConstruirePiano(racine.transform);
            VoyantLumineux voyant = ConstruireVoyant(racine.transform);
            AffichageResistance resistance = ConstruireEtabliEtResistance(racine.transform);

            // Le gestionnaire + sa source audio (buzzer / réussite), placée près du piano
            GameObject objetGestionnaire = Vide("Gestionnaire", racine.transform, new Vector3(-2.5f, 1.4f, 0.2f));
            AudioSource source = objetGestionnaire.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0.7f;

            GestionnaireEnigmePiano gestionnaire = objetGestionnaire.AddComponent<GestionnaireEnigmePiano>();
            DefinirReference(gestionnaire, "donnees", donnees);
            DefinirTableau(gestionnaire, "touches", touches);
            DefinirReference(gestionnaire, "affichageResistance", resistance);
            DefinirReference(gestionnaire, "voyant", voyant);
            DefinirReference(gestionnaire, "porte", porte);
            DefinirReference(gestionnaire, "sourceAudio", source);
        }

        /// <summary>
        /// Piano droit contre le mur Ouest. On le construit dans son propre repère :
        /// X = le long du clavier (Do à gauche), Z négatif = côté joueur, Z positif = côté mur.
        /// </summary>
        private static TouchePiano[] ConstruirePiano(Transform parent)
        {
            // Tourné de -90° : le joueur, face au mur Ouest, a le Do à sa gauche
            GameObject piano = Vide("Piano", parent, new Vector3(-DemiLargeur + 0.35f, 0, 0.5f));
            piano.transform.localRotation = Quaternion.Euler(0, -90, 0);
            Transform p = piano.transform;

            // Meuble
            Cube("Caisse", p, new Vector3(0, 0.625f, 0.15f), new Vector3(0.6f, 1.25f, 0.35f), matBoisPiano);
            Cube("Couvercle", p, new Vector3(0, 1.265f, 0.14f), new Vector3(0.64f, 0.03f, 0.39f), matBoisPiano);
            Cube("Lit_Clavier", p, new Vector3(0, HauteurClavier - 0.06f, -0.13f), new Vector3(0.6f, 0.06f, 0.26f), matBoisPiano);
            Cube("Joue_Gauche", p, new Vector3(-0.285f, HauteurClavier + 0.02f, -0.13f), new Vector3(0.03f, 0.1f, 0.26f), matBoisPiano);
            Cube("Joue_Droite", p, new Vector3(0.285f, HauteurClavier + 0.02f, -0.13f), new Vector3(0.03f, 0.1f, 0.26f), matBoisPiano);
            Cube("Pied_Gauche", p, new Vector3(-0.27f, (HauteurClavier - 0.09f) / 2f, -0.22f), new Vector3(0.05f, HauteurClavier - 0.09f, 0.05f), matBoisPiano);
            Cube("Pied_Droit", p, new Vector3(0.27f, (HauteurClavier - 0.09f) / 2f, -0.22f), new Vector3(0.05f, HauteurClavier - 0.09f, 0.05f), matBoisPiano);
            Cube("Pupitre", p, new Vector3(0, 0.98f, -0.04f), new Vector3(0.5f, 0.25f, 0.02f), matBoisPiano, true, false);
            Cube("Frise_Piano", p, new Vector3(0, 1.17f, -0.03f), new Vector3(0.6f, 0.02f, 0.01f), matLaiton, true, false);

            // Touches blanches (interactives)
            Transform blanches = Vide("Touches_Blanches", p, Vector3.zero).transform;
            TouchePiano[] touches = new TouchePiano[OutilsPiano.NombreDeNotes];
            for (int i = 0; i < OutilsPiano.NombreDeNotes; i++)
            {
                NoteBlanche note = (NoteBlanche)i;
                float x = (i - 3) * PasTouche; // Do à gauche, Si à droite
                touches[i] = CreerToucheBlanche(blanches, note, new Vector3(x, HauteurClavier - 0.015f, -0.135f));
            }

            // Touches noires (décoratives : pas de collider, pas de script)
            // Placées entre Do-Ré, Ré-Mi, Fa-Sol, Sol-La, La-Si
            Transform noires = Vide("Touches_Noires", p, Vector3.zero).transform;
            int[] separations = { 1, 2, 4, 5, 6 };
            for (int i = 0; i < separations.Length; i++)
            {
                float x = (separations[i] - 3.5f) * PasTouche;
                Cube("Touche_Noire_" + (i + 1), noires, new Vector3(x, HauteurClavier + 0.015f, -0.09f),
                     new Vector3(0.035f, 0.03f, 0.13f), matToucheNoire, true, false);
            }

            return touches;
        }

        /// <summary>
        /// Une touche blanche :
        ///   Touche_Do (BoxCollider + XRSimpleInteractable + XRPokeFilter + AudioSource + TouchePiano)
        ///     └─ Visuel (cube sans collider, c'est lui qui s'enfonce)
        /// </summary>
        private static TouchePiano CreerToucheBlanche(Transform parent, NoteBlanche note, Vector3 position)
        {
            Vector3 taille = new Vector3(LargeurTouche, 0.03f, 0.22f);
            GameObject touche = Vide("Touche_" + note, parent, position);

            // Collider immobile, un peu moins épais : le doigt doit descendre de 2 cm pour valider l'appui
            BoxCollider collider = touche.AddComponent<BoxCollider>();
            collider.size = new Vector3(taille.x, 0.02f, taille.z);
            collider.center = new Vector3(0, 0.005f, 0);

            GameObject visuel = Cube("Visuel", touche.transform, Vector3.zero, taille, matToucheBlanche, false, false);

            // AddComponent<TouchePiano> ajoute aussi automatiquement XRSimpleInteractable, XRPokeFilter et AudioSource
            TouchePiano script = touche.AddComponent<TouchePiano>();
            DefinirEnum(script, "note", (int)note);
            DefinirReference(script, "visuel", visuel.transform);

            // Son 3D qui vient de la touche
            AudioSource source = touche.GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.minDistance = 0.5f;
            source.maxDistance = 10f;

            // Filtre poke : appui de HAUT en BAS (axe -Y de la touche)
            XRSimpleInteractable interactable = touche.GetComponent<XRSimpleInteractable>();
            XRPokeFilter filtre = touche.GetComponent<XRPokeFilter>();
            SerializedObject so = new SerializedObject(filtre);
            so.FindProperty("m_Interactable").objectReferenceValue = interactable;
            so.FindProperty("m_PokeCollider").objectReferenceValue = collider;
            so.FindProperty("m_PokeConfiguration.m_UseConstant").boolValue = true;
            SerializedProperty direction = so.FindProperty("m_PokeConfiguration.m_ConstantValue.m_PokeDirection");
            int indexNegY = Array.IndexOf(direction.enumNames, "NegativeY");
            if (indexNegY >= 0) direction.enumValueIndex = indexNegY;
            else Debug.LogWarning("[Salle Piano] Axe 'NegativeY' introuvable dans XRPokeFilter : règle-le à la main (Poke Direction = -Y).");
            so.FindProperty("m_PokeConfiguration.m_ConstantValue.m_EnablePokeAngleThreshold").boolValue = true;
            so.FindProperty("m_PokeConfiguration.m_ConstantValue.m_PokeAngleThreshold").floatValue = 45f;
            so.ApplyModifiedPropertiesWithoutUndo();

            return script;
        }

        private static VoyantLumineux ConstruireVoyant(Transform parent)
        {
            // Au mur, au-dessus du piano
            GameObject voyant = Vide("Voyant", parent, new Vector3(-DemiLargeur + 0.1f, 1.7f, 0.5f));
            Transform v = voyant.transform;

            Cube("Plaque", v, new Vector3(-0.08f, 0, 0), new Vector3(0.04f, 0.35f, 0.35f), matLaiton, true, false);
            Primitive(PrimitiveType.Cylinder, "Collerette", v, new Vector3(-0.04f, 0, 0), new Vector3(0.18f, 0.03f, 0.18f),
                      Quaternion.Euler(0, 0, 90), matLaiton, true, false);
            GameObject ampoule = Primitive(PrimitiveType.Sphere, "Ampoule", v, Vector3.zero, Vector3.one * 0.14f,
                                           Quaternion.identity, matVoyant, false, false);

            // Halo (temps réel) : visible sur PC uniquement, le profil URP du Quest coupe les lumières additionnelles
            Light lumiere = Vide("Lumiere_Voyant", v, new Vector3(0.15f, 0, 0)).AddComponent<Light>();
            lumiere.type = LightType.Point;
            lumiere.range = 1.5f;
            lumiere.intensity = 1.5f;
            lumiere.shadows = LightShadows.None;
            lumiere.lightmapBakeType = LightmapBakeType.Realtime;

            VoyantLumineux script = voyant.AddComponent<VoyantLumineux>();
            DefinirReference(script, "ampoule", ampoule.GetComponent<Renderer>());
            DefinirReference(script, "lumiere", lumiere);
            return script;
        }

        private static AffichageResistance ConstruireEtabliEtResistance(Transform parent)
        {
            // Établi à côté du piano (au Sud de celui-ci), contre le mur Ouest
            GameObject etabli = Vide("Etabli", parent, new Vector3(-DemiLargeur + 0.32f, 0, -0.6f));
            Transform e = etabli.transform;
            Cube("Plateau", e, new Vector3(0, 0.875f, 0), new Vector3(0.6f, 0.05f, 1.2f), matEtabli);
            Cube("Etagere", e, new Vector3(0, 0.25f, 0), new Vector3(0.55f, 0.03f, 1.1f), matEtabli);
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0) ? -0.25f : 0.25f;
                float z = (i < 2) ? -0.55f : 0.55f;
                Cube("Pied_" + (i + 1), e, new Vector3(x, 0.425f, z), new Vector3(0.05f, 0.85f, 0.05f), matMetal);
            }

            // Résistance géante couchée sur l'établi, le long de l'axe Z.
            // Le joueur, face au mur Ouest, la lit de gauche (-Z) à droite (+Z) : tolérance à droite.
            GameObject resistance = Vide("Resistance", e, new Vector3(0, 1.0f, 0));
            Transform r = resistance.transform;
            Quaternion axeZ = Quaternion.Euler(90, 0, 0); // un cylindre Unity est vertical : on le couche

            Primitive(PrimitiveType.Cylinder, "Corps", r, Vector3.zero, new Vector3(0.12f, 0.25f, 0.12f), axeZ, matResistanceCorps, false, true);
            Primitive(PrimitiveType.Sphere, "Embout_Gauche", r, new Vector3(0, 0, -0.25f), Vector3.one * 0.14f, Quaternion.identity, matResistanceCorps, false, false);
            Primitive(PrimitiveType.Sphere, "Embout_Droit", r, new Vector3(0, 0, 0.25f), Vector3.one * 0.14f, Quaternion.identity, matResistanceCorps, false, false);
            Primitive(PrimitiveType.Cylinder, "Fil_Gauche", r, new Vector3(0, 0, -0.39f), new Vector3(0.015f, 0.1f, 0.015f), axeZ, matMetal, false, false);
            Primitive(PrimitiveType.Cylinder, "Fil_Droit", r, new Vector3(0, 0, 0.39f), new Vector3(0.015f, 0.1f, 0.015f), axeZ, matMetal, false, false);
            Cube("Support_Gauche", r, new Vector3(0, -0.085f, -0.25f), new Vector3(0.08f, 0.03f, 0.04f), matMetal, false, false);
            Cube("Support_Droit", r, new Vector3(0, -0.085f, 0.25f), new Vector3(0.08f, 0.03f, 0.04f), matMetal, false, false);

            // Les 4 anneaux : 3 groupés à gauche, la tolérance isolée à droite
            float[] positionsAnneaux = { -0.17f, -0.11f, -0.05f, 0.15f };
            Renderer[] anneaux = new Renderer[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject anneau = Primitive(PrimitiveType.Cylinder, "Anneau_" + (i + 1), r, new Vector3(0, 0, positionsAnneaux[i]),
                                              new Vector3(0.13f, 0.012f, 0.13f), axeZ, matResistanceAnneau, false, false);
                anneaux[i] = anneau.GetComponent<Renderer>();
            }

            AffichageResistance script = resistance.AddComponent<AffichageResistance>();
            DefinirTableau(script, "anneaux", anneaux);
            return script;
        }

        // ====================================================================
        // Joueur XR
        // ====================================================================

        private static void AjouterJoueurXR()
        {
            // Gestionnaire d'interactions XR (un seul par scène)
            Vide("XR Interaction Manager", null, Vector3.zero).AddComponent<XRInteractionManager>();

            GameObject prefab = TrouverPrefabXROrigin();
            Vector3 depart = new Vector3(0, 0, -1.6f);
            Quaternion orientation = Quaternion.Euler(0, -50, 0); // regarde vers le piano

            if (prefab != null)
            {
                GameObject joueur = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                joueur.transform.SetPositionAndRotation(depart, orientation);
                Debug.Log("[Salle Piano] XR Origin ajouté : " + AssetDatabase.GetAssetPath(prefab));
            }
            else
            {
                // Pas de prefab XR trouvé : simple caméra pour pouvoir au moins visualiser la salle
                GameObject camera = Vide("Main Camera (sans XR)", null, depart + Vector3.up * 1.6f);
                camera.transform.rotation = orientation;
                camera.tag = "MainCamera";
                camera.AddComponent<Camera>();
                camera.AddComponent<AudioListener>();
                Debug.LogWarning("[Salle Piano] Aucun prefab XR Origin trouvé : une caméra simple a été ajoutée à la place.");
            }
        }

        private static GameObject TrouverPrefabXROrigin()
        {
            string[] guids = AssetDatabase.FindAssets("XR Origin t:Prefab");
            foreach (string nomVoulu in NomsPrefabsXROrigin)
            {
                foreach (string guid in guids)
                {
                    string chemin = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileNameWithoutExtension(chemin) == nomVoulu)
                    {
                        return AssetDatabase.LoadAssetAtPath<GameObject>(chemin);
                    }
                }
            }
            return null;
        }

        // ====================================================================
        // Outils de construction
        // ====================================================================

        private static GameObject Vide(string nom, Transform parent, Vector3 positionLocale)
        {
            GameObject go = new GameObject(nom);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = positionLocale;
            return go;
        }

        private static GameObject Cube(string nom, Transform parent, Vector3 positionLocale, Vector3 taille, Material materiau,
                                       bool statique = true, bool avecCollider = true)
        {
            return Primitive(PrimitiveType.Cube, nom, parent, positionLocale, taille, Quaternion.identity, materiau, statique, avecCollider);
        }

        /// <summary>
        /// Crée une primitive. "statique" = participe au calcul de lumière (lightmaps) et au regroupement (batching).
        /// Les objets qui bougent ou changent de couleur en jeu ne doivent pas être statiques.
        /// </summary>
        private static GameObject Primitive(PrimitiveType type, string nom, Transform parent, Vector3 positionLocale,
                                            Vector3 echelle, Quaternion rotationLocale, Material materiau,
                                            bool statique, bool avecCollider)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = nom;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = positionLocale;
            go.transform.localRotation = rotationLocale;
            go.transform.localScale = echelle;
            go.GetComponent<Renderer>().sharedMaterial = materiau;

            if (!avecCollider)
            {
                Object.DestroyImmediate(go.GetComponent<Collider>());
            }

            if (statique)
            {
                GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic);
            }

            return go;
        }

        // --- Remplissage des champs [SerializeField] privés des scripts ---

        private static void DefinirReference(Object cible, string champ, Object valeur)
        {
            SerializedObject so = new SerializedObject(cible);
            SerializedProperty prop = TrouverPropriete(so, champ);
            if (prop == null) return;
            prop.objectReferenceValue = valeur;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void DefinirTableau(Object cible, string champ, Object[] valeurs)
        {
            SerializedObject so = new SerializedObject(cible);
            SerializedProperty prop = TrouverPropriete(so, champ);
            if (prop == null) return;
            prop.arraySize = valeurs.Length;
            for (int i = 0; i < valeurs.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = valeurs[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void DefinirEnum(Object cible, string champ, int index)
        {
            SerializedObject so = new SerializedObject(cible);
            SerializedProperty prop = TrouverPropriete(so, champ);
            if (prop == null) return;
            prop.enumValueIndex = index;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void DefinirVector3(Object cible, string champ, Vector3 valeur)
        {
            SerializedObject so = new SerializedObject(cible);
            SerializedProperty prop = TrouverPropriete(so, champ);
            if (prop == null) return;
            prop.vector3Value = valeur;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SerializedProperty TrouverPropriete(SerializedObject so, string champ)
        {
            SerializedProperty prop = so.FindProperty(champ);
            if (prop == null)
            {
                Debug.LogError("[Salle Piano] Champ '" + champ + "' introuvable sur " + so.targetObject.GetType().Name);
            }
            return prop;
        }
    }
}
