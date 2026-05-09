using UnityEngine;
using UnityEngine.AI;

public class Enemies : MonoBehaviour
{

    private NavMeshAgent agent;
    private Transform player;
    public int damageAmount = 10;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (player != null && !agent.isStopped)
        {
            agent.SetDestination(player.position);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Player"))
        {
            ThirdPersonController playerScript = hit.gameObject.GetComponent<ThirdPersonController>();

            if (playerScript != null)
            {
                playerScript.TakeDamage(damageAmount);
                Die();
            }
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
