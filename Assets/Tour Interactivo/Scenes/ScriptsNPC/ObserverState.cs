using Unity.VisualScripting;
using UnityEngine;

public class ObserveState : IState
{
    private NPCController npc;
    private StateMachine stateMachine;
    private float timer;

    public ObserveState(StateMachine stateMachine, NPCController npc)
    {
        this.stateMachine = stateMachine;
        this.npc = npc;
    }

    public void Enter()
    {
        timer = 0f;

        // Detener el movimiento en el NavMeshAgent
        if (npc.Agent != null && npc.Agent.isOnNavMesh)
        {
            npc.Agent.isStopped = true;
            npc.Agent.velocity = Vector3.zero;
        }

        // Opcional: Rotar suavemente hacia el objeto a observar
        if (npc.LastObservedTarget != null)
        {
            Vector3 lookDirection = npc.LastObservedTarget.position - npc.transform.position;
            lookDirection.y = 0; // Evitar inclinaciones verticales
            if (lookDirection != Vector3.zero)
            {
                npc.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    public void Update()
    {
        timer += Time.deltaTime;

        // Tras cumplir los 4 segundos, vuelve al estado de patrulla
        if (timer >= npc.ObserveDuration)
        {
            stateMachine.ChangeState(npc.RoamState);
        }
    }

    public void Exit()
    {
        // Reanudar el movimiento al salir del estado
        if (npc.Agent != null && npc.Agent.isOnNavMesh)
        {
            npc.Agent.isStopped = false;
        }
    }
}