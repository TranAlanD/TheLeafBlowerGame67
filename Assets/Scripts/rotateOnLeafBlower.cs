using UnityEngine;
using System.Collections;

public class RotateOnLeafBlower : MonoBehaviour
{
    public Transform pivotCenter;
    public float rotationDuration = 0.5f;
    public float cooldownTime = 0.5f; 

    private bool isRotating = false;
    private bool canRotate = true;

    public void RotateBlocks()
    {
        if (pivotCenter == null || isRotating || !canRotate)
            return;

        StartCoroutine(RotateStep());
    }

    IEnumerator RotateStep()
    {
        isRotating = true;
        canRotate = false;

        Quaternion startRot = pivotCenter.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 90f, 0f);

        float elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            pivotCenter.rotation = Quaternion.Slerp(startRot, endRot, elapsed / rotationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        pivotCenter.rotation = endRot;
        isRotating = false;

        yield return new WaitForSeconds(cooldownTime);
        canRotate = true;
    }
}
