/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

// This is a modified version from Code Monkey's Field Of View tutorial

using System;
using UnityEngine;
using UnityEngine.Serialization;

public class FieldOfView : MonoBehaviour
{
    [SerializeField] private GameObject DarkFieldObject;
    [SerializeField] private LayerMask LayerMask;
    public float FieldOfViewAngle = 90f;
    public float ViewDistance = 50f;
    public int RayCount = 50;
    public Vector3 Origin = Vector3.zero;

    private Mesh _mesh;
    private float _startingAngle;

    private void Awake() {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
        // DarkFieldObject.SetActive(true);
    }

    private void OnEnable()
    {
        DarkFieldObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (DarkFieldObject != null)
        {
            DarkFieldObject.SetActive(false);
        }
    }

    private void LateUpdate() {
        int rayCount = RayCount;
        float angle = _startingAngle;
        float angleIncrease = FieldOfViewAngle / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Origin;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++) {
            Vector3 vertex;
            RaycastHit2D raycastHit2D = Physics2D.Raycast(Origin, GetVectorFromAngle(angle), ViewDistance, LayerMask);
            if (raycastHit2D.collider == null) {
                // No hit
                vertex = Origin + GetVectorFromAngle(angle) * ViewDistance;
            } else {
                // Hit object
                vertex = raycastHit2D.point;
            }
            vertices[vertexIndex] = vertex;

            if (i > 0) {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }


        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
        _mesh.bounds = new Bounds(Origin, Vector3.one * 1000f);
    }

    public void SetOrigin(Vector3 origin) {
        // this.origin = origin;
    }

    public void SetAimDirection(Vector3 aimDirection) {
        _startingAngle = GetAngleFromVectorFloat(aimDirection) + FieldOfViewAngle / 2f;
    }

    private float GetAngleFromVectorFloat(Vector3 dir) {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;

        return n;
    }

    private Vector3 GetVectorFromAngle(float angle) {
        // angle = 0 -> 360
        float angleRad = angle * (Mathf.PI/180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }
}
