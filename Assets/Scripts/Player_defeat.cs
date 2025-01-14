using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public Weapon weapon;
    public Camera followCamera;
    public GameManager manager;

    public AudioSource coinSound;
    public AudioSource healthSound;
    public AudioSource damageSound;
    public AudioSource shotSound;
    public AudioSource dodgeSound;
    public AudioSource reloadSound;

    public int coin;
    public int health;
    public int score;

    public int maxCoin;
    public int maxHealth;

    float hAxis;
    float vAxis;

    bool jDown;
    bool fDown;
    bool rDown;

    bool isDodge;
    bool isReload;
    bool isFireReady;
    bool isBorder;
    bool isDamage;
    bool isDead;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid;
    Animator anim;
    MeshRenderer[] meshs;

    GameObject nearObject;

    float fireDelay;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();

        //PlayerPrefs.SetInt("MaxScore", 500);
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Move();
        Turn();
        Attack();
        Reload();
        Dodge();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1");
        rDown = Input.GetButtonDown("Reload");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if(isDodge)
            moveVec = dodgeVec;

        if (isReload || isDead || !manager.isBattle)
            moveVec = Vector3.zero;

        if (!isBorder)
            transform.position += moveVec * speed * 1f * Time.deltaTime;

        anim.SetBool("isWalk", moveVec != Vector3.zero);
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec);

        if (fDown && !isDead && manager.isBattle)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Attack()
    {
        fireDelay += Time.deltaTime;
        isFireReady = weapon.rate < fireDelay;

        if(fDown && isFireReady && !isDodge && weapon.curAmmo!=0 && !isDead && manager.isBattle) {
            shotSound.Play();
            weapon.Use();
            anim.SetTrigger("doShot");
            fireDelay = 0;
        }
    }

    void Reload()
    {
        if(rDown && !isDodge && isFireReady && !isDead && !isReload && (weapon.curAmmo != weapon.maxAmmo) && manager.isBattle)
        {
            reloadSound.Play();
            anim.SetTrigger("doReload");
            isReload= true;

            Invoke("ReloadOut", 1.5f);
        }
    }

    void ReloadOut()
    {
        weapon.curAmmo = weapon.maxAmmo;
        isReload= false;
    }

    void Dodge()
    {
        if(jDown && moveVec != Vector3.zero && !isDodge &&!isDead && manager.isBattle) {
            dodgeSound.Play();
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 2, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 2, LayerMask.GetMask("Wall"));
    }

    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor") {
            
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch(item.type)
            {
                case Item.Type.Coin:
                    coin += item.value;
                    coinSound.Play();
                    if(coin > maxCoin)
                        coin= maxCoin;
                    Destroy(other.gameObject);
                    break;
                case Item.Type.Heart:
                    if(health != maxHealth)
                    {
                        health += item.value;
                        healthSound.Play();
                        if (health > maxHealth)
                            health = maxHealth;
                        Destroy(other.gameObject);
                    }
                    break;
            }
        }
        else if(other.tag == "EnemyBullet")
        {
            if (!isDamage && (health>0))
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;
                StartCoroutine(OnDamage());
            }
        }
    }

    IEnumerator OnDamage()
    {
        damageSound.Play();
        isDamage = true;
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.yellow;
        }

        if (health <= 0 && !isDead)
            OnDie();

        yield return new WaitForSeconds(1f);

        isDamage= false;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }
    }

    void OnDie()
    {
        anim.SetTrigger("doDie");
        isDead = true;
        Invoke("GameOver", 3f);
    }

    void GameOver()
    {
        manager.GameOver();
    }
}
