using UnityEngine;
using UnityEngine.AI;

public class Enemies : MonoBehaviour
{

    private NavMeshAgent agent;
    private Transform player;
    public float attack = 15;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player != null && agent.enabled)
        {
            agent.SetDestination(player.position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {


        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController pScript = collision.gameObject.GetComponent<PlayerController>();

            if (pScript != null)
            {
                pScript.TakeDamage(attack);
            }

            Destroy(gameObject);
        }
    }
}
