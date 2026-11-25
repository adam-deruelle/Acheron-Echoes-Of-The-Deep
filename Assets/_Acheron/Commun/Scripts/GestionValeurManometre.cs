using UnityEngine;

public class GestionValeurManometre : MonoBehaviour
{
    public GameObject objetVanne; // à glisser dans l'Inspector
    private GestionVannes vanne;

    private float valPression = 0;

    // Points mesurés (angles dépliés, sans saut de 360°)
    private readonly float[] bars = { 0f, 100f, 200f, 300f, 400f , 500f};
    private readonly float[] angles = { 45f, 12f, -58f, -132f, -211f , -270f};

    void Start()
    {
        vanne = objetVanne.GetComponent<GestionVannes>();
    }

    void Update()
    {
        valPression = vanne.pression;

        // Limite la pression à la plage du cadran
        float p = Mathf.Clamp(valPression, 0f, 500f);

        // Segment de 100 bar concerné (0 à 3)
        int i = Mathf.Min((int)(p / 100f), 4);
        float t = (p - bars[i]) / 100f;

        // Interpolation linéaire entre les deux points mesurés
        float angle = Mathf.Lerp(angles[i], angles[i + 1], t);

        // Rotation en Z uniquement
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}

