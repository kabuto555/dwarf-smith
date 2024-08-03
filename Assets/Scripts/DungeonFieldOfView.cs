using System.Collections.Generic;
using CodeMonkey.FieldOfView;
using UnityEngine;

public class DungeonFieldOfView : FieldOfView
{
    [SerializeField] private DungeonGridController DungeonGridController;

    private bool _hasDungeonGridController;

    private HashSet<Vector3Int> _tileCellPositionsInFieldOfView;

    protected override void Awake()
    {
        base.Awake();

        _tileCellPositionsInFieldOfView = new HashSet<Vector3Int>();
    }

    protected override void LateUpdate()
    {
        _hasDungeonGridController = DungeonGridController != null;

        _tileCellPositionsInFieldOfView.Clear();

        base.LateUpdate();
    }

    protected override void OnCollisionHit(RaycastHit2D raycastHit2D)
    {
        if (!_hasDungeonGridController)
        {
            return;
        }

        var localRayVector = raycastHit2D.point - new Vector2(Origin.x, Origin.y);
        var localRayVectorWithSmallStep = localRayVector.normalized * (localRayVector.magnitude + 0.01f);
        var pointWithSmallStep = Origin + new Vector3(localRayVectorWithSmallStep.x, localRayVectorWithSmallStep.y);
        var cellPosition = DungeonGridController.GridLayout.WorldToCell(pointWithSmallStep);

        if (!DungeonGridController.IsCellPositionCollider(cellPosition))
        {
            return;
        }

        _tileCellPositionsInFieldOfView.Add(cellPosition);
    }

    protected override (Vector3[], int[]) PostProcessMesh(Vector3[] rayVertices, int[] rayTriangles)
    {
        if (!_hasDungeonGridController)
        {
            return (rayVertices, rayTriangles);
        }

        Vector3[] tileVertices = new Vector3[_tileCellPositionsInFieldOfView.Count * 4];
        int[] tileTriangles = new int[_tileCellPositionsInFieldOfView.Count * 6];
        int tileVerticesIndex = 0;
        int trianglesIndex = 0;
        var tileSize = DungeonGridController.GridLayout.cellSize;
        int currentMeshVerticesCount = rayVertices.Length;

        foreach (var cellPosition in _tileCellPositionsInFieldOfView)
        {
            var cellWorldPosition = DungeonGridController.GridLayout.CellToWorld(cellPosition);
            tileVertices[tileVerticesIndex] =     new Vector3(cellWorldPosition.x,                  cellWorldPosition.y);
            tileVertices[tileVerticesIndex + 1] = new Vector3(cellWorldPosition.x + tileSize.x,   cellWorldPosition.y);
            tileVertices[tileVerticesIndex + 2] = new Vector3(cellWorldPosition.x,                cellWorldPosition.y + tileSize.y);
            tileVertices[tileVerticesIndex + 3] = new Vector3(cellWorldPosition.x + tileSize.x, cellWorldPosition.y + tileSize.y);

            tileTriangles[trianglesIndex] = currentMeshVerticesCount + tileVerticesIndex;
            tileTriangles[trianglesIndex + 1] = currentMeshVerticesCount + tileVerticesIndex + 1;
            tileTriangles[trianglesIndex + 2] = currentMeshVerticesCount + tileVerticesIndex + 2;

            tileTriangles[trianglesIndex + 3] = currentMeshVerticesCount + tileVerticesIndex + 1;
            tileTriangles[trianglesIndex + 4] = currentMeshVerticesCount + tileVerticesIndex + 2;
            tileTriangles[trianglesIndex + 5] = currentMeshVerticesCount + tileVerticesIndex + 3;

            tileVerticesIndex += 4;
            trianglesIndex += 6;
        }

        var allVertices = new Vector3[rayVertices.Length + tileVertices.Length];
        rayVertices.CopyTo(allVertices, 0);
        tileVertices.CopyTo(allVertices, rayVertices.Length);

        var allTriangles = new int[rayTriangles.Length + tileTriangles.Length];
        rayTriangles.CopyTo(allTriangles, 0);
        tileTriangles.CopyTo(allTriangles, rayTriangles.Length);

        return (allVertices, allTriangles);
    }
}
