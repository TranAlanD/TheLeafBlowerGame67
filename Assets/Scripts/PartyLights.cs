using UnityEngine;

public class PartyLights : MonoBehaviour
{
    public Light directionalLight;
    public float speed;
    private int state = 1;
    // (1, blue go up) (2, green go down) (3, red go up) (4, blue go down) (5, green go up) (6, red go down)

    void Update()
    {
        Color c = directionalLight.color;

        if (state == 1) {
            if (c.b >= 1f) state = 2;
            c.b += speed;
        }

        if (state == 2) {
            if (c.g <= 0f) state = 3;
            c.g -= speed;
        }

        if (state == 3) {
            if (c.r >= 1f) state = 4;
            c.r += speed;
        }

        if (state == 4) {
            if (c.b <= 0f) state = 5;
            c.b -= speed;
        }

        if (state == 5) {
            if (c.g >= 1f) state = 6;
            c.g += speed;
        }

        if (state == 6) {
            if (c.r <= 0f) state = 1;
            c.r -= speed;
        }

        directionalLight.color = c;
    }
}
