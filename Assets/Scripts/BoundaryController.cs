using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryController : MonoBehaviour
{
    [SerializeField] 
    private Camera _camera;
    [SerializeField] 
    private MeshFilter _meshFilter;
    [SerializeField] 
    private MeshCollider _meshCollider;

    [SerializeField] 
    [Range(0, 100)]
    private float _widthPercentage;    
    [SerializeField] 
    [Range(0, 100)]
    private float _heightPercentage;
    
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
        float halfHeightProportion = 0.5f + ((_heightPercentage / 100f) / 2f);
        float halfWidthProportion = 0.5f + ((_widthPercentage / 100f) /2f);
        float maxHeight = _camera.scaledPixelHeight * (halfHeightProportion);
        float maxWidth = _camera.scaledPixelWidth* (halfWidthProportion);        
        float minHeight = _camera.scaledPixelHeight * (1 - (halfHeightProportion));
        float minWidth = _camera.scaledPixelWidth* (1 - (halfWidthProportion));
        Vector3 cornerLeftDown = CameraVector(new Vector3(minWidth,minHeight,1));
        Vector3 cornerLeftUp = CameraVector(new Vector3(minWidth, maxHeight, 1));
        Vector3 cornerRightUp = CameraVector(new Vector3(maxWidth, maxHeight, 1));
        Vector3 cornerRightDown = CameraVector(new Vector3(maxWidth, minHeight, 1));

        Mesh mesh = new Mesh();
        Vector3 cameraPosition = _camera.transform.position;
        Vector3[] verticies = new[]
        { 
            cameraPosition,
            cameraPosition + cornerLeftDown,
            cameraPosition + cornerLeftUp,
            cameraPosition + cornerRightUp,
            cameraPosition + cornerRightDown,
        };
        
        Vector3[] normals = new Vector3[verticies.Length];
        for (int i = 0; i < verticies.Length; i++)
        {
            normals[i] = Vector3.forward;
        }

        mesh.vertices = verticies;
        mesh.triangles = new[]
        {
            1, 0, 2,
            2, 0, 3,
            3, 0, 4,
            4,0, 1,
        };
        _meshFilter.sharedMesh = mesh;
        _meshCollider.sharedMesh = mesh;
    }

    private Vector3 CameraVector(Vector3 screenPosition)
    {
        Vector3 worldPosition = _camera.ScreenToWorldPoint(screenPosition);
        return (worldPosition - _camera.transform.position).normalized * 40f;
    }
}
