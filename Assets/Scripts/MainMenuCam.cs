using UnityEngine;

public class MainMenuCam: MonoBehaviour
{
    private int state = -1;

    void Update() {
        transform.Rotate(state * 0.01f, 0, 0);

        float xRot = transform.eulerAngles.x;
        if (xRot > 180) xRot -= 360;

        if (xRot < -70) {
            state = 1;
        }
        if (xRot > 20) {
            state = -1;
        }
    }
}
