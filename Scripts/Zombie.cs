using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Manager;

public class Zombie : MonoBehaviourPunCallbacks
{
    private Animator animator;
    private NavMeshAgent agent;
    private Collider ZombieCollider;

    private bool IsAttacking, IsDead, Head;

    public float Health = 100f, Damage = 20f;

    private void Awake()
    {
        if (gameObject.CompareTag("ZombieHead")) Head = true;
    }

    private void Start()
    {
        if (Head)
        {
            ZombieCollider = GetComponent<Collider>();
            return;
        }

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        ZombieCollider = GetComponent<Collider>();
        
        Health = ZombieSpawner.instance.ZHealth;
        animator.SetFloat("speed", ZombieSpawner.instance.ZAnimationSpeed);
        agent.speed = ZombieSpawner.instance.ZAgentSpeed;
        Damage = ZombieSpawner.instance.ZDamage;
    }

    private void Update()
    {        
        if (Head) return;

        if (Health <= 0)
        {
            if (IsDead) return;
            Die();
            IsDead = true;
            return;
        }

        if (ManagerBase.roomState == RoomState.GameOver) return;

        FollowPlayer();
        CheckAttack();
    }

    // used in the weapon base script To "detecthit" the damage of the zombie 
    [PunRPC]
    void SyncZombieHealth(float amount) =>
                Health -= amount;

    void Die() 
    {
        photonView.RPC("DieRPC", RpcTarget.All);
    }

    [PunRPC]
    void DieRPC()
    {
        agent.speed = 0f;
        agent.enabled = false;
        ZombieCollider.enabled = false;
        animator.SetBool("Attack", false);
        animator.SetTrigger("Die");
    
        ZombieSpawner.instance.ZombieSpawned--;

        Destroy(gameObject, 5f);
    }

    void FollowPlayer() 
    {
        if (IsAttacking) return;

        agent.enabled = true;
        agent.destination = NearestPlayer().transform.position;
    }

    void CheckAttack()
    {
        float DistanceFromPlayer = Vector3.Distance(NearestPlayer().transform.position, transform.position); 
        Vector3 DirectionToPlayer = NearestPlayer().transform.position - transform.position;
        float angle = Vector3.Angle(DirectionToPlayer, transform.forward);
        
        if (DistanceFromPlayer <= 1.8f && angle <= 65f)
        {
            Attack();
        }
        else 
        {
            animator.SetBool("Attack", false);
            IsAttacking = false;
        }
    }

    void Attack() 
    {
        if (IsAttacking) return;

        IsAttacking = true;
        agent.enabled = false;
        animator.SetBool("Attack", true);
        NearestPlayer().GetDamage(Damage);
        StartCoroutine(ResetHitting());
    }

    Player NearestPlayer() 
    {
        Player Nearest = null;
        float minDist = Mathf.Infinity;
        foreach (Player players in GameManager.instance.AlivePlayersList) 
        {
            float dist = Vector3.Distance(players.transform.position, transform.position);
            if (dist < minDist) 
            {
                Nearest = players;
                minDist = dist;
            }
        }
        return Nearest;
    }

    IEnumerator ResetHitting() 
    {
        yield return new WaitForSeconds(1f);
        IsAttacking = false;
    }
}
