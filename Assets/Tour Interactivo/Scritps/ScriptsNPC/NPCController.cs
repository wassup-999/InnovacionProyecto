using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    [Header("Animation")]
    public Animator Animator;

    [Header("Detection & Observe")]
    public float DetectionRange = 10f;
    public float ObserveDuration = 4f;
    public string TargetTag = "Observar";

    [Header("Roam")]
    public float RoamRadius = 15f;
    public float RoamWaitTime = 2f;

    [Header("Dispersion")]
    public Vector3 HomePosition;
    public float MinSeparationFromOthers = 2.5f;

    [HideInInspector] public NavMeshAgent Agent;
    public StateMachine stateMachine;

    // Estados
    public RoamState RoamState;
    public ObserveState ObserveState;

    // Referencias a los objetos observados
    [HideInInspector] public Transform LastObservedTarget;
    private Transform previouslyObservedTarget;
    private float reDetectTimer = 0f;

    // Registro de todos los NPC para dispersión y separación
    public static readonly List<NPCController> All = new List<NPCController>();
    private static bool dispersionDone;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        HomePosition = transform.position;

        if (Agent != null)
        {
            // Evitar bloqueos mutuos en pasillos: los NPC no tienen collider físico
            Agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        if (!All.Contains(this))
        {
            All.Add(this);
        }

        // Si no se asignó manualmente en el inspector, lo busca en los hijos
        if (Animator == null)
        {
            Animator = GetComponentInChildren<Animator>();
        }
    }

    private void OnDestroy()
    {
        All.Remove(this);
    }

    void Start()
    {
        if (!dispersionDone)
        {
            dispersionDone = true;

            try
            {
                DisperseAll();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        stateMachine = new StateMachine();

        RoamState = new RoamState(stateMachine, this);
        ObserveState = new ObserveState(stateMachine, this);

        stateMachine.Initialize(RoamState);
    }

    void Update()
    {
        // 1. Actualizar la máquina de estados
        if (stateMachine != null)
        {
            stateMachine.Update();
        }

        // 2. Temporizador para evitar re-detectar de inmediato el mismo objeto visto
        if (reDetectTimer > 0)
        {
            reDetectTimer -= Time.deltaTime;
            if (reDetectTimer <= 0)
            {
                previouslyObservedTarget = null;
            }
        }

        // 3. Control de animaciones (Velocidad normalizada para Idle/Walk)
        if (Animator != null && Agent != null)
        {
            float normalizedSpeed = Agent.velocity.magnitude / Mathf.Max(Agent.speed, 0.01f);
            Animator.SetFloat("Speed", normalizedSpeed, 0.1f, Time.deltaTime);
        }
    }

    /// <summary>
    /// Distancia al NPC más cercano (excepto este) desde un punto dado.
    /// </summary>
    public float DistanceToNearestOtherNpc(Vector3 point)
    {
        float best = float.MaxValue;

        foreach (var other in All)
        {
            if (other == null || other == this) continue;

            float d = (other.transform.position - point).sqrMagnitude;
            if (d < best)
            {
                best = d;
            }
        }

        return best == float.MaxValue ? float.MaxValue : Mathf.Sqrt(best);
    }

    /// <summary>
    /// Reparte a todos los NPC en anclas (HomePosition) bien separadas sobre el NavMesh,
    /// reduciendo el radio de patrulla para que sus zonas no se solapen.
    /// </summary>
    public static void DisperseAll()
    {
        var npcs = UnityEngine.Object.FindObjectsByType<NPCController>(UnityEngine.FindObjectsSortMode.None);
        if (npcs == null || npcs.Length == 0) return;

        All.Clear();
        All.AddRange(npcs);

        var tri = NavMesh.CalculateTriangulation();
        if (tri.vertices == null || tri.vertices.Length == 0) return;

        Bounds bounds = new Bounds(tri.vertices[0], Vector3.zero);
        foreach (var v in tri.vertices)
        {
            bounds.Encapsulate(v);
        }

        float sampleY = npcs[0].transform.position.y;

        // Centroide del grupo (referencia para repartir y para filtrar conectividad)
        Vector3 centroid = Vector3.zero;
        foreach (var n in npcs) centroid += n.transform.position;
        centroid /= npcs.Length;

        NavMeshHit centerHit;
        Vector3 reachabilityOrigin = NavMesh.SamplePosition(centroid, out centerHit, 3f, NavMesh.AllAreas)
            ? centerHit.position
            : npcs[0].transform.position;

        // Generar puntos candidatos alcanzables (PathComplete) sobre el NavMesh
        var candidates = new List<Vector3>();
        var rnd = new System.Random(1337);

        for (int i = 0; i < 1500; i++)
        {
            float x = Mathf.Lerp(bounds.min.x + 1f, bounds.max.x - 1f, (float)rnd.NextDouble());
            float z = Mathf.Lerp(bounds.min.z + 1f, bounds.max.z - 1f, (float)rnd.NextDouble());
            Vector3 probe = new Vector3(x, sampleY, z);

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(probe, out hit, 1.5f, NavMesh.AllAreas))
            {
                continue;
            }

            NavMeshPath reachPath = new NavMeshPath();
            if (!NavMesh.CalculatePath(reachabilityOrigin, hit.position, NavMesh.AllAreas, reachPath)
                || reachPath.status != NavMeshPathStatus.PathComplete)
            {
                continue;
            }

            candidates.Add(hit.position);

            if (candidates.Count >= npcs.Length * 20)
            {
                break;
            }
        }

        if (candidates.Count == 0) return;

        // Ancla inicial: candidato más cercano al centroide de los NPC
        var anchors = new List<Vector3>();
        Vector3 first = candidates[0];
        float bestToCenter = float.MaxValue;
        foreach (var c in candidates)
        {
            float d = (c - centroid).sqrMagnitude;
            if (d < bestToCenter)
            {
                bestToCenter = d;
                first = c;
            }
        }
        anchors.Add(first);

        // Farthest point sampling: cada ancla nueva es la más alejada de las ya elegidas
        while (anchors.Count < npcs.Length && anchors.Count < candidates.Count)
        {
            Vector3 chosen = candidates[0];
            float bestMinDist = -1f;

            foreach (var c in candidates)
            {
                float minDist = float.MaxValue;
                foreach (var a in anchors)
                {
                    float d = (c - a).sqrMagnitude;
                    if (d < minDist) minDist = d;
                }

                if (minDist > bestMinDist)
                {
                    bestMinDist = minDist;
                    chosen = c;
                }
            }

            anchors.Add(chosen);
        }

        // Asignar a cada NPC un ancla distinta. Si un ancla no es alcanzable se penaliza,
        // pero todos reciben una para garantizar la dispersión.
        var pool = new List<Vector3>(anchors);

        foreach (var npc in npcs)
        {
            if (pool.Count == 0) break;

            int bestIndex = 0;
            float bestScore = float.MaxValue;

            for (int i = 0; i < pool.Count; i++)
            {
                float distance = Vector3.Distance(npc.transform.position, pool[i]);

                NavMeshPath path = new NavMeshPath();
                bool reachable = NavMesh.CalculatePath(npc.transform.position, pool[i], NavMesh.AllAreas, path)
                    && path.status == NavMeshPathStatus.PathComplete;

                float score = reachable ? distance : distance + 10000f;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            Vector3 anchor = pool[bestIndex];
            pool.RemoveAt(bestIndex);

            npc.HomePosition = anchor;

            if (npc.Agent != null)
            {
                npc.Agent.avoidancePriority = 20 + (bestIndex * 7) % 60;
            }
        }

        // Reducir el radio de patrulla según la distancia al ancla más cercana
        foreach (var npc in npcs)
        {
            float nearest = float.MaxValue;

            foreach (var other in npcs)
            {
                if (other == npc) continue;

                float d = Vector3.Distance(npc.HomePosition, other.HomePosition);
                if (d < nearest) nearest = d;
            }

            if (nearest < float.MaxValue)
            {
                npc.RoamRadius = Mathf.Clamp(nearest * 0.35f, 2f, npc.RoamRadius);
            }
        }
    }

    /// <summary>
    /// Revisa si un punto de destino está detrás del NPC (ángulo mayor a 135 grados).
    /// </summary>
    public bool IsTargetBehind(Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        directionToTarget.y = 0; // Ignoramos altura

        float angle = Vector3.Angle(transform.forward, directionToTarget);
        return angle > 135f;
    }

    /// <summary>
    /// Busca si hay un collider cercano con el TAG especificado que no sea el recién observado.
    /// </summary>
    public Transform DetectObserveTarget()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, DetectionRange);

        foreach (var col in hitColliders)
        {
            if (col.CompareTag(TargetTag))
            {
                // Solo si es un objeto distinto al último y no está en cooldown
                if (col.transform == LastObservedTarget || col.transform == previouslyObservedTarget)
                {
                    continue;
                }

                // No perseguir un objetivo que otro NPC ya está observando (evita agrupamientos)
                bool alreadyTaken = false;
                foreach (var other in All)
                {
                    if (other == null || other == this) continue;

                    if (other.LastObservedTarget == col.transform)
                    {
                        alreadyTaken = true;
                        break;
                    }
                }

                if (!alreadyTaken)
                {
                    return col.transform;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Limpia el objetivo actual y coloca un cooldown para no volver a observarlo de inmediato.
    /// </summary>
    public void FinishObserving()
    {
        previouslyObservedTarget = LastObservedTarget;
        LastObservedTarget = null;
        reDetectTimer = 5f; // Tiempo en segundos antes de poder re-observar el mismo objeto
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(HomePosition, RoamRadius);
    }
}
