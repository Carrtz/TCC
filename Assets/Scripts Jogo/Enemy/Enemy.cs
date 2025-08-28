using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject PointA;
    public GameObject PointB;

    private Rigidbody2D rb;

    private Transform currentTargetPoint;

    public float health;
    public float speed = 2f;
    public float stoppingDistance = 0.5f;
    [SerializeField] private bool Die; 

   
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // anim = GetComponent<Animator>(); 

        currentTargetPoint = PointB.transform;
    }


    void Update()
    {
        if(health <= 0)
        {
            Destroy(gameObject);
        }
      
        Vector2 direction = (currentTargetPoint.position - transform.position).normalized;

       
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y); 

        
        if (direction.x > 0.01f) 
        {
            transform.localScale = new Vector3(1, 1, 1); 
        }
        else if (direction.x < -0.01f) 
        {
            transform.localScale = new Vector3(-1, 1, 1); 
        }

        
        if (Vector2.Distance(transform.position, currentTargetPoint.position) < stoppingDistance)
        {
         
            if (currentTargetPoint == PointB.transform)
            {
                currentTargetPoint = PointA.transform;
            }
           
            else if (currentTargetPoint == PointA.transform)
            {
                currentTargetPoint = PointB.transform;
            }
        }

        // anim.SetBool("isRunning", rb.velocity.x != 0); 
    }

    void OnDrawGizmos()
    {
        if (PointA != null && PointB != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(PointA.transform.position, stoppingDistance);
            Gizmos.DrawWireSphere(PointB.transform.position, stoppingDistance);
            Gizmos.DrawLine(PointA.transform.position, PointB.transform.position);
        }
    }

 
    void OnCollisionEnter2D(Collision2D col)
    {
        
        if (col.gameObject.CompareTag("Player"))
        {
           
            Die = true;
            print("jogador atingido");
        }
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log(damage);
    }
}