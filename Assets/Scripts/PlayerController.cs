using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Vector3Int CellPosition;

    private DungeonGridController _dungeonGridController;
    private GridLayout GridLayout => _dungeonGridController.GridLayout;

    private void Awake()
    {
        _dungeonGridController = FindObjectOfType<DungeonGridController>();
    }

    void Start()
    {
        SetCellPosition(GridLayout.WorldToCell(transform.position));
    }


    public void SetCellPosition(Vector3Int newCellPosition)
    {
        CellPosition = new Vector3Int(newCellPosition.x, newCellPosition.y, CellPosition.z);
        SnapToCurrentCellPosition();
    }

    private void MovePressed(Vector3Int axis)
    {
        var testPosition = CellPosition + axis;
        if (!_dungeonGridController.IsCellPositionCollider(testPosition))
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
        transform.position = _dungeonGridController.CellToWorldCentered(CellPosition);
    }
}
