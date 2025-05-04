using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
[DisallowMultipleComponent]
public class animal : MonoBehaviour
{
    private Transform player;
    public float detectionRadius = 10f;
    public float runAwayDistance = 15f;
    public float wanderRadius = 10f;

    public bool attackPlayer = false;
    // public float wanderTime = 5f;

    private NavMeshAgent agent;
    private Rigidbody rb;
    // private float timer;

    void Start()
    {
        agent = transform.GetComponent<NavMeshAgent>();
        rb = transform.GetComponent<Rigidbody>();
        // timer = wanderTime;
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;


    }

    void FixedUpdate()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRadius)
        {
            // TODO add animale to face the player
            if (attackPlayer) agent.SetDestination(player.position-(new Vector3(0.5f, 0.5f, 0.5f)));
            else RunAwayFromPlayer();
        }
        else
        {
            WanderRandomly();
        }
    }

    void RunAwayFromPlayer()
    {
        // Debug.Log("Run away from player");
        Vector3 runDirection = transform.position + (transform.position - player.position).normalized * runAwayDistance;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(runDirection, out hit, runAwayDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            
            // agent.Move(agent.desiredVelocity * Time.deltaTime);
        }
    }

    void WanderRandomly()
    {
        // timer -= Time.deltaTime;
        // if (timer <= 0f || !agent.hasPath)
        // {
        // Debug.Log("Wander randomly");
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
            
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            // agent.Move(agent.desiredVelocity * Time.deltaTime);
        }
            // timer = wanderTime;
        // }
    }

    void OnTriggerEnter(Collider triggerObject)
    {
        Debug.Log("ready to trigger");
        if (triggerObject.CompareTag("Player") && attackPlayer)
        {
            agent.isStopped = true;
            Debug.Log("trigger");
            StartCoroutine(WaitTimer());
        }
    }

    IEnumerator WaitTimer(){
        yield return new WaitForSeconds(5);
        agent.isStopped = false;
    }

}
