using UnityEngine;

public class CoinCollect : MonoBehaviour
{
    public AudioClip pickupSound;
    public float destroyDelay = 0.2f; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;

            Destroy(gameObject, destroyDelay);
        }
    }
}
