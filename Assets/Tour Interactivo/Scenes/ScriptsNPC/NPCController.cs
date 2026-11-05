using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    [Header("Detection & Observe")]
    public float DetectionRange = 10f;
    public float ObserveDuration = 4f;
    public string TargetColliderName = "Observar";

    [Header("Roam")]
    public float RoamRadius = 15f;
    public float RoamWaitTime = 2f;

    [HideInInspector] public NavMeshAgent Agent;
    public StateMachine stateMachine;

    // Estados
    public RoamState RoamState;
    public ObserveState ObserveState;

    // Referencia al último objeto observado para no repetirlo de inmediato
    [HideInInspector] public Transform LastObservedTarget;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        stateMachine = new StateMachine();

        RoamState = new RoamState(stateMachine, this);
        ObserveState = new ObserveState(stateMachine, this);

        stateMachine.Initialize(RoamState);
    }

    void Update()
    {
        stateMachine.Update();
    }

    /// <summary>
    /// Busca si hay un collider cercano con el nombre 'Observar' que no sea el último que vio.
    /// </summary>
    public Transform DetectObserveTarget()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, DetectionRange);

        foreach (var col in hitColliders)
        {
            if (col.gameObject.name == TargetColliderName)
            {
                // Si es un objeto nuevo (diferente al último observado), lo detectamos
                if (col.transform != LastObservedTarget)
                {
                    return col.transform;
                }
            }
        }
        return null;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);

        Gizmos.color = Color.purple;
        Gizmos.DrawWireSphere(transform.position, RoamRadius);
    }
}