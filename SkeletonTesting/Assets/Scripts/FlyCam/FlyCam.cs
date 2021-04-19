using UnityEngine;
using UnityEngine.Internal;


public class FlyCam : MonoBehaviour
{
    private float _minDistance = 0;
    private float _distance = 10;
    /// <summary>The target which camera should look at </summary>
    public Transform target;
    /// <summary>Minimum distance between target and camera</summary>
    public float minDistance
    {
        get { return _minDistance; }
        set
        {
            _minDistance = value;
            if(_minDistance < 0)
                _minDistance = 0;
        }
    }
    /// <summary>Distance between camera and target</summary>
    public float distance
    {
        get { return _distance; }
        set
        {
            _distance = value;
            if(_distance< minDistance)
                _distance = minDistance;
        }
    }
    /// <summary>Camera rotation in global coordinate frame</summary>
    public Vector3 rotation = Vector3.zero;
    ///<summary>Translation in global coordinate frame</summary>
    public Vector3 translation = Vector3.zero;

    void Start()
    {
        LookAtTargetFrom(Vector3.forward, Vector3.up, 5);
    }
    void Update()
    {
        transform.rotation = Quaternion.Euler(rotation);
        transform.position = target.position - distance * transform.forward + translation;
    }

    public void ClearTranslation()
    {
        translation = Vector3.zero;
    }

    /// <summary>Look at target from a point</summary>
    /// <param name="targetLocalDirection">target direction from camera position in target frame</param>
    /// <param name="targetLocalCameraUp">camera up direction in target frame</param>
    /// <param name="distance">distance between camera and target</param>
    public void LookAtTargetFromLocal(Vector3 targetLocalDirection, Vector3 targetLocalCameraUp, float distance)
    {
        Vector3 direction = target.TransformDirection(targetLocalDirection);
        Vector3 cameraUp = target.TransformDirection(targetLocalCameraUp);
        LookAtTargetFrom(direction, cameraUp, distance);
    }

    /// <summary>Look at target from a point</summary>
    /// <param name="targetLocalDirection">target direction from camera position in global frame</param>
    /// <param name="targetLocalCameraUp">camera up direction in global frame</param>
    /// <param name="distance">distance between camera and target</param>
    public void LookAtTargetFrom(Vector3 direction, Vector3 cameraUp, float distance)
    {
        direction = direction.normalized;
        cameraUp = cameraUp.normalized;
        Vector3 cameraForward = -direction;
        ClearTranslation();
        rotation = (Quaternion.LookRotation(cameraForward, cameraUp)).eulerAngles;
        this.distance = distance;
    }


}
