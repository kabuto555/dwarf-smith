using UnityEngine;

public class EntityController : MonoBehaviour
{
    public Vector3Int CellPosition;
    protected DungeonGridController DungeonGridController => DungeonGridController.Instance != null ? DungeonGridController.Instance : FindObjectOfType<DungeonGridController>();
    protected GridLayout GridLayout => DungeonGridController.GridLayout;
    public bool DebugSnapToGrid;

    protected virtual void Start()
    {
        SetCellPosition(GridLayout.WorldToCell(transform.position));
    }

    public void SetCellPosition(Vector3Int newCellPosition)
    {
        CellPosition = new Vector3Int(newCellPosition.x, newCellPosition.y, CellPosition.z);
        SnapToCurrentCellPosition();
    }

    private void SnapToCurrentCellPosition()
    {
        transform.position = DungeonGridController.CellToWorldCentered(CellPosition);
    }

    private void OnValidate()
    {
        if (DebugSnapToGrid)
        {
            DebugSnapToGrid = false;
            SnapToCurrentCellPosition();
        }
    }
}
