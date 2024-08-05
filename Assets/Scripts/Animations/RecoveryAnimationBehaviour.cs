using UnityEngine;

namespace Animations
{
    public class RecoveryAnimationBehaviour : StateMachineBehaviour
    {
        public string TriggerName;
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.ResetTrigger(TriggerName);
            animator.SetBool(EntityController.AnimParamIsAttacking, false);
        }
    }
}
