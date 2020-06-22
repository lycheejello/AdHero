using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MessageAnimationManager : MonoBehaviour
{
    private Animator animator;
    [SerializeField]
    private GameObject moneybags;
    [SerializeField]
    private GameObject raccoon;
    //TODO: more robust
    public enum State {
        Wave = 0,
        Earn = 1,
        Sword = 2,
        Sad = 3,
        Fall = 4
    }

    public void SetAnimation(State state) {
        if (!animator) {
            animator = GetComponent<Animator>();
        }

        animator.SetInteger("index", (int)state);
        moneybags.SetActive(state == State.Earn);
        raccoon.SetActive(state != State.Earn);
    }
}
