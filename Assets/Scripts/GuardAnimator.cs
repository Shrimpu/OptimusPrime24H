using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GuardPatrol))]
public class GuardAnimator : MonoBehaviour
{
    public Animator anim;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        GetComponent<GuardPatrol>().stateChanged += ChangeAnimation;
    }

    void ChangeAnimation(GuardPatrol.MoveState state, float speed)
    {
        if (state == GuardPatrol.MoveState.chasing)
            anim.SetBool("Chasing", true);
        else
            anim.SetBool("Chasing", false);

        anim.SetFloat("MoveSpeed", speed);

        //switch(state)
        //{
        //    case GuardPatrol.MoveState.chasing:
        //        // do big anim
        //        break;
        //    case GuardPatrol.MoveState.walking:
        //        // do other anim
        //        break;
        //}

    }
}
