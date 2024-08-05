using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : EntityController
{
    public EntityStats Stats = new();
    public override EntityStats CommonStats => Stats;

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
            Animator.SetInteger(AnimParamAim, 1);
        }
    }

    public void MoveDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MovePressed(Vector3Int.down);
            Animator.SetInteger(AnimParamAim, 3);
        }
    }

    public void MoveLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MovePressed(Vector3Int.left);
            Animator.SetInteger(AnimParamAim, 2);
        }
    }

    public void MoveRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MovePressed(Vector3Int.right);
            Animator.SetInteger(AnimParamAim, 0);
        }
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
