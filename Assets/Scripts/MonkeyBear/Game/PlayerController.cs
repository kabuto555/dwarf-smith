using MonkeyBear.Model;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MonkeyBear.Game
{
    public class PlayerController : EntityController
    {
        public EntityStats Stats = new();
        public override EntityStats CommonStats => Stats;

        [SerializeField] private ActionIndicator ActionIndicator;
        [SerializeField] private WeaponBehaviour RightWeapon;

        protected override void Start()
        {
            base.Start();

            ActionIndicator.gameObject.SetActive(false);
            ActionIndicator.IndicatorColor = Color.yellow;
        }

        private void MovePressed(Vector3Int axis)
        {
            if (Animator.GetBool(AnimParamIsAttacking))
            {
                return;
            }

            var testPosition = CellPosition + axis;
            if (!DungeonGridController.IsCellPositionCollider(testPosition, includeEnemies: true))
            {
                SetCellPosition(CellPosition + axis);
                ActionIndicator.IndicateMovement();
            }
        }

        public void MoveUp(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SetAimDirection(1);
                MovePressed(Vector3Int.up);
            }
        }

        public void MoveDown(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SetAimDirection(3);
                MovePressed(Vector3Int.down);
            }
        }

        public void MoveLeft(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SetAimDirection(2);
                MovePressed(Vector3Int.left);
            }
        }

        public void MoveRight(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SetAimDirection(0);
                MovePressed(Vector3Int.right);
            }
        }

        private void SetAimDirection(int cellDirection)
        {
            if (cellDirection < 0 || cellDirection > 3)
            {
                return;
            }

            Animator.SetInteger(AnimParamAim, cellDirection);
            ActionIndicator.gameObject.SetActive(true);
            ActionIndicator.SetAimDirection(cellDirection);
        }

        public void RightHandAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                AttackingWeapon = RightWeapon;
                RightWeapon.StartAttack(OnRightHandAttackHit);
                Animator.SetTrigger(AnimParamRSwing);
            }
        }

        public void OnRightHandAttackHit(EntityController otherEntity)
        {
            var damagePayload = new DamagePayload()
            {
                Physical = 10
            };
            otherEntity.OnReceivedDamage(damagePayload);
        }
    }
}
