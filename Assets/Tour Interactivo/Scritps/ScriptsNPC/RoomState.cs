using UnityEngine;
using UnityEngine.AI;

public class RoamState : IState
{
    private StateMachine stateMachine;
    private NPCController npc;

    private float waitTimer;
    private bool isWaiting;

    public RoamState(StateMachine stateMachine, NPCController npc)
    {
        this.stateMachine = stateMachine;
        this.npc = npc;
    }

    public void Enter()
    {
        isWaiting = false;
        SetNewDestination();
    }

    public void Update()
    {
        // 1. Detección del collider "Observar"
        Transform newTarget = npc.DetectObserveTarget();
        if (newTarget != null)
        {
            npc.LastObservedTarget = newTarget;
            stateMachine.ChangeState(npc.ObserveState);
            return;
        }

        // 2. Lógica de movimiento y espera en Roam
        if (!npc.Agent.pathPending && npc.Agent.remainingDistance <= npc.Agent.stoppingDistance)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = 0f;
            }

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
        // Limpiar el destino si cambia de estado
        if (npc.Agent != null && npc.Agent.isOnNavMesh)
        {
            npc.Agent.ResetPath();
        }
    }

    /// <summary>
    /// Genera un destino válido forzándolo a estar sobre la superficie del NavMesh.
    /// </summary>
    private void SetNewDestination()
    {
        Vector3 newPos = GetRandomNavMeshPosition(npc.transform.position, npc.RoamRadius);

        if (newPos != Vector3.zero)
        {
            npc.Agent.SetDestination(newPos);
        }
    }

    private Vector3 GetRandomNavMeshPosition(Vector3 center, float radius)
    {
        // Genera una dirección aleatoria solo en el plano horizontal (X, Z) para evitar puntos en el aire
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;

        NavMeshHit hit;
        // SamplePosition busca la posición válida en el NavMesh más cercana a 'randomDirection'
        // Usamos maxDistance = 5f para forzar que encuentre suelo debajo/cerca.
        if (NavMesh.SamplePosition(randomDirection, out hit, 5f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return Vector3.zero;
    }
}