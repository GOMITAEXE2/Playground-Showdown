using UnityEngine;
using UnityEngine.AI;

public class ComportamientoEnemigo : MonoBehaviour
{
    public Transform jugador; // Referencia al jugador
    public float rangoDeteccion = 15f; // Rango de detección
    public float distanciaDetenerse = 10f; // Distancia mínima para detenerse y disparar
    public SimpleShoot scriptDisparo; // Referencia al script de disparo
    public float radioPatrullaje = 20f; // Radio de patrullaje para seleccionar puntos aleatorios
    public float velocidadPatrullaje = 2f; // Velocidad de patrullaje
    public float velocidadPersecucion = 4f; // Velocidad de persecución
    public float intervaloDisparo = 1f; // Intervalo entre disparos en segundos
    public float tiempoDescanso = 3f; // Tiempo en segundos para descansar antes de reanudar el patrullaje

    private bool estaDisparando = false;
    private float ultimoTiempoDisparo = 0f; // Tiempo del último disparo
    private NavMeshAgent agente; // Componente NavMeshAgent
    private bool estaDescansando = false; // Bandera para controlar si el enemigo está descansando
    private float tiempoRestanteDescanso = 0f; // Tiempo restante del descanso

    void Start()
    {
        agente = GetComponent<NavMeshAgent>(); // Obtener el componente NavMeshAgent
        agente.speed = velocidadPatrullaje; // Configurar la velocidad de patrullaje por defecto
        Patrullar(); // Iniciar patrullaje al comenzar
    }

    void Update()
    {
        float distanciaAlJugador = Vector3.Distance(transform.position, jugador.position);

        if (distanciaAlJugador <= rangoDeteccion)
        {
            // Si el jugador está dentro del rango de visión, perseguir
            PerseguirJugador();
            Debug.Log("Persiguiendo al jugador");
        }
        else
        {
            // Si el jugador no está en rango, continuar patrullando
            Patrullar();
        }

        // Verificar si el enemigo está lo suficientemente cerca del jugador para detenerse y disparar
        if (distanciaAlJugador <= distanciaDetenerse)
        {
            DetenerYDisparar();
            Debug.Log("Disparando al jugador");
        }
        else
        {
            // Detener el disparo si el enemigo se aleja
            DetenerDisparoSiEsNecesario();
        }

        // Si está descansando, reducir el tiempo restante de descanso
        if (estaDescansando)
        {
            velocidadPatrullaje = 0f;
            tiempoRestanteDescanso -= Time.deltaTime;
            Debug.Log("Descansando" + tiempoRestanteDescanso);
            if (tiempoRestanteDescanso <= 0f)
            {
                velocidadPatrullaje = 2f;
                estaDescansando = false; // Terminar el descanso
                Patrullar(); // Reanudar el patrullaje
                Debug.Log("Patrullando");
            }
        }
    }

    void PerseguirJugador()
    {
        // Cambiar la velocidad a la de persecución
        agente.speed = velocidadPersecucion;

        // Mover al enemigo hacia la posición del jugador
        agente.SetDestination(jugador.position);

        // Mirar al jugador mientras lo persigue
        transform.LookAt(new Vector3(jugador.position.x, transform.position.y, jugador.position.z));
    }

    void DetenerYDisparar()
    {
        // Detener al enemigo para disparar si está dentro del rango
        agente.isStopped = true; // Detener el agente de navegación

        // Asegurarnos de que el enemigo no pase la distancia de "distanciaDetenerse"
        float distanciaAlJugador = Vector3.Distance(transform.position, jugador.position);
        if (distanciaAlJugador <= distanciaDetenerse)
        {
            // Mirar al jugador mientras lo persigue/dispara
            transform.LookAt(new Vector3(jugador.position.x, transform.position.y, jugador.position.z));

            // Verificar si ha pasado el intervalo de disparo
            if (Time.time - ultimoTiempoDisparo >= intervaloDisparo)
            {
                // Activar el disparo si no se está disparando
                if (!estaDisparando)
                {
                    estaDisparando = true;
                    scriptDisparo.StartShooting();
                    ultimoTiempoDisparo = Time.time; // Actualizar el tiempo del último disparo
                }
            }
        }
    }

    void DetenerDisparoSiEsNecesario()
    {
        // Si el enemigo ya no está disparando, detenerlo
        if (estaDisparando)
        {
            estaDisparando = false;
            scriptDisparo.StopShooting();
        }

        // Si el enemigo se aleja del jugador, reiniciar patrullaje
        if (Vector3.Distance(transform.position, jugador.position) > distanciaDetenerse)
        {
            agente.isStopped = false; // Reactivar el movimiento del agente
        }
    }

    void Patrullar()
    {
        // Cambiar la velocidad a la de patrullaje
        agente.speed = velocidadPatrullaje;

        // Si el enemigo ya está disparando, detener el disparo
        DetenerDisparoSiEsNecesario();

        if (!agente.pathPending && agente.remainingDistance < 0.5f)
        {
            // Si el enemigo ha llegado al destino, tomar una decisión aleatoria
            if (Random.value < 0.75f) // 75% de probabilidad de descansar
            {
                Descansar();
            }
            else
            {
                // Establecer un nuevo punto de patrullaje aleatorio
                Vector3 puntoPatrullajeAleatorio = ObtenerUbicacionNavMeshAleatoria();
                agente.SetDestination(puntoPatrullajeAleatorio);
            }
        }
    }

    void Descansar()
    {
        // Iniciar descanso
        estaDescansando = true;
        tiempoRestanteDescanso = tiempoDescanso; // Establecer el tiempo de descanso
        agente.isStopped = true; // Detener el agente durante el descanso
    }

    Vector3 ObtenerUbicacionNavMeshAleatoria()
    {
        // Obtener un punto aleatorio dentro del radio de patrullaje
        Vector3 direccionAleatoria = Random.insideUnitSphere * radioPatrullaje;
        direccionAleatoria += transform.position;

        NavMeshHit hit;
        NavMesh.SamplePosition(direccionAleatoria, out hit, radioPatrullaje, NavMesh.AllAreas);
        return hit.position; // Retornar el punto válido en el NavMesh
    }
}
