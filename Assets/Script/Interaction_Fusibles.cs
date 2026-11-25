using UnityEngine;

public class InteractionFusible : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 positionInitiale;

    [Header("Réglages du Slot")]
    public Transform slotCible; // Glissez l'objet "Zone_Slot_1" ici dans l'émetteur
    public float distanceDeClipsage = 0.2f; // Distance à laquelle le fusible se clipse tout seul

    void Start()
    {
        // On sauvegarde la position de départ au sol
        positionInitiale = transform.position;
    }

    void OnMouseDown()
    {
        // Quand on clique sur le fusible
        isDragging = true;
        offset = transform.position - ObtenirPositionSouris3D();
    }

    void OnMouseDrag()
    {
        // Quand on maintient le clic et qu'on bouge la souris
        if (isDragging)
        {
            transform.position = ObtenirPositionSouris3D() + offset;
        }
    }

    void OnMouseUp()
    {
        // Quand on relâche le bouton de la souris
        isDragging = false;

        // On vérifie la distance entre le fusible et son slot dans le disjoncteur
        float distance = Vector3.Distance(transform.position, slotCible.position);

        if (distance <= distanceDeClipsage)
        {
            // Étape finale : Le fusible se met parfaitement en place et se bloque
            transform.position = slotCible.position;
            transform.rotation = slotCible.rotation;
            GetComponent<Rigidbody>().isKinematic = true; // Désactive la physique pour le bloquer
            Debug.Log("Fusible correctement branché !");
        }
        else
        {
            // Si on rate le slot, le fusible retourne par terre
            transform.position = positionInitiale;
        }
    }

    private Vector3 ObtenirPositionSouris3D()
    {
        Vector3 positionSouris = Input.mousePosition;
        positionSouris.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(positionSouris);
    }
}

