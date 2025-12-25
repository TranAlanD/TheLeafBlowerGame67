using UnityEngine;

public class LeavesDestroyed : MonoBehaviour
{
    public Transform block;          
    public float lowerAmount = 3f;   
    public float lowerSpeed = 2f;    

    public int totalLeaves = 3;      

    private int leavesDestroyed = 0;
    private bool hasLowered = false;

    public void LeafDestroyed()
    {
        if (hasLowered) return;

        leavesDestroyed++;

        if (leavesDestroyed >= totalLeaves)
        {
            StartCoroutine(LowerBlock());
            hasLowered = true;
        }
    }

    private System.Collections.IEnumerator LowerBlock()
    {
        Vector3 startPos = block.position;
        Vector3 targetPos = startPos - new Vector3(0, lowerAmount, 0);
        float elapsed = 0f;
        float duration = lowerAmount / lowerSpeed;

        while (elapsed < duration)
        {
            block.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        block.position = targetPos;
    }
}
