using UnityEngine;

public class LeafManager : MonoBehaviour
{
    public LeavesDestroyed leafTracker;
    public GameObject[] waterBlocks;

    private void OnMouseDown()
    {
        RemoveLeaf();
    }

    private void RemoveLeaf()
    {
        if (leafTracker != null)
            leafTracker.LeafDestroyed();

        if (waterBlocks != null)
        {
            foreach (GameObject block in waterBlocks)
                block.SetActive(true);
        }

        gameObject.SetActive(false);
    }
}
