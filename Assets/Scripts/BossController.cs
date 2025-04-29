using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossAI_NavMesh : MonoBehaviour
{
    enum EstadoPrincipal { Patrullando, Alerta }
    enum SubEstadoAlerta { Ninguno, AtaqueBasico, AtaqueArea, AtaquePerseguir }

    EstadoPrincipal estado = EstadoPrincipal.Patrullando;
    SubEstadoAlerta subestado = SubEstadoAlerta.Ninguno;

    public Transform[] puntosPatrulla;
    private int indicePatrulla = 0;
    public Transform jugador;
    public float rangoDeteccion = 10f;
    public float rangoAtaqueBasico = 2f;
    public float rangoAtaqueArea = 6f;
    public float velocidadPatrulla = 2f;
    public float velocidadPerseguir = 5f;

    private float vidaActual;
    public float vidaMaxima = 100f;

    private bool enPersecucionRapida = false;

    private Animator animator;
    private NavMeshAgent agent;

    // Cooldowns
    public float cooldownAtaqueBasico = 2f;
    public float cooldownAtaqueArea = 3f;
    private float tiempoUltimoAtaqueBasico = 0;
    private float tiempoUltimoAtaqueArea = 0;

    void Start()
    {
        vidaActual = vidaMaxima;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        jugador = GameObject.FindGameObjectWithTag("Player").transform;

        ActivarAnimacionPatrullar();
        agent.speed = velocidadPatrulla;
        agent.SetDestination(puntosPatrulla[indicePatrulla].position);
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
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            indicePatrulla = (indicePatrulla + 1) % puntosPatrulla.Length;
            agent.SetDestination(puntosPatrulla[indicePatrulla].position);
        }
    }

    void SubEstadosAlerta()
    {
        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (enPersecucionRapida)
        {
            agent.speed = velocidadPerseguir;
            agent.SetDestination(jugador.position);
           // RotarHacia(jugador.position);
            return;
        }

        if (distancia <= rangoAtaqueBasico && subestado != SubEstadoAlerta.AtaqueBasico)//&& Time.time - tiempoUltimoAtaqueBasico >= cooldownAtaqueBasico
        {
           
                subestado = SubEstadoAlerta.AtaqueBasico;
               // ActivarAnimacionAtaqueBasico();
            
            //DetenerMovimiento();
            AtaqueBasico();
        }
        else if (distancia <= rangoAtaqueArea && subestado != SubEstadoAlerta.AtaqueArea) //&& Time.time - tiempoUltimoAtaqueArea >= cooldownAtaqueArea
        {
            
                subestado = SubEstadoAlerta.AtaqueArea;
                //ActivarAnimacionAtaqueArea();
            
            //DetenerMovimiento();
            AtaqueArea();
        }
        else
        {
            agent.speed = velocidadPatrulla;
            agent.SetDestination(jugador.position);
            //RotarHacia(jugador.position);

            if (subestado != SubEstadoAlerta.Ninguno)
            {
                subestado = SubEstadoAlerta.Ninguno;
                ActivarAnimacionSeguir();
            }
        }
    }

    void DetenerMovimiento()
    {
        agent.ResetPath();
    }

    void AtaqueBasico()
    {
        if (Time.time - tiempoUltimoAtaqueBasico >= cooldownAtaqueBasico)
        {
            Debug.Log("Boss hace Ataque Básico");
            tiempoUltimoAtaqueBasico = Time.time;
            animator.SetTrigger("AtaqueBasico");
        }
    }

    void AtaqueArea()
    {
        if (Time.time - tiempoUltimoAtaqueArea >= cooldownAtaqueArea)
        {
            Debug.Log("Boss hace Ataque de Área");
            tiempoUltimoAtaqueArea = Time.time;
            animator.SetTrigger("AtaqueArea");
        }
    }

    void RotarHacia(Vector3 objetivo)
    {
        Vector3 direccion = (objetivo - transform.position).normalized;
        direccion.y = 0;
        if (direccion != Vector3.zero)
        {
            Quaternion rotacionDeseada = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, Time.deltaTime * 5f);
        }
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
        agent.speed = velocidadPerseguir;

        yield return new WaitForSeconds(5f);

        enPersecucionRapida = false;
        subestado = SubEstadoAlerta.Ninguno;
        ActivarAnimacionSeguir();
        agent.speed = velocidadPatrulla;

        Debug.Log("Persecución rápida terminada");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && enPersecucionRapida)
        {
            enPersecucionRapida = false;
            subestado = SubEstadoAlerta.Ninguno;
            ActivarAnimacionSeguir();
            agent.speed = velocidadPatrulla;

            Debug.Log("Boss golpeó al jugador y terminó la persecución rápida.");
        }
    }

    // Animaciones
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
        animator.SetTrigger("AtaqueArea");
    }

    void ActivarAnimacionPerseguirRapido()
    {
        animator.ResetTrigger("AtaqueBasico");
        animator.ResetTrigger("AtaqueArea");
        animator.SetTrigger("Run");
    }
}