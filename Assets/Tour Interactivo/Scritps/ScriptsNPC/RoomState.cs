using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RoamState : IState
{
    private StateMachine stateMachine;
    private NPCController npc;

    private float waitTimer;
    private bool isWaiting;
    private bool isTurning;
    private float walkTimeout;
    private float stuckTimer;

    public RoamState(StateMachine stateMachine, NPCController npc)
    {
        this.stateMachine = stateMachine;
        this.npc = npc;
    }

    public void Enter()
    {
        isWaiting = false;
        isTurning = false;
        walkTimeout = 0f;
        stuckTimer = 0f;
        SetNewDestination();
    }

    public void Update()
    {
        // 1. Si está ejecutando la corrutina del giro de 180°, no procesa el resto
        if (isTurning) return;

        // 2. Detección de colliders con el Tag especificado
        Transform newTarget = npc.DetectObserveTarget();
        if (newTarget != null)
        {
            npc.LastObservedTarget = newTarget;
            stateMachine.ChangeState(npc.ObserveState);
            return;
        }

        // 3. Llegada, espera y patrulla
        bool reached = !npc.Agent.pathPending && npc.Agent.hasPath &&
            npc.Agent.pathStatus != NavMeshPathStatus.PathInvalid &&
            npc.Agent.remainingDistance <= Mathf.Max(npc.Agent.stoppingDistance, 0.25f);

        bool noPath = !npc.Agent.pathPending && !npc.Agent.hasPath;

        if (!isWaiting)
        {
            walkTimeout += Time.deltaTime;

            // Detectar atascos (con path activo pero sin avanzar por otros NPC o geometría)
            bool hasActivePath = npc.Agent.hasPath && !npc.Agent.pathPending && !reached;
            if (hasActivePath && npc.Agent.velocity.sqrMagnitude < 0.04f)
            {
                stuckTimer += Time.deltaTime;
            }
            else
            {
                stuckTimer = 0f;
            }

            if (reached || noPath || walkTimeout >= 8f || stuckTimer >= 1.5f)
            {
                isWaiting = true;
                waitTimer = 0f;
                stuckTimer = 0f;
                if (npc.Agent.hasPath && npc.Agent.isOnNavMesh)
                {
                    npc.Agent.ResetPath();
                }
            }
        }
        else
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= npc.RoamWaitTime)
            {
                isWaiting = false;
                SetNewDestination();
            }
        }
    }

    public void Exit()
    {
        if (npc.Agent != null && npc.Agent.isOnNavMesh)
        {
            npc.Agent.ResetPath();
        }
    }

    private void SetNewDestination()
    {
        walkTimeout = 0f;
        stuckTimer = 0f;
        Vector3 newPos = GetRandomNavMeshPosition(npc.HomePosition, npc.RoamRadius);

        if (newPos == Vector3.zero)
        {
            isWaiting = true;
            waitTimer = 0f;
            return;
        }

        // Si el objetivo está a sus espaldas, realiza el giro de 180° primero
        if (npc.IsTargetBehind(newPos))
        {
            npc.StartCoroutine(ExecuteTurn180AndMove(newPos));
        }
        else
        {
            npc.Agent.SetDestination(newPos);
            isWaiting = false;
        }
    }

    private IEnumerator ExecuteTurn180AndMove(Vector3 destination)
    {
        isTurning = true;
        walkTimeout = 0f;
        npc.Agent.isStopped = true;
        bool previousUpdateRotation = npc.Agent.updateRotation;
        npc.Agent.updateRotation = false;

        // Disparar animación de giro en el Animator
        if (npc.Animator != null)
        {
            npc.Animator.SetTrigger("DoTurn180");
        }

        // Rotar gradualmente al NPC hacia el nuevo punto
        Vector3 direction = (destination - npc.transform.position).normalized;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            float turnDuration = 1.9f; // Duración real del clip Turn180
            float elapsed = 0f;

            while (elapsed < turnDuration)
            {
                elapsed += Time.deltaTime;
                npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, targetRotation, elapsed / turnDuration);
                yield return null;
            }

            npc.transform.rotation = targetRotation;
        }

        npc.Agent.updateRotation = previousUpdateRotation;
        npc.Agent.isStopped = false;
        npc.Agent.SetDestination(destination);
        isWaiting = false;
        isTurning = false;
    }

    private Vector3 GetRandomNavMeshPosition(Vector3 center, float radius)
    {
        Vector3 fallback = Vector3.zero;

        for (int i = 0; i < 12; i++)
        {
            // Punto aleatorio solo en el plano horizontal (X, Z)
            Vector2 flatOffset = Random.insideUnitCircle * radius;
            Vector3 randomPoint = new Vector3(center.x + flatOffset.x, center.y, center.z + flatOffset.y);

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(randomPoint, out hit, 3f, NavMesh.AllAreas))
            {
                continue;
            }

            // Solo aceptar destinos con un camino completo y alcanzable
            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(npc.transform.position, hit.position, NavMesh.AllAreas, path)
                || path.status != NavMeshPathStatus.PathComplete)
            {
                continue;
            }

            if (fallback == Vector3.zero)
            {
                fallback = hit.position;
            }

            // Preferir destinos alejados de los otros NPC para dispersarse
            if (npc.DistanceToNearestOtherNpc(hit.position) >= npc.MinSeparationFromOthers)
            {
                return hit.position;
            }
        }

        return fallback;
    }
}
