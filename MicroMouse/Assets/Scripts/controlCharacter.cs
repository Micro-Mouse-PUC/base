using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[RequireComponent(typeof(Rigidbody2D))]
public class controlCharacter: MonoBehaviour
{

    Rigidbody2D rigidbody2d;
    [SerializeField] float speed = 2f;
    Vector2 motionVector;
    public Vector2 lastMotionVector;
    public bool moving;

    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
}

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        motionVector = new Vector2(horizontal, vertical);
        animator.SetFloat("horizontal", motionVector.x);
        animator.SetFloat("vertical", motionVector.y);
        moving = horizontal != 0 || vertical != 0;
        animator.SetBool("moving", moving);
        if(moving) {
            lastMotionVector = new Vector2(
                horizontal,
                vertical
            ).normalized;
            animator.SetFloat("lastHorizontal", horizontal);
            animator.SetFloat("lastVertical", vertical);
        }

        //Flip Sprite
        if(horizontal > 0){
            gameObject.transform.localScale = new Vector3(2,2,1);
        } else if (horizontal < 0){
            gameObject.transform.localScale = new Vector3(-2,2,1);
        }
    }

    void FixedUpdate() {
        Move();
    }

    private void Move() {
        rigidbody2d.velocity = motionVector * speed;
    }
}
