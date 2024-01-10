using System.Collections;
using System.Collections.Generic;
using System.Timers;
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

#region Properites
    [SerializeField] 
    private ResultManager _resultManager;
    [SerializeField]
    private float _velocityChangeSpeed = 12f;    
    [SerializeField]
    private float _manualVelocityIncreaseModifier = 0.9f;
    [SerializeField]
    private MouseInputSurface _mouseInputSurface;
    [SerializeField] 
    private MeshFilter _meshFilter;
    [SerializeField] 
    private List<DiceSide> _diceSides = new List<DiceSide>();
    [SerializeField] 
    private Rigidbody _rigidbody;
    [SerializeField] 
    [Tooltip("Used for automatic sides generation from mesh. Any face with greater or equal area will become a side")]
    private float _minimumFaceSurfaceArea = 1f;
    [SerializeField] 
    private float _throwVelocityMultiplier;
    [FormerlySerializedAs("_minimalVewlocity")] [SerializeField] 
    private float _minimalVelocity;
    [SerializeField] 
    private TMP_Text _textPrefab;
    [SerializeField]
    private float _followingLerpSpeed;
    [SerializeField] 
    private float _numberTextDistance = 0.75f;
    [SerializeField] 
    private float _rollFinishHeight = 2.03f;    
    [SerializeField] 
    private float _rollFinishVelocity = 0.02f;

    [Header("Debug")]
    [SerializeField]
    private TMP_Text _velocityDebug;
    
    private Vector3 _currentVelocity;
    private Vector3 _previousTargetPosition;
#endregion
    
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
        if (!IsInAir())
        {
            StartCoroutine(DragAndRollCoroutine());
        }
    }

#endregion
    
#region PublicFunctions
    public void FindDiceFaces()
    {
        GetSidesFromMesh(_meshFilter.sharedMesh);
    }

    public void RollAutomatically()
    {
        StartCoroutine(RollAutomaticallyCoroutine());
    }
#endregion
    
#region PrivateFunctions
    private void GetSidesFromMesh(Mesh mesh)
    {
        ClearSides();
        Dictionary<Vector3,  List<Vector3>> normalDictionary = new Dictionary<Vector3, List<Vector3>>();
        for (int index = 0; index < mesh.vertices.Length; index++)
        {
            Vector3 vertex = mesh.vertices[index];
            Vector3 normal = mesh.normals[index];
            if (!normalDictionary.ContainsKey(normal))
            {
                normalDictionary.Add(normal, new List<Vector3>());
            }
            normalDictionary[normal].Add(vertex);
        }

        int sideCount = 1;
        foreach (KeyValuePair<Vector3, List<Vector3>> valuePair in normalDictionary)
        {
            if (TryCalculateSurfaceArea(valuePair.Value, out float surface))
            {
                if (surface > _minimumFaceSurfaceArea)
                {
                    CreateSide(valuePair.Key, sideCount);
                    sideCount++;
                }
            }
        }
    }

    private bool TryCalculateSurfaceArea(List<Vector3> vertexes, out float calculatedSurface)
    {
        if (vertexes.Count < 2)
        {
            calculatedSurface = 0;
            return false;
        }

        float area = 0f;
        Vector3 origin = vertexes[0];
        for (int i = 1; i < vertexes.Count - 1; i++)
        {
            Vector3 current = vertexes[i];
            Vector3 next = vertexes[i+1];
            Vector3 cross = Vector3.Cross(origin-current, origin-next);
            area += cross.magnitude /2f;
        }

        calculatedSurface = area;
        return true;
    }

    private void ClearSides()
    {
        foreach (DiceSide side in _diceSides)
        {
            DestroyImmediate(side.Text.gameObject);
        }
        _diceSides.Clear();
    }

    private void CreateSide(Vector3 normal, int result)
    {
        TMP_Text textInstance = Instantiate(_textPrefab, transform);
        textInstance.transform.localPosition = normal * _numberTextDistance;
        textInstance.transform.forward = -normal;
        DiceSide side = new DiceSide(textInstance, normal, result);
        _diceSides.Add(side);
    }
    
    private void AdjustDicePosition(Vector3 targetPosition)
    {
        Vector3 currentPosition = transform.position;
        float lerpT = Time.deltaTime * _followingLerpSpeed;
        transform.position =  Vector3.Lerp(currentPosition, targetPosition, lerpT);
    }

    private void AdjustVelocity(Vector3 targetPosition)
    {
        Vector3 newVelocity = (targetPosition - _previousTargetPosition)/100f * _manualVelocityIncreaseModifier / Time.deltaTime;
        _currentVelocity = Vector3.Lerp(_currentVelocity, newVelocity, _velocityChangeSpeed * Time.deltaTime);
        _velocityDebug.text = $"Velocity: {_currentVelocity} - {_currentVelocity.magnitude} / {_minimalVelocity}";
        _previousTargetPosition = targetPosition;
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

    private void StartDragging()
    {
        _rigidbody.useGravity = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _currentVelocity = Vector3.zero;
    }

    private void StartRolling()
    {
        _rigidbody.useGravity = true;
        _resultManager.StartRoll();
    }

    private bool IsInAir()
    {
        return _rigidbody.velocity.magnitude > _rollFinishVelocity|| transform.position.y > _rollFinishHeight;
    }

    private void SubmitResult()
    {
        int result = GetClosestResult();
        _resultManager.AddRollResult(result);
        Debug.Log("Random numer rolled " + result);
    }

    private Vector3 GetRandomVelocity()
    {
        float x = Random.Range(-1f, 1f);
        float z = Random.Range(-1f, 1f);
        return new Vector3(x,0,z).normalized * (Random.Range(3, 5) * _throwVelocityMultiplier);
    }
#endregion

#region Corotuines
    IEnumerator DragAndRollCoroutine()
    {
        StartDragging();
        do
        {
            Vector3 mousePosition = _mouseInputSurface.GetMousePosition();
            AdjustDicePosition(mousePosition);
            AdjustVelocity(mousePosition);
            yield return null;
        } while (Input.GetMouseButton(0));
        
        if (_currentVelocity.magnitude > _minimalVelocity)
        {
            StartRolling();
            _rigidbody.velocity =_currentVelocity * _throwVelocityMultiplier;
            yield return null;
            while(IsInAir())
            {
                yield return null;
            }

            SubmitResult();
        }
        else
        {
            _resultManager.DiscardRoll();
            Vector3 landingPosition = new Vector3(transform.position.x, _rollFinishHeight, transform.position.z);
            while (transform.position.y > _rollFinishHeight + 0.02f)
            {
                float t = _followingLerpSpeed * Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, landingPosition, t);
                yield return null;
            }
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.useGravity = true;
            Debug.Log("Put down dice");
        }
    }
    
    IEnumerator RollAutomaticallyCoroutine()
    {
        StartDragging();
        Vector3 centerPosition = _mouseInputSurface.GetCenterPosition();
        do
        {
            AdjustDicePosition(centerPosition);
            yield return null;
        } while (Vector3.Distance(centerPosition, transform.position) > 0.02f);

        StartRolling();

        _rigidbody.velocity = GetRandomVelocity();
        yield return null;
        while(IsInAir())
        {
            yield return null;
        }
        SubmitResult();
    }
#endregion

}
