using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : EntityController
{
    private void MovePressed(Vector3Int axis)
    {
        var testPosition = CellPosition + axis;
        if (!DungeonGridController.IsCellPositionCollider(testPosition))
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
        }
    }

    public void MoveDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MovePressed(Vector3Int.down);
        }
    }

    public void MoveLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MovePressed(Vector3Int.left);
        }
    }

    public void MoveRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MovePressed(Vector3Int.right);
        }
    }

    private void SnapToCurrentCellPosition()
    {
        transform.position = DungeonGridController.CellToWorldCentered(CellPosition);
    }
}
