using UnityEngine;

public class DualDialogueActivator : MonoBehaviour
{
    public GameObject firstCanvas;
    public GameObject secondCanvas;
    public Transform player;
    public float activationRadius = 3f;

    private bool inRange = false;

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);
        inRange = distance <= activationRadius;

        if (inRange && !firstCanvas.activeSelf && !secondCanvas.activeSelf)
        {
            firstCanvas.SetActive(true);
        }
        else if (!inRange)
        {
            firstCanvas.SetActive(false);
        }

        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            if (firstCanvas.activeSelf)
                firstCanvas.SetActive(false);

            if (!secondCanvas.activeSelf)
                secondCanvas.SetActive(true);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
