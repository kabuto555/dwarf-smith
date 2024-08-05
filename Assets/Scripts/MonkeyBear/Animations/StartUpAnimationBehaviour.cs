using MonkeyBear.Game;
using UnityEngine;

namespace MonkeyBear.Animations
{
    public class StartUpAnimationBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(EntityController.AnimParamIsAttacking, true);
        }
    }
}
