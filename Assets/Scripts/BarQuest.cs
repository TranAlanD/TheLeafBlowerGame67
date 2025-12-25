using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BarQuest : MonoBehaviour
{
    public DJQuest djQuest;
    private int questState = 0;
    public GameObject player;
    public GameObject barRoof;
    public DualDialogueActivator dialogue1;
    public DualDialogueActivator dialogue2;

    void Update() {
        if (questState == 0) {
            if (Vector3.Distance(player.transform.position, transform.position) < 4f) {
                questState = 1;
            }
        } else if (questState == 2) {
            if (Vector3.Distance(player.transform.position, transform.position) < 4f) {
                questState = 3;
            }
        } else if (questState == 3) {
            if (player.transform.position.y < 0) {
                SceneManager.LoadScene("MainMenu");
            }
        }

        if (djQuest.getQuestState() == 5) {
            questState = 2;
            barRoof.SetActive(true);
            dialogue1.enabled = false;
            dialogue2.enabled = true;
        }
    }
}
