using System.Collections.Generic;
using CodeMonkey.FieldOfView;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(GridLayout))]
public class DungeonGridController : MonoBehaviour
{
    public static DungeonGridController Instance;

    public PlayerController Player;
    public List<EnemyController> Enemies = new();

    private GridLayout _gridLayout;
    public GridLayout GridLayout => _gridLayout != null ? _gridLayout : GetComponent<GridLayout>();
    private TilemapCollider2D _tilemapCollider;
    private FieldOfView _fieldOfView;

    private void Awake()
    {
        Instance = this;

        _gridLayout = GetComponent<GridLayout>();
        _tilemapCollider = GetComponentInChildren<TilemapCollider2D>(true);
        _fieldOfView = GetComponentInChildren<FieldOfView>(true);
    }

    private void Update()
    {
        _fieldOfView.Origin = Player.transform.position;
    }

    public bool IsCellPositionCollider(Vector3Int cellPosition)
    {
        return _tilemapCollider.OverlapPoint(CellToWorldCentered(cellPosition));
    }

    public Vector3 CellToWorldCentered(Vector3Int cellPosition)
    {
        var cellCenter = GridLayout.GetLayoutCellCenter();
        return GridLayout.CellToWorld(cellPosition) + new Vector3(cellCenter.x, cellCenter.y);
    }

    public void RegisterEnemyController(EnemyController enemy)
    {
        Enemies.Add(enemy);
        enemy.gameObject.layer = LayerMask.NameToLayer("Behind Mask");
    }
}
