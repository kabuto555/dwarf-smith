using MonkeyBear.Game;
using UnityEngine;

namespace MonkeyBear.Animations
{
    public class ActiveAttackFramesAnimationBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(EntityController.AnimParamIsFrameAttackValid, true);
            animator.SendMessage("OnActiveAttackFramesStarted");
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(EntityController.AnimParamIsFrameAttackValid, false);
            animator.SendMessage("OnActiveAttackFramesEnded");
        }
    }
}
