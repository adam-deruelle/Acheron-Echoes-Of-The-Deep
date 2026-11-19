using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Acheron.Enigmes.Piano.Editeur
{
    /// <summary>
    /// Génère le carnet papier du joueur sans casque (Docs/Carnet_EnigmePiano.md)
    /// à partir du ScriptableObject DonneesEnigmePiano.
    /// → Si on modifie le tableau dans Unity, il suffit de relancer le menu pour mettre le carnet à jour.
    /// </summary>
    public static class GenerateurCarnetPiano
    {
        /// <summary>Chemin du carnet, relatif à la racine du projet (à côté du dossier Assets).</summary>
        public const string CheminCarnet = "Docs/Carnet_EnigmePiano.md";

        [MenuItem("Acheron/Générer Carnet Piano", priority = 101)]
        private static void MenuGenererCarnet()
        {
            DonneesEnigmePiano donnees = TrouverDonnees();
            if (donnees == null)
            {
                EditorUtility.DisplayDialog("Carnet Piano",
                    "Aucun asset DonneesEnigmePiano trouvé.\nLance d'abord « Acheron > Générer Salle Piano ».", "OK");
                return;
            }

            if (GenererCarnet(donnees))
            {
                EditorUtility.RevealInFinder(CheminCompletCarnet());
            }
        }

        /// <summary>Cherche le premier asset DonneesEnigmePiano du projet.</summary>
        public static DonneesEnigmePiano TrouverDonnees()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(DonneesEnigmePiano));
            if (guids.Length == 0) return null;

            if (guids.Length > 1)
            {
                Debug.LogWarning("[Carnet Piano] Plusieurs assets DonneesEnigmePiano trouvés, utilisation du premier : "
                                 + AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            return AssetDatabase.LoadAssetAtPath<DonneesEnigmePiano>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        /// <summary>Écrit le fichier Markdown. Renvoie false en cas de problème.</summary>
        public static bool GenererCarnet(DonneesEnigmePiano donnees)
        {
            if (!donnees.EstValide(out string erreur))
            {
                EditorUtility.DisplayDialog("Carnet Piano", "Les données sont invalides :\n" + erreur, "OK");
                return false;
            }

            string chemin = CheminCompletCarnet();
            Directory.CreateDirectory(Path.GetDirectoryName(chemin));
            File.WriteAllText(chemin, ConstruireMarkdown(donnees), new UTF8Encoding(false));

            Debug.Log("[Carnet Piano] Carnet généré : " + chemin);
            return true;
        }

        private static string CheminCompletCarnet()
        {
            // Application.dataPath = ".../MonProjet/Assets" → on remonte d'un dossier
            string racineProjet = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(racineProjet, CheminCarnet);
        }

        // --------------------------------------------------------------------
        // Construction du texte
        // --------------------------------------------------------------------

        private static string ConstruireMarkdown(DonneesEnigmePiano donnees)
        {
            StringBuilder md = new StringBuilder();

            md.AppendLine("# Carnet technique — Salle du piano");
            md.AppendLine();
            md.AppendLine("> *Acheron : Echoes of the Deep* — document du joueur **sans casque**.");
            md.AppendLine("> Fichier généré automatiquement depuis `" + AssetDatabase.GetAssetPath(donnees)
                          + "` (menu **Acheron > Générer Carnet Piano**). Ne pas modifier à la main.");
            md.AppendLine();
            md.AppendLine("## Mission");
            md.AppendLine();
            md.AppendLine("1. Demande à ton coéquipier de te décrire **les 4 anneaux de couleur** de la grosse résistance posée sur l'établi.");
            md.AppendLine("2. Calcule la **valeur** de la résistance grâce au code couleur (partie 1).");
            md.AppendLine("3. Demande-lui la **couleur du voyant** allumé au-dessus du piano.");
            md.AppendLine("4. Lis la **séquence de 4 notes** dans le tableau (partie 2).");
            md.AppendLine("5. Guide-le sur le clavier (partie 3) : les touches ne portent aucun nom !");
            md.AppendLine();
            md.AppendLine("Une fausse note et tout est à recommencer depuis la première note.");
            md.AppendLine();

            AjouterCodeCouleur(md);
            AjouterTableauSequences(md, donnees);
            AjouterClavier(md);

            md.AppendLine("---");
            md.AppendLine("*Généré le " + DateTime.Now.ToString("dd/MM/yyyy à HH:mm") + ".*");
            return md.ToString();
        }

        private static void AjouterCodeCouleur(StringBuilder md)
        {
            md.AppendLine("## 1. Code couleur des résistances (CEI 60062)");
            md.AppendLine();
            md.AppendLine("Une résistance à 4 anneaux se lit **de gauche à droite**, l'anneau de tolérance");
            md.AppendLine("(séparé des trois autres par un espace plus grand) étant placé **à droite** :");
            md.AppendLine();
            md.AppendLine("```");
            md.AppendLine("   ┌──────────────────────────────┐");
            md.AppendLine("───┤  ██  ██  ██           ██     ├───");
            md.AppendLine("   └──────────────────────────────┘");
            md.AppendLine("      1er 2e  multi-       tolé-");
            md.AppendLine("      chif.   plicateur    rance");
            md.AppendLine("```");
            md.AppendLine();
            md.AppendLine("**Valeur = (1er chiffre)(2e chiffre) × multiplicateur**");
            md.AppendLine();
            md.AppendLine("| Couleur | Chiffre | Multiplicateur | Tolérance |");
            md.AppendLine("|---|:---:|:---:|:---:|");

            foreach (CouleurAnneau couleur in Enum.GetValues(typeof(CouleurAnneau)))
            {
                md.AppendLine("| " + CodeCouleurResistance.NomCouleur(couleur)
                              + " | " + CodeCouleurResistance.TexteChiffre(couleur)
                              + " | " + CodeCouleurResistance.TexteMultiplicateur(couleur)
                              + " | " + CodeCouleurResistance.TexteTolerance(couleur) + " |");
            }

            md.AppendLine();
            md.AppendLine("**Exemple :** Jaune – Violet – Rouge – Or → 4 7 × 100 = 4 700 Ω = **4,7 kΩ** (± 5 %).");
            md.AppendLine();
        }

        private static void AjouterTableauSequences(StringBuilder md, DonneesEnigmePiano donnees)
        {
            md.AppendLine("## 2. Tableau des séquences");
            md.AppendLine();
            md.AppendLine("Ligne = valeur de la résistance, colonne = couleur du voyant.");
            md.AppendLine();

            // En-tête
            StringBuilder entete = new StringBuilder("| Résistance |");
            StringBuilder separateur = new StringBuilder("|---|");
            for (int c = 0; c < OutilsPiano.NombreDeCouleurs; c++)
            {
                entete.Append(" Voyant " + OutilsPiano.NomCouleur((CouleurVoyant)c).ToUpper() + " |");
                separateur.Append(":---:|");
            }
            md.AppendLine(entete.ToString());
            md.AppendLine(separateur.ToString());

            // Une ligne par valeur
            for (int i = 0; i < donnees.NombreDeValeurs; i++)
            {
                LigneResistance ligne = donnees.GetLigne(i);
                StringBuilder texteLigne = new StringBuilder("| **" + CodeCouleurResistance.FormaterValeur(ligne.valeurOhms) + "** |");
                for (int c = 0; c < OutilsPiano.NombreDeCouleurs; c++)
                {
                    texteLigne.Append(" " + OutilsPiano.SequenceEnTexte(ligne.GetSequence((CouleurVoyant)c)) + " |");
                }
                md.AppendLine(texteLigne.ToString());
            }
            md.AppendLine();
        }

        private static void AjouterClavier(StringBuilder md)
        {
            md.AppendLine("## 3. Le clavier");
            md.AppendLine();
            md.AppendLine("Le piano a une seule octave. **Seules les 7 touches blanches** jouent une note ;");
            md.AppendLine("les touches noires sont décoratives (repères : groupe de 2 noires, puis groupe de 3).");
            md.AppendLine();
            md.AppendLine("```");
            md.Append(DessinerClavier());
            md.AppendLine("```");
            md.AppendLine();
            md.AppendLine("Astuce pour guider : le **Do** est la touche blanche juste à gauche du groupe de **2** touches noires,");
            md.AppendLine("le **Fa** celle juste à gauche du groupe de **3** touches noires.");
            md.AppendLine();
        }

        /// <summary>Dessine le clavier en caractères (7 touches blanches de 6 caractères de large).</summary>
        private static string DessinerClavier()
        {
            const int largeurTouche = 6;
            const int nbBlanches = OutilsPiano.NombreDeNotes;
            int largeurTotale = largeurTouche * nbBlanches;

            // Positions (en numéro de séparation entre touches blanches) des touches noires :
            // entre Do-Ré (1), Ré-Mi (2), Fa-Sol (4), Sol-La (5), La-Si (6)
            int[] separationsNoires = { 1, 2, 4, 5, 6 };

            StringBuilder dessin = new StringBuilder();
            string bordure = "+" + new string('-', largeurTotale - 1) + "+";
            dessin.AppendLine(bordure);

            // Partie haute : touches noires
            for (int rangee = 0; rangee < 3; rangee++)
            {
                StringBuilder ligne = new StringBuilder();
                for (int col = 0; col <= largeurTotale; col++)
                {
                    bool estNoire = false;
                    foreach (int s in separationsNoires)
                    {
                        if (Math.Abs(col - s * largeurTouche) <= 1) estNoire = true;
                    }

                    if (estNoire) ligne.Append('█');
                    else if (col % largeurTouche == 0) ligne.Append('|');
                    else ligne.Append(' ');
                }
                dessin.AppendLine(ligne.ToString());
            }

            // Partie basse : touches blanches seules
            string ligneBlanche = "";
            string ligneNoms = "";
            for (int n = 0; n < nbBlanches; n++)
            {
                ligneBlanche += "|" + new string(' ', largeurTouche - 1);
                ligneNoms += "|" + Centrer(OutilsPiano.NomNote((NoteBlanche)n), largeurTouche - 1);
            }
            dessin.AppendLine(ligneBlanche + "|");
            dessin.AppendLine(ligneNoms + "|");
            dessin.AppendLine(ligneBlanche + "|");
            dessin.AppendLine(bordure);
            return dessin.ToString();
        }

        private static string Centrer(string texte, int largeur)
        {
            int gauche = (largeur - texte.Length) / 2;
            return new string(' ', gauche) + texte + new string(' ', largeur - texte.Length - gauche);
        }
    }
}
