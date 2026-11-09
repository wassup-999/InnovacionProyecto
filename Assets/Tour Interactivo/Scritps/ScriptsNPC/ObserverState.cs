using UnityEngine;

public class ObserveState : IState
{
    private NPCController npc;
    private StateMachine stateMachine;

    private float timer;
    private bool hasArrived;

    public ObserveState(StateMachine stateMachine, NPCController npc)
    {
        this.stateMachine = stateMachine;
        this.npc = npc;
    }

    public void Enter()
    {
        timer = 0f;
        hasArrived = false;

        if (npc.LastObservedTarget != null && npc.Agent != null && npc.Agent.isOnNavMesh)
        {
            npc.Agent.isStopped = false;

            // Obtener el punto más cercano en la superficie del Collider
            Collider targetCollider = npc.LastObservedTarget.GetComponent<Collider>();
            Vector3 destinationPoint = npc.LastObservedTarget.position;

            if (targetCollider != null)
            {
                destinationPoint = targetCollider.ClosestPoint(npc.transform.position);
            }

            npc.Agent.SetDestination(destinationPoint);
        }
    }

    public void Update()
    {
        if (npc.LastObservedTarget == null)
        {
            stateMachine.ChangeState(npc.RoamState);
            return;
        }

        // FASE 1: Caminar hasta estar cerca del objetivo
        if (!hasArrived)
        {
            if (!npc.Agent.pathPending && npc.Agent.remainingDistance <= npc.Agent.stoppingDistance)
            {
                hasArrived = true;

                // Frenar al NPC al llegar
                npc.Agent.isStopped = true;
                npc.Agent.velocity = Vector3.zero;

                // Orientarlo hacia el objeto
                Vector3 lookDirection = npc.LastObservedTarget.position - npc.transform.position;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    npc.transform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }
        }
        // FASE 2: Esperar en la posición observando
        else
        {
            timer += Time.deltaTime;

            if (timer >= npc.ObserveDuration)
            {
                stateMachine.ChangeState(npc.RoamState);
            }
        }
    }

    public void Exit()
    {
        // Reanudar el NavMeshAgent
        if (npc.Agent != null && npc.Agent.isOnNavMesh)
        {
            npc.Agent.isStopped = false;
            npc.Agent.ResetPath();
        }

        // Limpia la referencia e inicia el cooldown de 5 segundos para este objetivo
        npc.FinishObserving();
    }
}