using UnityEngine;
using UnityEngine.AI;

public class AINPCController : MonoBehaviour
{
    public Animator animator;
    public float walkRadius = 10f;   // NPC'nin dola�aca�� alan yar��ap�
    public float idleTime = 3f;      // Durdu�u zaman bekleme s�resi

    private NavMeshAgent agent;
    private float idleTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToRandomPoint();
    }

    void Update()
    {
        // Hedefe ula�t�ysa bekleme
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
