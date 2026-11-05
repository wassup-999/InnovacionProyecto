using Unity.VisualScripting;
using UnityEngine;

public class RoamState : IState
{
    private StateMachine stateMachine;
    private NPCController npc;

    public RoamState(StateMachine stateMachine, NPCController npc)
    {
        this.stateMachine = stateMachine;
        this.npc = npc;
    }

    public void Enter()
    {
        // Lógica que se ejecuta al entrar en el estado de "Roam"
        Debug.Log("Entrando en el estado Roam");
        npc.Agent.isStopped = false; // Asegurarse de que el NPC pueda moverse
        npc.Agent.SetDestination(npc.transform.position + Random.insideUnitSphere * npc.RoamRadius);
    }

    public void Update()
    {
        // Chequear en cada frame si entra al rango un nuevo collider "Observar"
        Transform newTarget = npc.DetectObserveTarget();
        if (newTarget != null)
        {
            npc.LastObservedTarget = newTarget; // Guardar referencia
            stateMachine.ChangeState(npc.ObserveState);
            return;
        }

        // --- Tu lógica habitual de Roam / Pathfinding ---
        if (!npc.Agent.pathPending && npc.Agent.remainingDistance <= npc.Agent.stoppingDistance)
        {
            // Esperar un tiempo antes de moverse nuevamente
            npc.Invoke(nameof(StartRoaming), npc.RoamWaitTime);
        }
    }

    public void Exit()
    {
        // Lógica que se ejecuta al salir del estado de "Roam"
        Debug.Log("Saliendo del estado Roam");
        npc.Agent.isStopped = true; // Detener al NPC
    }

    private void StartRoaming()
    {
        npc.Agent.SetDestination(npc.transform.position + Random.insideUnitSphere * npc.RoamRadius);
    }
}