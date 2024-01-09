using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInputSurface : MonoBehaviour
{
    [SerializeField] 
    private Camera _camera;
    [SerializeField]
    private float _throwHeight = 5f;

    [SerializeField] 
    private MeshFilter _meshFilter;
    [SerializeField] 
    private MeshCollider _meshCollider;
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(PlaneOrigin(), new Vector3(100, 0, 100));
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Gizmos.DrawCube(PlaneOrigin(), new Vector3(100, 0, 100));
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GetMousePosition(), 0.2f);
        Gizmos.DrawLine(_camera.transform.position, GetMousePosition());

        float height = + _camera.scaledPixelHeight;
        float width = _camera.scaledPixelWidth;
        Vector3 cornerLeftDown = CastPointOnPlane(new Vector3(0,0,1));
        Vector3 cornerLeftUp = CastPointOnPlane(new Vector3(0, height, 1));
        Vector3 cornerRightUp = CastPointOnPlane(new Vector3(width, height, 1));
        Vector3 cornerRightDown = CastPointOnPlane(new Vector3(width, 0, 1));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(cornerLeftUp, cornerLeftDown);
        Gizmos.DrawLine(cornerLeftDown, cornerRightDown);
        Gizmos.DrawLine(cornerRightDown, cornerRightUp);
        Gizmos.DrawLine(cornerRightUp, cornerLeftUp);
        Gizmos.DrawSphere(cornerLeftUp, 1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(cornerLeftDown, 1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(cornerRightDown, 1f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(cornerRightUp, 1f);
    }
    
    private void Awake()
    {
        if (!_camera)
        {
            _camera = Camera.main;
        }
        MakeAreaCollider();
    }

    public void MakeAreaCollider()
    {
        float height = _camera.scaledPixelHeight;
        float width = _camera.scaledPixelWidth;
        Vector3 cornerLeftDown = CastPointOnPlane(new Vector3(0,0,1));
        Vector3 cornerLeftUp = CastPointOnPlane(new Vector3(0, height, 1));
        Vector3 cornerRightUp = CastPointOnPlane(new Vector3(width, height, 1));
        Vector3 cornerRightDown = CastPointOnPlane(new Vector3(width, 0, 1));

        Mesh mesh = new Mesh();
        Vector3 delta =Vector3.up * 10f;
        Vector3[] verticies = new[]
        {
            cornerLeftDown - delta,
            cornerLeftDown + delta,
            cornerLeftUp - delta,
            cornerLeftUp + delta,
            cornerRightUp - delta,
            cornerRightUp + delta,
            cornerRightDown - delta,
            cornerRightDown + delta,
        };
        
        Vector3[] normals = new Vector3[verticies.Length];
        for (int i = 0; i < verticies.Length; i++)
        {
            normals[i] = Vector3.forward;
        }

        mesh.vertices = verticies;
        mesh.triangles = new[]
        {
            0, 1, 3,
            3, 2, 0,
            4, 5, 7,
            7, 6, 4,
            7, 0, 6, 
            7, 1, 0,            
            7, 0, 6, 
            7, 1, 0,
            3,5,2,
            4,2,5,
        };
        _meshFilter.sharedMesh = mesh;
        _meshCollider.sharedMesh = mesh;
    }

    public Vector3 GetMousePosition()
    {
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5);
        return CastPointOnPlane(mousePosition);
    }

    public Vector3 GetCenterPosition()
    {
        float height = _camera.scaledPixelHeight;
        float width = _camera.scaledPixelWidth;
        Vector3 centerPosition = new Vector3(width / 2f, height / 2f, 5f);
        return CastPointOnPlane(centerPosition);
    }

    public Vector3 CastPointOnPlane(Vector3 origin)
    {

        Vector3 worldPosition = _camera.ScreenToWorldPoint(origin);
        
        if (Trigonometry.PointIntersectsAPlane(_camera.transform.position, 
                worldPosition,
                PlaneOrigin(),
                Vector3.up, out Vector3 result))
        {
            return result;
        }
        return origin;
    }
    
    Vector3 PlaneOrigin()
    {
        return new Vector3(0, _throwHeight, 0);
    }
}
