// Attach this to the Release state in the Animator
using UnityEngine;

public class ArcherFireExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        animator.GetComponent<ArcherBrain>().OnFireAnimationComplete();
    }
}