using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class DiceController : MonoBehaviour
{
    [SerializeField] 
    private Camera _camera;
    [SerializeField]
    private float _throwHeight = 5f;
    [SerializeField] 
    private Rigidbody _rigidbody;
    [SerializeField] 
    private float _throwVelocityMultiplier;
    [SerializeField] 
    private float _minimalVewlocity;
    [SerializeField] 
    private MeshFilter _meshFilter;
    [SerializeField]
    private List<Vector3> _diceFaceNormals = new List<Vector3>();
    [SerializeField] 
    private TMP_Text _textPrefab;
    [SerializeField]
    private float _followingLerpSpeed;
    private Vector3 _currentVelocity;
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(PlaneOrigin(), new Vector3(100,0, 100));
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Gizmos.DrawCube(PlaneOrigin(), new Vector3(100,0, 100));
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GetMousePosition(), 0.2f);
        Gizmos.DrawLine(_camera.transform.position,GetMousePosition());

        Dictionary<Vector3,  List<Vector3>> normalDictionary = new Dictionary<Vector3, List<Vector3>>();
        Mesh mesh = _meshFilter.sharedMesh;
        for (int index = 0; index < mesh.vertices.Length; index++)
        {
            Vector3 vertex =mesh.vertices[index];
            Vector3 normal = mesh.normals[index];
            if (!normalDictionary.ContainsKey(normal))
            {
                normalDictionary.Add(normal, new List<Vector3>());
            }
            normalDictionary[normal].Add(vertex);
        }

        foreach (KeyValuePair<Vector3, List<Vector3>> valuePair in normalDictionary)
        {
            if (valuePair.Value.Count > 4)
            {
                foreach (Vector3 vertex in valuePair.Value)
                {
                    Gizmos.color = new Color(vertex.x, vertex.y, vertex.z, 1f);
                    Gizmos.DrawLine(transform.position, transform.position+(valuePair.Key*2f));
                }
            }

        }
    }

    private void Awake()
    {
        if (!_camera)
        {
            _camera = Camera.main;
        }
    }

    private void OnMouseDown()
    {
        StartCoroutine(DragAndRollCoroutine());
    }

    public void FindDiceFaces()
    {
        GetSidesFromMesh(_meshFilter.mesh);
    }

    private void GetSidesFromMesh(Mesh mesh)
    {
        _diceFaceNormals.Clear();
        Dictionary<Vector3,  List<Vector3>> normalDictionary = new Dictionary<Vector3, List<Vector3>>();
        for (int index = 0; index < _meshFilter.mesh.vertices.Length; index++)
        {
            Vector3 vertex = _meshFilter.mesh.vertices[index];
            Vector3 normal = _meshFilter.mesh.normals[index];
            if (!normalDictionary.ContainsKey(normal))
            {
                normalDictionary.Add(normal, new List<Vector3>());
            }
            normalDictionary[normal].Add(vertex);
        }

        foreach (KeyValuePair<Vector3, List<Vector3>> valuePair in normalDictionary)
        {
            if (valuePair.Value.Count > 4)
            {
                _diceFaceNormals.Add(valuePair.Key);
                
            }
        }
    }
        
    IEnumerator DragAndRollCoroutine()
    {
        _rigidbody.useGravity = false;
        do
        {
            AdjustDicePosition();
            yield return null;
        } while (Input.GetMouseButton(0));

        _rigidbody.useGravity = true;
        Vector3 adjustedVelocity = _currentVelocity / 1000f;
        if (adjustedVelocity.magnitude > _minimalVewlocity)
        {
            _rigidbody.velocity = adjustedVelocity * _throwVelocityMultiplier;
            yield return null;
            while(_rigidbody.velocity.magnitude > 0.2f || transform.position.y < 0.5f)
            {
                yield return null;
            }
            Debug.Log("Random numer rolled " + GetClosestFaceIndex(transform.up));
        }
        else
        {
            _rigidbody.velocity = Vector3.zero;
            Debug.Log("Put down dice");
        }
        

    }

    private void AdjustDicePosition()
    {
        Vector3 targetPosition = GetMousePosition();
        Vector3 currentPosition = transform.position;
        _currentVelocity = (targetPosition - currentPosition) / Time.deltaTime;
        float lerpT = Time.deltaTime * _followingLerpSpeed;
        transform.position =  Vector3.Lerp(currentPosition, targetPosition, lerpT);
    }

    private int GetClosestFaceIndex(Vector3 soughtUp)
    {
        float currentDot = 2;
        int closestFaceIndex = 0;
        
        for (int index = 0; index < _diceFaceNormals.Count; index++)
        {
            Vector3 face = _diceFaceNormals[index];
            float newDot = Mathf.Abs(Vector3.Dot(face, soughtUp));
            if (newDot < currentDot)
            {
                closestFaceIndex = index;
                currentDot = newDot;
            }
        }

        return closestFaceIndex;
    }
    
    private Vector3 GetMousePosition()
    {
        Vector3 mousePosition = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5));
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        
        if (Trigonometry.PointIntersectsAPlane(_camera.transform.position, 
                                                mousePosition,
                                                PlaneOrigin(),
                                                Vector3.up, out Vector3 result))
        {
            return result;
        }

        return mousePosition;
    }

    Vector3 PlaneOrigin()
    {
        return new Vector3(0, _throwHeight, 0);
    }
}
