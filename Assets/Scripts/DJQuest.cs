using UnityEngine;
using System.Collections;

public class DJQuest : MonoBehaviour
{
    private int questState = 0;
    public GameObject player;
    public GameObject microphoneHead;
    public GameObject soundForce;
    public DualDialogueActivator welcomeTalk;
    public DualDialogueActivator speakInMicTalk;
    public DualDialogueActivator barOpenTalk;
    private float timer = 0;

    void Update() {
        if (questState == 0) {
            welcomeTalk.enabled = true;
            if (Vector3.Distance(player.transform.position, transform.position) < 3f) {
                //play AudioClip
                questState = 1;
                Debug.Log(questState);
            }
        } else if (questState == 1) {
            if (RenderSettings.fog) {
                welcomeTalk.enabled = false;
                speakInMicTalk.enabled = true;
                questState = 2;
            }
        } else if (questState == 2) {
            if (Vector3.Distance(player.transform.position, transform.position) < 3f) {
                //play AudioClip
                questState = 3;
            }
        } else if (questState == 3) {
            if (Vector3.Distance(player.transform.position, microphoneHead.transform.position) < 2f) {
                if (Input.GetMouseButtonDown(0)) {
                    RenderSettings.fogDensity = 0.01f;
                    speakInMicTalk.enabled = false;
                    barOpenTalk.enabled = true;
                    //play AudioClip
                    soundForce.GetComponent<Rigidbody>().AddForce(new Vector3(0, 2, 30), ForceMode.VelocityChange);
                    questState = 4;
                }
            }
        } else if (questState == 4) {
            timer += Time.deltaTime;
            if (timer > 5) {
                questState = 5;
                barOpenTalk.enabled = false;
            }
        } else if (questState == 5) {
            //
        }
    }

    public int getQuestState() {
        return questState;
    }
}
