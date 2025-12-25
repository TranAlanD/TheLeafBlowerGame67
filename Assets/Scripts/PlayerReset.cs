using UnityEngine;

public class PlayerReset: MonoBehaviour
{
    private Vector3 startPos;
    private Quaternion startRot;

    void Start() {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    void Update() {
        if (transform.position.y - startPos.y < -20) {
            transform.position = startPos;
            transform.rotation = startRot;
        }
    }
}
