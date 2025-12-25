using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LeafProjectile: MonoBehaviour
{
    public float explosionRadius = 20f;
    public float explosionForce = 500000f;
    public float upwardModifier = 0.5f;
    private int fogIncreaseState = 0;
    private Light directionalLight;
    private AudioSource tune;
    private AudioSource reverbTune;

    bool armed = false;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.02f);
        armed = true;
        tune = GameObject.Find("Tune").GetComponent<AudioSource>();
        reverbTune = GameObject.Find("ReverbTune").GetComponent<AudioSource>();
        reverbTune.volume = 0.01f;
        directionalLight = GameObject.Find("Directional Light").GetComponent<Light>();
    }

    void Update() {
        if (fogIncreaseState == 1) {
            directionalLight.intensity += directionalLight.intensity * 0.0088f;
            tune.volume -= reverbTune.volume * 0.02f;
            reverbTune.volume += reverbTune.volume * 0.02f;
            RenderSettings.fogDensity += RenderSettings.fogDensity * 0.01f;
            if (RenderSettings.fogDensity >= 0.34f) {
                fogIncreaseState = 0;
            }
        }
        if (fogIncreaseState == -1) {
            RenderSettings.fogDensity -= Time.deltaTime;
            if (RenderSettings.fogDensity <= 0.03f) {
                fogIncreaseState = 0;
            }
        }
    }

    void OnCollisionEnter(Collision collision) {
        if (!armed) return;
        if (!collision.collider.CompareTag("Player")) {
            // Get all colliders in the explosion radius
            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

            // Destroy the ball
        }
        if (collision.collider.CompareTag("Fire")) {
            TriggerFog();
            this.GetComponent<Rigidbody>().position = new Vector3(0, -500, 0);
            Destroy(gameObject, 20f);
        } else {
            Destroy(gameObject, 0.02f);
        }
    }

    public void TriggerFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogDensity = 0.001f;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(120f/255f, 120f/255f, 100f/255f);
        fogIncreaseState = 1;

        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Blowable");
        foreach (GameObject obj in allObjects)
        {
            obj.transform.localScale = new Vector3(3f, 3f, 1f);
        }
    }
}
