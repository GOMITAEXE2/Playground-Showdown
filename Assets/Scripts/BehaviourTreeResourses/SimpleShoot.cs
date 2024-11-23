using UnityEngine;

public class SimpleShoot : MonoBehaviour
{
    public GameObject projectilePrefab; // Prefab del proyectil
    public Transform shootPoint; // Punto de origen de los disparos
    public float shootInterval = 1f; // Intervalo entre disparos
    public float projectileSpeed = 10f; // Velocidad de los proyectiles

    private bool isShooting = false;

    public void StartShooting()
    {
        if (!isShooting)
        {
            isShooting = true;
            InvokeRepeating(nameof(Shoot), 0f, shootInterval);
        }
    }

    public void StopShooting()
    {
        if (isShooting)
        {
            isShooting = false;
            CancelInvoke(nameof(Shoot));
        }
    }

    void Shoot()
    {
        if (projectilePrefab != null && shootPoint != null)
        {
            // Crear un proyectil y asignar dirección y velocidad
            GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = shootPoint.forward * projectileSpeed;
            }

            // Destruir el proyectil después de un tiempo
            Destroy(projectile, 5f);
        }
    }
}
