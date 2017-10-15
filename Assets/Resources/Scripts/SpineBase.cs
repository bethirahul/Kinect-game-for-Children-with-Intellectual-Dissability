using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class SpineBase : MonoBehaviour
{
    public GameObject BodySourceManagerGO;
    public Windows.Kinect.JointType spineBase;
    public Windows.Kinect.JointType leftFoot;
    public Windows.Kinect.JointType rightFoot;
    private BodySourceManager bodyManager;
    private Windows.Kinect.Body[] bodies = null;

    public Vector3 currentPosition;
    public Vector3 leftFootPos;
    public Vector3 rightFootPos;
    public bool isTracked;

    // Use this for initialization
    void Start ()
    {
        if (BodySourceManagerGO == null)
        {
            Debug.Log("Game object not initialized");
            return;

        }
        bodyManager = BodySourceManagerGO.GetComponent<BodySourceManager>();
        isTracked = false;
    }

    // Update is called once per frame
    void Update ()
    {
        if (bodyManager == null)
            return;

        bodies = bodyManager.GetData();
        if (bodies == null)
            return;

        foreach (var body in bodies)
        {
            if (body.IsTracked)
            {
                //Debug.Log("body tracking id" + body.TrackingId + "Joint confidance" + body.Joints[trackingJoint].TrackingState);
                var pos  = body.Joints[spineBase].Position;
                currentPosition = new Vector3(pos.X * 10f, pos.Y * 10f, pos.Z * 10f);
                pos = body.Joints[leftFoot ].Position;
                leftFootPos     = new Vector3(pos.X * 10f, pos.Y * 10f, pos.Z * 10f);
                pos = body.Joints[rightFoot].Position;
                rightFootPos    = new Vector3(pos.X * 10f, pos.Y * 10f, pos.Z * 10f);
                if(currentPosition.z > 11.0f)
                {
                    isTracked = true;
                    /*gameObject.transform.position = currentPosition;*/
                    break;
                }
                else
                    isTracked = false;
            }
            else
            {
                isTracked = false;
                //Debug.Log("body is not being tracked");
            }
        }
    }
}
