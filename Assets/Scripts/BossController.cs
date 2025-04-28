using UnityEngine;
using System.Collections;

public class BossAI : MonoBehaviour
{
    enum EstadoPrincipal { Patrullando, Alerta }
    enum SubEstadoAlerta { Ninguno, AtaqueBasico, AtaqueArea, AtaquePerseguir }

    EstadoPrincipal estado = EstadoPrincipal.Patrullando;
    SubEstadoAlerta subestado = SubEstadoAlerta.Ninguno;

    public Transform[] puntosPatrulla;
    private int indicePatrulla = 0;
    public Transform jugador;
    public float rangoDeteccion = 100f;
    public float rangoAtaqueBasico = 30f;
    public float rangoAtaqueArea = 50f;
    public float velocidadPatrulla = 2f;
    public float velocidadPerseguir = 5f;

    private float vidaActual;
    public float vidaMaxima = 100f;

    private bool enPersecucionRapida = false;

    private Animator animator;

    // Variables para cooldown
    public float cooldownAtaqueBasico = 2f;
    public float cooldownAtaqueArea = 3f;
    private float tiempoUltimoAtaqueBasico = -Mathf.Infinity;
    private float tiempoUltimoAtaqueArea = -Mathf.Infinity;

    void Start()
    {
        vidaActual = vidaMaxima;
        animator = GetComponent<Animator>();
        ActivarAnimacionPatrullar();
    }

    void Update()
    {
        switch (estado)
        {
            case EstadoPrincipal.Patrullando:
                Patrullar();
                if (Vector3.Distance(transform.position, jugador.position) < rangoDeteccion)
                {
                    estado = EstadoPrincipal.Alerta;
                    ActivarAnimacionSeguir();
                }
                break;

            case EstadoPrincipal.Alerta:
                SubEstadosAlerta();
                break;
        }
    }

    void Patrullar()
    {
        Transform destino = puntosPatrulla[indicePatrulla];
        MoverHacia(destino.position, velocidadPatrulla);

        if (Vector3.Distance(transform.position, destino.position) < 0.5f)
        {
            indicePatrulla = (indicePatrulla + 1) % puntosPatrulla.Length;
        }
    }

    void SubEstadosAlerta()
    {
        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (enPersecucionRapida)
        {
            MoverHacia(jugador.position, velocidadPerseguir);
            RotarHacia(jugador.position);
            return;
        }

        if (distancia <= rangoAtaqueBasico)
        {
            if (subestado != SubEstadoAlerta.AtaqueBasico)
            {
                subestado = SubEstadoAlerta.AtaqueBasico;
                ActivarAnimacionAtaqueBasico();
            }
            AtaqueBasico();
        }
        else if (distancia <= rangoAtaqueArea)
        {
            if (subestado != SubEstadoAlerta.AtaqueArea)
            {
                subestado = SubEstadoAlerta.AtaqueArea;
                ActivarAnimacionAtaqueArea();
            }
            AtaqueArea();
        }
        else
        {
            // Sigue persiguiendo normal
            MoverHacia(jugador.position, velocidadPatrulla);
            RotarHacia(jugador.position);

            if (subestado != SubEstadoAlerta.Ninguno)
            {
                subestado = SubEstadoAlerta.Ninguno;
                ActivarAnimacionSeguir();
            }
        }
    }

    void AtaqueBasico()
    {
        if (Time.time - tiempoUltimoAtaqueBasico >= cooldownAtaqueBasico)
        {
            Debug.Log("Boss hace Ataque Básico");
            tiempoUltimoAtaqueBasico = Time.time;
            animator.SetTrigger("AtaqueBasico"); // Reafirmar trigger cada ataque
        }
    }

    void AtaqueArea()
    {
        if (Time.time - tiempoUltimoAtaqueArea >= cooldownAtaqueArea)
        {
            Debug.Log("Boss hace Ataque de Área");
            tiempoUltimoAtaqueArea = Time.time;
            animator.SetTrigger("AtaqueArea"); // Reafirmar trigger cada ataque
        }
    }

    void MoverHacia(Vector3 destino, float velocidad)
    {
        transform.position = Vector3.MoveTowards(transform.position, destino, velocidad * Time.deltaTime);
    }

    void RotarHacia(Vector3 objetivo)
    {
        Vector3 direccion = (objetivo - transform.position).normalized;
        direccion.y = 0; // Evitar que rote en eje Y
        if (direccion != Vector3.zero)
        {
            Quaternion rotacionDeseada = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, Time.deltaTime * 5f); // 5f = velocidad de giro
        }
    }

    void PerseguirRapido()
    {
        MoverHacia(jugador.position, velocidadPerseguir);
        RotarHacia(jugador.position);
    }

    public void RecibirDaño(float daño)
    {
        vidaActual -= daño;

        if (vidaActual % 20 <= 0.1f && !enPersecucionRapida)
        {
            StartCoroutine(ActivarPersecucionRapida());
        }
    }

    IEnumerator ActivarPersecucionRapida()
    {
        enPersecucionRapida = true;
        subestado = SubEstadoAlerta.AtaquePerseguir;
        ActivarAnimacionPerseguirRapido();

        Debug.Log("¡Persecución rápida activada!");

        yield return new WaitForSeconds(5f);

        enPersecucionRapida = false;
        subestado = SubEstadoAlerta.Ninguno;
        ActivarAnimacionSeguir();

        Debug.Log("Persecución rápida terminada");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && enPersecucionRapida)
        {
            enPersecucionRapida = false;
            subestado = SubEstadoAlerta.Ninguno;
            ActivarAnimacionSeguir();

            Debug.Log("Boss golpeó al jugador y terminó la persecución rápida.");
        }
    }

    // Funciones de animación
    void ActivarAnimacionPatrullar()
    {
        animator.ResetTrigger("AtaqueBasico");
        animator.ResetTrigger("AtaqueArea");
        animator.ResetTrigger("Run");
        animator.SetTrigger("Walk");
    }

    void ActivarAnimacionSeguir()
    {
        animator.ResetTrigger("AtaqueBasico");
        animator.ResetTrigger("AtaqueArea");
        animator.ResetTrigger("Run");
        animator.SetTrigger("Walk");
    }

    void ActivarAnimacionAtaqueBasico()
    {
        
        animator.ResetTrigger("Walk");
        animator.SetTrigger("AtaqueBasico");
    }

    void ActivarAnimacionAtaqueArea()
    {
        animator.ResetTrigger("Walk");
        animator.ResetTrigger("Walk");
        animator.SetTrigger("AtaqueArea");
    }

    void ActivarAnimacionPerseguirRapido()
    {
        animator.ResetTrigger("AtaqueBasico");
        animator.ResetTrigger("AtaqueArea");
        animator.SetTrigger("Run");
    }
}