using UnityEngine;

public class EntityController : MonoBehaviour
{
    public Vector3Int CellPosition;
    public virtual EntityStats CommonStats { get; set; }

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

    public float DistanceFromCellPosition(Vector3Int otherCell)
    {
        var diffVector = otherCell - CellPosition;
        return new Vector2Int(diffVector.x, diffVector.y).magnitude;
    }

    public bool HasLineOfSightToCell(Vector3Int otherCell)
    {
        var raycastHit2D = Physics2D.Linecast(transform.position, DungeonGridController.CellToWorldCentered(otherCell), DungeonGridController.FieldOfViewLayerMask);
        return raycastHit2D.collider == null;
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
