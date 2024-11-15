using UnityEngine;
using TMPro;

public class ArmaConfiguracionesScript : MonoBehaviour
{
    // bala
    public GameObject bala;

    // fuerza de la bala
    public float fuerzaDisparo, fuerzaVertical;

    // Estadísticas del arma
    public float tiempoEntreDisparos, dispersión, tiempoRecarga, tiempoEntreTiros;
    public int tamañoCargador, balasPorPulsación;
    public bool permitirPresionContinua;

    int balasRestantes, balasDisparadas;

    // Retroceso
    public Rigidbody rbJugador;
    public float fuerzaRetroceso;

    // Boleanos
    bool disparando, listoParaDisparar, recargando;

    // Referencias
    public Camera camaraFPS;
    public Transform puntoAtaque;

    // Gráficos
    public GameObject destelloDisparo;
    public TextMeshProUGUI displayMunicion;

    // Corrección de errores :D
    public bool permitirInvoke = true;

    private void Awake()
    {
        // asegurar que el cargador esté lleno
        balasRestantes = tamañoCargador;
        listoParaDisparar = true;
    }

    private void Update()
    {
        MiEntrada();

        // Establecer el display de munición, si existe :D
        if (displayMunicion != null)
            displayMunicion.SetText(balasRestantes / balasPorPulsación + " / " + tamañoCargador / balasPorPulsación);
    }
    private void MiEntrada()
    {
        // Verificar si se permite mantener presionado el botón y tomar la entrada correspondiente
        if (permitirPresionContinua) disparando = Input.GetKey(KeyCode.Mouse0);
        else disparando = Input.GetKeyDown(KeyCode.Mouse0);

        // Recargar
        if (Input.GetKeyDown(KeyCode.R) && balasRestantes < tamañoCargador && !recargando) Recargar();
        // Recargar automáticamente al intentar disparar sin munición
        if (listoParaDisparar && disparando && !recargando && balasRestantes <= 0) Recargar();

        // Disparar
        if (listoParaDisparar && disparando && !recargando && balasRestantes > 0)
        {
            // Establecer las balas disparadas a 0
            balasDisparadas = 0;

            Disparar();
        }
    }

    private void Disparar()
    {
        listoParaDisparar = false;

        // Encontrar la posición exacta del impacto usando un raycast
        Ray ray = camaraFPS.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Solo un rayo a través del medio de tu vista actual
        RaycastHit impacto;

        // verificar si el rayo golpea algo
        Vector3 puntoObjetivo;
        if (Physics.Raycast(ray, out impacto))
            puntoObjetivo = impacto.point;
        else
            puntoObjetivo = ray.GetPoint(75); // Solo un punto lejos del jugador

        // Calcular la dirección desde el punto de ataque hasta el punto objetivo
        Vector3 direcciónSinDispersión = puntoObjetivo - puntoAtaque.position;

        // Calcular la dispersión
        float x = Random.Range(-dispersión, dispersión);
        float y = Random.Range(-dispersión, dispersión);

        // Calcular nueva dirección con dispersión
        Vector3 direcciónConDispersión = direcciónSinDispersión + new Vector3(x, y, 0); // Solo añade dispersión a la última dirección

        // Instanciar bala/proyectil
        GameObject balaActual = Instantiate(bala, puntoAtaque.position, Quaternion.identity); // almacenar la bala instanciada en balaActual
        // Rotar la bala hacia la dirección de disparo
        balaActual.transform.forward = direcciónConDispersión.normalized;

        // Agregar fuerzas a la bala
        balaActual.GetComponent<Rigidbody>().AddForce(direcciónConDispersión.normalized * fuerzaDisparo, ForceMode.Impulse);
        balaActual.GetComponent<Rigidbody>().AddForce(camaraFPS.transform.up * fuerzaVertical, ForceMode.Impulse);

        // Instanciar destello de disparo, si tienes uno
        if (destelloDisparo != null)
            Instantiate(destelloDisparo, puntoAtaque.position, Quaternion.identity);

        balasRestantes--;
        balasDisparadas++;

        // Invocar la función resetShot (si no está ya invocada), con tu tiempoEntreDisparos
        if (permitirInvoke)
        {
            Invoke("ResetearDisparo", tiempoEntreDisparos);
            permitirInvoke = false;

            // Añadir retroceso al jugador (debería llamarse solo una vez)
            rbJugador.AddForce(-direcciónConDispersión.normalized * fuerzaRetroceso, ForceMode.Impulse);
        }

        // si hay más de una balaPorPulsación, asegurarse de repetir la función disparar
        if (balasDisparadas < balasPorPulsación && balasRestantes > 0)
            Invoke("Disparar", tiempoEntreTiros);
    }
    private void ResetearDisparo()
    {
        // Permitir disparar e invocar de nuevo
        listoParaDisparar = true;
        permitirInvoke = true;
    }

    private void Recargar()
    {
        recargando = true;
        Invoke("RecargaFinalizada", tiempoRecarga); // Invocar la función RecargaFinalizada con tu tiempoRecarga como retraso
    }
    private void RecargaFinalizada()
    {
        // Llenar el cargador
        balasRestantes = tamañoCargador;
        recargando = false;
    }
}