using UnityEngine;

/// <summary>
/// SIMPLE TEST - Detects player and F key press
/// </summary>
public class TestMom : MonoBehaviour
{
    public float range = 5f;
    private Transform player;

    void Start()
    {
        Debug.LogError("===== TEST MOM STARTED =====");
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.LogError("Found player: " + playerObj.name);
        }
        else
        {
            Debug.LogError("PLAYER NOT FOUND! Check Player tag!");
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= range)
        {
            Debug.LogError($"PLAYER IN RANGE! Distance: {distance:F2}m");

            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.LogError("========== F KEY PRESSED NEAR MOM! ==========");
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
