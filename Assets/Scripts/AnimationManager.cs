using System.Collections;
using UnityEngine;

public class AnimationManager : MonoBehaviour { 

    [SerializeField] private GameObject raccoon;
    private Animator animator;
    private float raccoonSpeed = 20;
    private float raccoonTarget = 15;
    private static float raccoonStart = 15;
    private static float raccoonEnd = 40;
    private IEnumerator raccoonCR;

    private bool goRight = false;

    public void MoveRaccoon() {
        Animator animator = raccoon.GetComponent<Animator>();
        goRight = !goRight;
        if (goRight) {
            animator.SetTrigger("goRight");
        } else {
            animator.ResetTrigger("goRight");
            //animator.StartPlayback();
        }

        //RaccoonAsPlayer();
    }


    public void RaccoonAsPlayer() {

        raccoonTarget = raccoonTarget == raccoonStart ? raccoonEnd : raccoonStart;
        if (raccoonCR != null) {
            StopCoroutine(raccoonCR);
        }

        raccoonCR = ToggleRaccoon();
        StartCoroutine(raccoonCR);
    }

    private IEnumerator ToggleRaccoon() {

        yield return new WaitForSeconds(0.5f);
        animator.SetTrigger("isWalking");
        raccoon.transform.localScale = new Vector2(raccoon.transform.localScale.x * -1, 1);
        while (Mathf.Abs(raccoon.transform.position.x - raccoonTarget) > 0.001f) {
            raccoon.transform.position = Vector2.MoveTowards(raccoon.transform.position, new Vector2(raccoonTarget, raccoon.transform.position.y), raccoonSpeed * Time.deltaTime);
            yield return null;
        }
        animator.ResetTrigger("isWalking");
    }


}
