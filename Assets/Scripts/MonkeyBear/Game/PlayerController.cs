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
                CellPosition += axis;
                SnapToCurrentCellPosition();
            }
        }

        public void MoveUp(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MovePressed(Vector3Int.up);
                SetAimDirection(1);
            }
        }

        public void MoveDown(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MovePressed(Vector3Int.down);
                SetAimDirection(3);
            }
        }

        public void MoveLeft(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MovePressed(Vector3Int.left);
                SetAimDirection(2);
            }
        }

        public void MoveRight(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MovePressed(Vector3Int.right);
                SetAimDirection(0);
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
                Animator.SetTrigger(AnimParamRSwing);
            }
        }

        private void SnapToCurrentCellPosition()
        {
            transform.position = DungeonGridController.CellToWorldCentered(CellPosition);
        }
    }
}
