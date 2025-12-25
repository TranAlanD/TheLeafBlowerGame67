using UnityEngine;

public class LeafBlowerCollect : MonoBehaviour
{
    public GameObject leafBlower;
    public GameObject uiToActivate;
    public LeafBlowerMechanics leafBlowerMechanics; 
    public string playerTag = "Player";

    private void Start()
    {
        if (leafBlower != null)
            leafBlower.SetActive(false);

        if (uiToActivate != null)
            uiToActivate.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (leafBlower != null && !leafBlower.activeSelf)
                leafBlower.SetActive(true);

            if (leafBlowerMechanics != null)
                leafBlowerMechanics.isActivated = true;

            if (uiToActivate != null)
                StartCoroutine(ActivateTemporary(uiToActivate, 5f));

            GetComponent<Collider>().enabled = false;
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.enabled = false;
        }
    }

    private System.Collections.IEnumerator ActivateTemporary(GameObject obj, float duration)
    {
        Debug.Log("UI ACTIVATED");
        obj.SetActive(true);

        yield return new WaitForSecondsRealtime(duration);

        Debug.Log("UI DEACTIVATED");
        obj.SetActive(false);

        Destroy(gameObject);
    }
}
