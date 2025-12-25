using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public interface IBlowable { void Blow(Vector3 impulse, float ragdollSeconds = 0.8f); }

public class SpiderRagdoll : MonoBehaviour, IBlowable
{
    public Animator animator;
    public NavMeshAgent agent;
    public Rigidbody rootRB;

    [Header("Auto-collected")]
    public List<Rigidbody> limbRBs = new();
    public List<Collider> limbCols = new();

    [Header("Recover")]
    public float standUpDelay = 0.6f;   // extra settle time after ragdoll
    public string recoverStateName = ""; // optional: play a get-up anim

    bool ragdolled;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!rootRB) rootRB = GetComponent<Rigidbody>();

        foreach (var rb in GetComponentsInChildren<Rigidbody>())
        {
            if (rb == rootRB) continue;
            limbRBs.Add(rb);
            var col = rb.GetComponent<Collider>();
            if (col) limbCols.Add(col);
            // default them OFF
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            if (col) col.enabled = false;
        }
    }

    public void Blow(Vector3 impulse, float ragdollSeconds = 0.8f)
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        StartCoroutine(CoRagdollFor(ragdollSeconds, impulse));
    }

    IEnumerator CoRagdollFor(float seconds, Vector3 impulse)
    {
        EnterRagdoll();
        // apply impulse to a couple of limbs and root
        rootRB.AddForce(impulse, ForceMode.Impulse);
        foreach (var rb in limbRBs)
            rb.AddForce(impulse * 0.35f, ForceMode.Impulse);

        yield return new WaitForSeconds(seconds + standUpDelay);
        ExitRagdoll();
    }

    void EnterRagdoll()
    {
        if (ragdolled) return;
        ragdolled = true;

        // turn off planner & animator
        if (agent) agent.enabled = false;
        if (animator) animator.enabled = false;

        // free limbs
        foreach (var rb in limbRBs) rb.isKinematic = false;
        foreach (var c in limbCols) c.enabled = true;
    }

    void ExitRagdoll()
    {
        if (!ragdolled) return;
        ragdolled = false;

        // freeze limbs again
        foreach (var rb in limbRBs) rb.isKinematic = true;
        foreach (var c in limbCols) c.enabled = false;

        // snap agent to current root pos and re-enable
        if (agent)
        {
            Vector3 pos = rootRB.position;
            if (!agent.isOnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(pos, out hit, 2.0f, agent.areaMask))
                    pos = hit.position;
            }
            agent.Warp(pos);
            agent.enabled = true;
        }

        if (animator)
        {
            animator.enabled = true;
            if (!string.IsNullOrEmpty(recoverStateName))
                animator.Play(recoverStateName, 0, 0f);
        }
    }
}
