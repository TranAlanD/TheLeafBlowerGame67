using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public Camera playerCamera;     
    public Camera fallingCamera;     
    public GameObject playerCharacter; 

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            fallingCamera.gameObject.SetActive(false);

            playerCamera.gameObject.SetActive(true);

            if (playerCharacter != null)
                playerCharacter.SetActive(true);
        }
    }
}
