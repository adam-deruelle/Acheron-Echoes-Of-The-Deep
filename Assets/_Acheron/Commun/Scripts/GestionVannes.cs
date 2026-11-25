using UnityEngine;
using UnityEngine.UIElements;

public class GestionVannes : MonoBehaviour
{

    public float valeurMaxPression = 0;
    public float angleXMin = 177;
    public float angleXMax = 0;
    public float angle;
    public float pression;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        // angle = transform.localEulerAngles.x;

        // a = (valeurMaxPression - 0) / (angleXMax - angleXMin);

        // b = valeurMaxPression - a * angleXMax;

        // pression = a * angle + b;

        Vector3 dir = transform.localRotation * Vector3.up;
        angle = Vector3.SignedAngle(Vector3.up, dir, Vector3.right);

        float t = Mathf.InverseLerp(angleXMin, angleXMax, angle);
        pression = t * valeurMaxPression;

    }
}
