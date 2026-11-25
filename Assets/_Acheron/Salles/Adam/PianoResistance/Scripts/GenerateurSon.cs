using System.Collections.Generic;
using UnityEngine;

namespace Acheron.Enigmes.Piano
{
    /// <summary>
    /// Fabrique des sons par code (AudioClip "procéduraux") : pas besoin de fichiers audio.
    /// Les clips sont gardés en mémoire (cache) pour n'être calculés qu'une seule fois.
    /// À remplacer plus tard par de vrais enregistrements si on veut.
    /// </summary>
    public static class GenerateurSon
    {
        private const int FrequenceEchantillonnage = 44100; // échantillons par seconde

        private static readonly Dictionary<NoteBlanche, AudioClip> cacheNotes = new Dictionary<NoteBlanche, AudioClip>();
        private static AudioClip cacheBuzzer;
        private static AudioClip cacheReussite;

        /// <summary>
        /// Son de piano simplifié pour une note : une fréquence fondamentale + quelques harmoniques,
        /// avec une attaque très courte puis une décroissance (comme une corde frappée).
        /// </summary>
        public static AudioClip Note(NoteBlanche note)
        {
            if (cacheNotes.TryGetValue(note, out AudioClip clip) && clip != null)
            {
                return clip;
            }

            const float duree = 1.6f;
            float frequence = OutilsPiano.Frequence(note);
            int nbEchantillons = Mathf.RoundToInt(duree * FrequenceEchantillonnage);
            float[] donnees = new float[nbEchantillons];

            // Volume relatif des harmoniques (1 = fondamentale, 2 = octave au-dessus, ...)
            float[] volumesHarmoniques = { 1f, 0.45f, 0.25f, 0.12f, 0.06f };

            for (int i = 0; i < nbEchantillons; i++)
            {
                float t = (float)i / FrequenceEchantillonnage; // temps en secondes
                float echantillon = 0f;

                for (int h = 0; h < volumesHarmoniques.Length; h++)
                {
                    int rang = h + 1;
                    // Les harmoniques aiguës s'éteignent plus vite que la fondamentale
                    float decroissance = Mathf.Exp(-t * (2.5f + 1.5f * rang));
                    echantillon += volumesHarmoniques[h] * decroissance * Mathf.Sin(2f * Mathf.PI * frequence * rang * t);
                }

                donnees[i] = echantillon * Enveloppe(t, duree, 0.004f, 0.05f);
            }

            Normaliser(donnees, 0.8f);
            clip = CreerClip("Note_" + note, donnees);
            cacheNotes[note] = clip;
            return clip;
        }

        /// <summary>Buzzer d'erreur : son grave et "grinçant" (onde carrée).</summary>
        public static AudioClip Buzzer()
        {
            if (cacheBuzzer != null) return cacheBuzzer;

            const float duree = 0.6f;
            int nbEchantillons = Mathf.RoundToInt(duree * FrequenceEchantillonnage);
            float[] donnees = new float[nbEchantillons];

            for (int i = 0; i < nbEchantillons; i++)
            {
                float t = (float)i / FrequenceEchantillonnage;
                // Deux ondes carrées proches (110 et 116 Hz) : le mélange crée un battement désagréable
                float carre1 = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 110f * t));
                float carre2 = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 116f * t));
                donnees[i] = (carre1 + carre2) * 0.5f * Enveloppe(t, duree, 0.01f, 0.08f);
            }

            Normaliser(donnees, 0.5f);
            cacheBuzzer = CreerClip("Buzzer", donnees);
            return cacheBuzzer;
        }

        /// <summary>Petit arpège joyeux (Do - Mi - Sol - Do aigu) pour la réussite.</summary>
        public static AudioClip Reussite()
        {
            if (cacheReussite != null) return cacheReussite;

            float[] frequences = { 523.25f, 659.25f, 783.99f, 1046.5f };
            const float ecartEntreNotes = 0.12f;
            const float dureeNote = 0.9f;
            float dureeTotale = ecartEntreNotes * (frequences.Length - 1) + dureeNote;
            int nbEchantillons = Mathf.RoundToInt(dureeTotale * FrequenceEchantillonnage);
            float[] donnees = new float[nbEchantillons];

            for (int n = 0; n < frequences.Length; n++)
            {
                int debut = Mathf.RoundToInt(n * ecartEntreNotes * FrequenceEchantillonnage);
                for (int i = debut; i < nbEchantillons; i++)
                {
                    float t = (float)(i - debut) / FrequenceEchantillonnage;
                    if (t > dureeNote) break;
                    donnees[i] += Mathf.Sin(2f * Mathf.PI * frequences[n] * t) * Mathf.Exp(-t * 4f) * Enveloppe(t, dureeNote, 0.005f, 0.05f);
                }
            }

            Normaliser(donnees, 0.7f);
            cacheReussite = CreerClip("Reussite", donnees);
            return cacheReussite;
        }

        // --------------------------------------------------------------------
        // Outils internes
        // --------------------------------------------------------------------

        /// <summary>
        /// Enveloppe de volume : monte de 0 à 1 pendant "attaque", puis redescend à 0 pendant "fin"
        /// (évite les "clics" au début et à la fin du son).
        /// </summary>
        private static float Enveloppe(float t, float duree, float attaque, float fin)
        {
            if (t < attaque) return t / attaque;
            if (t > duree - fin) return Mathf.Max(0f, (duree - t) / fin);
            return 1f;
        }

        /// <summary>Ramène le volume maximal du son à "volumeMax" (entre 0 et 1).</summary>
        private static void Normaliser(float[] donnees, float volumeMax)
        {
            float max = 0f;
            foreach (float d in donnees) max = Mathf.Max(max, Mathf.Abs(d));
            if (max <= 0f) return;

            float facteur = volumeMax / max;
            for (int i = 0; i < donnees.Length; i++) donnees[i] *= facteur;
        }

        private static AudioClip CreerClip(string nom, float[] donnees)
        {
            // 1 canal (mono), pas de streaming : le son est entièrement en mémoire
            AudioClip clip = AudioClip.Create(nom, donnees.Length, 1, FrequenceEchantillonnage, false);
            clip.SetData(donnees, 0);
            return clip;
        }
    }
}
