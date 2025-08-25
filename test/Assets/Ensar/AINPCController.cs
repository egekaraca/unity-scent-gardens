using UnityEngine;
using UnityEngine.AI;

public class AINPCController : MonoBehaviour
{
    public Animator animator;
    public float walkRadius = 10f;   // NPC'nin dolaþacaðý alan yarýçapý
    public float idleTime = 3f;      // Durduðu zaman bekleme süresi

    private NavMeshAgent agent;
    private float idleTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToRandomPoint();
    }

    void Update()
    {
        // Hedefe ulaþtýysa bekleme
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            animator.SetBool("isWalking", false);
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleTime)
            {
                GoToRandomPoint();
                idleTimer = 0f;
            }
        }
        else
        {
            animator.SetBool("isWalking", true);
        }
    }

    void GoToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, walkRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
