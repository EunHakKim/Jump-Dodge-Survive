using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public int maxHealth;
    public int curHealth;
    public int score;
    public Transform target;
    public BoxCollider meleeArea;
    public bool isChase;
    public bool isAttack;
    public GameManager manager;

    public AudioSource attackSound;

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;
    NavMeshAgent nav;
    Animator anim;
    int attackMethod;
    bool isVictory;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<SkinnedMeshRenderer>().material;
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        attackMethod = 1;
        isVictory = false;

        Invoke("ChaseStart", 1);
    }
    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    void Update()
    {
        if (nav.enabled)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
        Player player = target.GetComponent<Player>();
        if (player.health == 0 && !isVictory)
        {
            nav.enabled = false;
            anim.SetTrigger("doVictory");
            isVictory= true;
        }
            
    }

    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    void Targeting()
    {
        float targetRadius = 1.5f;
        float targetRange = 3f;

        RaycastHit[] rayHits =
            Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));
        if(rayHits.Length > 0 && !isAttack && manager.isBattle)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        isChase= false;
        isAttack= true;
        attackSound.Play();
        if (attackMethod== 1)
        {
            anim.SetBool("isAttack1", true);
            yield return new WaitForSeconds(0.2f);
            meleeArea.enabled = true;

            yield return new WaitForSeconds(0.35f);
            meleeArea.enabled = false;
            anim.SetBool("isAttack1", false);
            attackMethod = 2;
        } else if(attackMethod== 2)
        {
            anim.SetBool("isAttack2", true);
            yield return new WaitForSeconds(0.4f);
            meleeArea.enabled = true;

            yield return new WaitForSeconds(0.2f);
            meleeArea.enabled = false;
            anim.SetBool("isAttack2", false);
            attackMethod = 1;
        }
        isChase= true;
        isAttack= false;
        
    }

    void FixedUpdate()
    {
        Targeting();
        FreezeVelocity();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);

            StartCoroutine(OnDamage(reactVec));
        }
    }

    IEnumerator OnDamage(Vector3 reactVec)
    {
        mat.color = Color.red;
        Player player = target.GetComponent<Player>();
        player.score += score;
        yield return new WaitForSeconds(0.1f);

        if(curHealth > 0)
        {
            mat.color = Color.white;
        }
        else
        {
            mat.color = Color.gray;
            gameObject.layer = 12;
            isChase= false;
            nav.enabled = false;
            anim.SetTrigger("doDie");
            

            reactVec = reactVec.normalized;
            reactVec += Vector3.up;
            rigid.AddForce(reactVec * 5, ForceMode.Impulse);

            Destroy(gameObject, 4);
        }
    }
}
