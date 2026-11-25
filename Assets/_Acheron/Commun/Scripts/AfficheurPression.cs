using TMPro;
using UnityEngine;

public class AfficheurPression : MonoBehaviour
{
    public GestionVannes vanne1, vanne2, vanne3;      
    public TMP_Text texte;
    public bool pressionBon, pressionDanger;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    void Update()
    {
        // Affiche la pression avec 1 décimale et l'unité
        float pressionTotale = vanne1.pression + vanne2.pression + vanne3.pression;
        texte.text = pressionTotale.ToString("F0");

        if (pressionTotale > 620)
        {
            texte.color = Color.red;
            pressionBon = false;
            pressionDanger = true;
        }
        else if (pressionTotale <= 620 && pressionTotale >= 580)
        {
            texte.color = Color.green;
            pressionBon = true;
            pressionDanger = false;
        } else
        {
            texte.color = new Color(1f, 0.5f, 0f); // orange;
            pressionBon = false;
            pressionDanger = false;
        }

    }
}