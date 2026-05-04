using UnityEngine;
using UnityEngine.AI;

public class Enemies : MonoBehaviour
{

    private NavMeshAgent agent;
    private Transform player;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;


        agent.speed = Random.Range(3f, 7f);
        agent.acceleration = Random.Range(6f, 12f);
        agent.angularSpeed = Random.Range(100f, 300f);
        agent.stoppingDistance =  Random.Range(1f, 2.5f);
        agent.avoidancePriority = Random.Range(0, 99);
    }

    void Update()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
        }
    }
}
