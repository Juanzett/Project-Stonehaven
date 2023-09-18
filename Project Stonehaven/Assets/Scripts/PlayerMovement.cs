using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] Rigidbody2D rb;
    Vector2 moveDirection;
    Animator anim;
    public Vector2 lastMotionVector;
    public bool moving;
    //[SerializeField] SpriteRenderer spriteProp;
    bool facingRight = true;
    float horizontal;
    float vertical;
   
   void Awake() 
   {
       anim = GetComponent<Animator>();
   }

    void Update()
    {
        Inputs();
    }

    void FixedUpdate() 
    {
        Move();
        LastMotion();
    }

    void LastMotion()
    {
        moving = horizontal != 0 || vertical != 0;
        anim.SetBool("Moving", moving);

       if (horizontal != 0 || vertical != 0) 
       {
            lastMotionVector = new Vector2(horizontal, vertical).normalized;

            anim.SetFloat("LastHorizontal", horizontal);
            anim.SetFloat("LastVertical", vertical);
       }
    }

    void Inputs()
    {
        //movimiento
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");        

        moveDirection = new Vector2(horizontal,vertical).normalized;
        anim.SetFloat("Horizontal", horizontal);
        anim.SetFloat("Vertical", vertical);

        // rotar sprite, lo hice de esta manera y no poniendole un flipX al sprite, porque de esta manera rotamos tambien los colliders, en caso de necesitarse, si no llega a ser necesario
        // solo hay que agregar un float nuevo al input horizontal y verificar si es positivo o negativo y ahi rotar el sprite con flipX
       if( horizontal > 0 && facingRight)
       {
         Flip();
       }
       else if( horizontal < 0 && !facingRight)
       {
         Flip();
       }
        
    }

    void Move()
    {
        rb.velocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
    }

    void Flip()
    {
       facingRight =!facingRight;
      // spriteProp.flipX = facingRight;
       transform.Rotate(0,180f,0f);
    }
    
}
