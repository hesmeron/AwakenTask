using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInputSurface : MonoBehaviour
{
    [SerializeField] 
    [Range(0, 100)]
    private float _widthPercentage;    
    [SerializeField] 
    [Range(0, 100)]
    private float _heightPercentage;
    [SerializeField] 
    private Camera _camera;
    [SerializeField]
    private float _throwHeight = 5f;


    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(PlaneOrigin(), new Vector3(100, 0, 100));
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Gizmos.DrawCube(PlaneOrigin(), new Vector3(100, 0, 100));
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GetMousePosition(), 0.2f);
        Gizmos.DrawLine(_camera.transform.position, GetMousePosition());
    }
    
    private void Awake()
    {
        if (!_camera)
        {
            _camera = Camera.main;
        }
    }

    public Vector3 GetMousePosition()
    {
        float halfHeightProportion = 0.5f + ((_heightPercentage / 100f) / 2f);
        float halfWidthProportion = 0.5f + ((_widthPercentage / 100f) /2f);
        float maxHeight = _camera.scaledPixelHeight * (halfHeightProportion);
        float maxWidth = _camera.scaledPixelWidth* (halfWidthProportion);        
        float minHeight = _camera.scaledPixelHeight * (1 - (halfHeightProportion));
        float minWidth = _camera.scaledPixelWidth* (1 - (halfWidthProportion));
        float x = Mathf.Clamp(Input.mousePosition.x, minWidth, maxWidth);
        float y = Mathf.Clamp(Input.mousePosition.y, minHeight, maxHeight);
        Vector3 mousePosition = new Vector3(x, y, 5);
        return CastPointOnPlane(mousePosition);
    }

    public Vector3 GetCenterPosition()
    {
        float height = _camera.scaledPixelHeight;
        float width = _camera.scaledPixelWidth;
        Vector3 centerPosition = new Vector3(width / 2f, height / 2f, 5f);
        return CastPointOnPlane(centerPosition);
    }

    private Vector3 CastPointOnPlane(Vector3 origin)
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
