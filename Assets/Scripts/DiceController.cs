using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class DiceController : MonoBehaviour
{
    [System.Serializable]
    class DiceSide : ISerializationCallbackReceiver
    {
        public TMP_Text Text;
        public Vector3 Normal;
        public int Result;
        
        public DiceSide(TMP_Text text, Vector3 normal, int result)
        {
            Text = text;
            Normal = normal;
            Result = result;
            text.text = result.ToString();
        }

        public void OnBeforeSerialize()
        {
            Text.text = Result.ToString();
        }

        public void OnAfterDeserialize()
        {
            
        }
    }

    [SerializeField] 
    private ResultManager _resultManager;
    [SerializeField]
    private MouseInputSurface _mouseInputSurface;
    [SerializeField] 
    private Rigidbody _rigidbody;
    [SerializeField] 
    private float _throwVelocityMultiplier;
    [SerializeField] 
    private float _minimalVewlocity;
    [SerializeField] 
    private MeshFilter _meshFilter;
    [SerializeField] 
    private List<DiceSide> _diceSides = new List<DiceSide>();
    [SerializeField] 
    private TMP_Text _textPrefab;
    [SerializeField]
    private float _followingLerpSpeed;
    [SerializeField] 
    private float _numberTextDistance = 0.75f;
    private Vector3 _currentVelocity;

#region UnityFunctions
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.position, transform.position + (_currentVelocity/10f));
        foreach (DiceSide side in _diceSides)
        {
            Vector3 normal = transform.TransformDirection(side.Normal);
            Gizmos.color = new Color(normal.x, normal.y, normal.z, 1f);
            Vector3 delta = normal * Mathf.Abs(2*Vector3.Dot(normal, Vector3.up));
            Gizmos.DrawLine(transform.position, transform.position+ delta);
        }
    }
    
    private void OnMouseDown()
    {
        StartCoroutine(DragAndRollCoroutine());
    }

#endregion
    public void FindDiceFaces()
    {
        GetSidesFromMesh(_meshFilter.mesh);
    }

    public void RollAutomatically()
    {
        StartCoroutine(RollAutomaticallyCoroutine());
    }

#region PrivateFunctions
    private void GetSidesFromMesh(Mesh mesh)
    {
        ClearSides();
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
                CreateSide(valuePair.Key);
            }
        }
    }

    private void ClearSides()
    {
        foreach (DiceSide side in _diceSides)
        {
            DestroyImmediate(side.Text.gameObject);
        }
        _diceSides.Clear();
    }

    private void CreateSide(Vector3 normal)
    {
        TMP_Text textInstance = Instantiate(_textPrefab, transform);
        textInstance.transform.localPosition = normal * _numberTextDistance;
        textInstance.transform.forward = -normal;
        DiceSide side = new DiceSide(textInstance, normal, 0);
        _diceSides.Add(side);
    }
    
    private void AdjustDicePosition(Vector3 targetPosition)
    {
        Vector3 currentPosition = transform.position;
        _currentVelocity = (targetPosition - currentPosition) / Time.deltaTime;
        float lerpT = Time.deltaTime * _followingLerpSpeed;
        transform.position =  Vector3.Lerp(currentPosition, targetPosition, lerpT);
    }

    private int GetClosestResult()
    {
        float currentAngle = 200;
        int closestFaceIndex = 0;
        
        for (int index = 0; index < _diceSides.Count; index++)
        {
            Vector3 face = transform.TransformDirection(_diceSides[index].Normal);
            float newAngle = Mathf.Abs(Vector3.Angle(face, Vector3.up));
            if (newAngle < currentAngle)
            {
                closestFaceIndex = index;
                currentAngle = newAngle;
            }
        }

        return _diceSides[closestFaceIndex].Result;
    }
#endregion

    IEnumerator DragAndRollCoroutine()
    {
        _rigidbody.useGravity = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        do
        {
            AdjustDicePosition(_mouseInputSurface.GetMousePosition());
            yield return null;
        } while (Input.GetMouseButton(0));

        _rigidbody.useGravity = true;
        Vector3 adjustedVelocity = _currentVelocity / 1000f;
        if (adjustedVelocity.magnitude > _minimalVewlocity)
        {
            _rigidbody.velocity = adjustedVelocity * _throwVelocityMultiplier;
            yield return null;
            while(_rigidbody.velocity.magnitude > 0.02f || transform.position.y > 2.03f)
            {
                yield return null;
            }

            int result = GetClosestResult();
            _resultManager.AddDiceResult(GetClosestResult());
            Debug.Log("Random number rolled " + result);
        }
        else
        {
            _rigidbody.velocity = Vector3.zero;
            Debug.Log("Put down dice");
        }
    }
    
    IEnumerator RollAutomaticallyCoroutine()
    {
        _rigidbody.useGravity = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        Vector3 centerPosition = _mouseInputSurface.GetCenterPosition();
        do
        {
            AdjustDicePosition(centerPosition);
            yield return null;
        } while (Vector3.Distance(centerPosition, transform.position) > 0.02f);

        _rigidbody.useGravity = true;
        float x = Random.Range(-1f, 1f);
        float z = Random.Range(-1f, 1f);
        Vector3 adjustedVelocity = new Vector3(x,0,z).normalized * (Random.Range(0.25f, 1.25f) * _throwVelocityMultiplier);
        _rigidbody.velocity = adjustedVelocity * _throwVelocityMultiplier;
        yield return null;
        while(_rigidbody.velocity.magnitude > 0.02f || transform.position.y > 2.03f)
        {
            yield return null;
        }

        int result = GetClosestResult();
        _resultManager.AddDiceResult(GetClosestResult());
        Debug.Log("Random numer rolled " + result);
    }
}
