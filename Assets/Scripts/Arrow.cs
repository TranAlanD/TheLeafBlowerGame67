using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float destroyDistance = 2f;

    void Update()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist <= destroyDistance)
                Destroy(gameObject);
        }
    }
}
