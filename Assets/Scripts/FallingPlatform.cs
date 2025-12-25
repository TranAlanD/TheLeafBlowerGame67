using UnityEngine;

public class FallingPlatform: MonoBehaviour
{
    private Vector3 startingPosition;
    private int state = 1;
    // (1, not falling) (2, falling)

    void Start() {
        startingPosition = transform.position;
    }

    void Update() {
        if (state == 2) {
            if (transform.position.y - startingPosition.y > -10) {
                transform.position -= new Vector3(0, 0.005f, 0);
            } else {
                transform.position = startingPosition;
                state = 1;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && state == 1)
            state = 2;
    }
}
