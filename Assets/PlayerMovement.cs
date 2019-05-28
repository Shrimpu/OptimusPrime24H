using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody rbd;
    [HideInInspector]
    public Animator anim;

    [Header("Stats")]
    public float moveSpeed;

    // Start is called before the first frame update
    void Start()
    {
        rbd = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        anim.SetFloat("MoveSpeed", rbd.velocity.magnitude);
    }

    private void Move()
    {
        float verticalInput = Input.GetAxis("Vertical") /** (moveSpeed * Time.deltaTime)*/;
        float horizontalInput = Input.GetAxis("Horizontal") /** (moveSpeed * Time.deltaTime)*/;
        float yAxis = rbd.velocity.y;

        Vector3 moveVector = new Vector3(horizontalInput, yAxis, verticalInput);
        moveVector.Normalize();

        rbd.velocity = moveVector * (moveSpeed * Time.deltaTime);

        if (moveVector.magnitude > 0f)
        {
            transform.rotation = Quaternion.LookRotation(moveVector);
        }
    }
}
