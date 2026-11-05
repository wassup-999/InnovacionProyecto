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

        // Si tenemos un punto objetivo registrado, le pedimos al NavMeshAgent que vaya hacia él
        if (npc.LastObservedTarget != null && npc.Agent != null && npc.Agent.isOnNavMesh)
        {
            npc.Agent.isStopped = false;
            npc.Agent.SetDestination(npc.LastObservedTarget.position);
        }
    }

    public void Update()
    {
        if (npc.LastObservedTarget == null)
        {
            stateMachine.ChangeState(npc.RoamState);
            return;
        }

        // FASE 1: Caminar hasta estar tocando / muy cerca del collider objetivo
        if (!hasArrived)
        {
            // Comprobamos si ya llegó al destino asignado
            if (!npc.Agent.pathPending && npc.Agent.remainingDistance <= npc.Agent.stoppingDistance)
            {
                hasArrived = true;

                // Frenar al NPC una vez llega a la marca
                npc.Agent.isStopped = true;
                npc.Agent.velocity = Vector3.zero;

                // Orientarlo para que mire directo al objeto
                Vector3 lookDirection = npc.LastObservedTarget.position - npc.transform.position;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    npc.transform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }
        }
        // FASE 2: Contar los 4 segundos en el sitio
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
        // Reanudar al agente para la patrulla
        if (npc.Agent != null && npc.Agent.isOnNavMesh)
        {
            npc.Agent.isStopped = false;
        }
    }
}