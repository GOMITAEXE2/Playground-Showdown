using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack, Rest }
    public State currentState;

    public float patrolRange = 20f; // Radio de búsqueda para puntos aleatorios
    public float chaseRange = 10f;
    public float attackRange = 2f;
    public float restDuration = 5f;
    public Color chaseColor = Color.red; // Color que cambiará en Chase
    public Color emissionColor = Color.red; // Color de emisión

    private Transform player;
    private NavMeshAgent agent;
    private Renderer enemyRenderer; // Referencia al Renderer
    private Material enemyMaterial; // Referencia al Material
    private Color originalColor; // Color original del enemigo
    private Color originalEmissionColor; // Color de emisión original
    private float stateTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemyRenderer = GetComponent<Renderer>(); // Obtenemos el Renderer
        enemyMaterial = enemyRenderer.material; // Guardamos una referencia al material
        originalColor = enemyMaterial.color; // Guardamos el color original
        originalEmissionColor = enemyMaterial.GetColor("_EmissionColor"); // Guardamos el color de emisión original
        currentState = State.Patrol;
        GoToRandomPatrolPoint();
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
            case State.Rest:
                Rest();
                break;
        }
    }

    private void Patrol()
    {
        if (!agent.isOnNavMesh) return;

        // Si llega al destino, genera un nuevo punto aleatorio
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToRandomPatrolPoint();
        }

        // Cambia a estado Chase si el jugador está dentro del rango
        if (Vector3.Distance(player.position, transform.position) <= chaseRange)
        {
            currentState = State.Chase;
        }
    }

    private void Chase()
    {
        if (!agent.isOnNavMesh) return;

        agent.SetDestination(player.position);

        // Cambia el color del enemigo cuando entra en estado de persecución
        enemyMaterial.color = chaseColor;

        // Cambiar la emisión (brillo)
        enemyMaterial.EnableKeyword("_EMISSION");
        enemyMaterial.SetColor("_EmissionColor", emissionColor * Mathf.LinearToGammaSpace(1.0f));

        if (Vector3.Distance(player.position, transform.position) <= attackRange)
        {
            currentState = State.Attack;
        }
        else if (Vector3.Distance(player.position, transform.position) > chaseRange)
        {
            currentState = State.Patrol;
            enemyMaterial.color = originalColor;
            enemyMaterial.SetColor("_EmissionColor", originalEmissionColor);
            GoToRandomPatrolPoint();
        }
    }

    private void Attack()
    {
        if (!agent.isOnNavMesh) return;

        agent.isStopped = true;
        Debug.Log("Atacando al jugador!");

        if (Vector3.Distance(player.position, transform.position) > attackRange)
        {
            agent.isStopped = false;
            currentState = State.Chase;
        }
        else if (Vector3.Distance(player.position, transform.position) > chaseRange)
        {
            agent.isStopped = false;
            currentState = State.Patrol;
            enemyMaterial.color = originalColor;
            enemyMaterial.SetColor("_EmissionColor", originalEmissionColor);
            GoToRandomPatrolPoint();
        }
    }

    private void Rest()
    {
        agent.isStopped = true;
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            currentState = State.Patrol;
            agent.isStopped = false;
            GoToRandomPatrolPoint();
        }
    }

    private void GoToRandomPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRange; // Genera un punto aleatorio dentro del rango
        randomDirection += transform.position; // Ajusta el punto aleatorio relativo a la posición actual

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position); // Establece el destino si es válido
        }
    }
}
