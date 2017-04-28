using UnityEngine;
using System.Collections;

public class HandheldCtrl : MonoBehaviour
{
    public GameObject photonInit;
    public GameObject mainObject,sub1,sub2,sub3;
    private Transform[] tr;
    private PhotonView pv;
    private PhotonView objectPV;
    public PhotonView gesturePV;
    private ObjectCtrl objectCtrl;
    public PhotonView systemPV;
    public Camera mainCamera;

    private int focusIndex = 0;
    private bool connection = false;
    private bool idleTurn = false;
    private float idleStartTime;
    private Vector2 curPos;
    // Use this for initialization
    void Start()
    {
        mainCamera = Camera.main.GetComponent<Camera>();
        tr = new Transform[4];
        tr[0] = mainObject.GetComponent<Transform>();
        tr[1] = sub1.GetComponent<Transform>();
        tr[2] = sub2.GetComponent<Transform>();
        tr[3] = sub3.GetComponent<Transform>();
        pv = GetComponent<PhotonView>();
        objectPV = mainObject.GetComponent<PhotonView>();
        objectCtrl = mainObject.GetComponent<ObjectCtrl>();
    }
    // Update is called once per frame
    void Update()
    {
        if (connection)
        {
            focusIndex = objectCtrl.focusIndex;
            if (Input.touchCount == 1 && Input.GetTouch(0).position.x < 2000 && Input.GetTouch(0).position.x > 400
                    && objectCtrl.State == ObjectCtrl.ObjectState.idle)
            {
                curPos = Input.GetTouch(0).deltaPosition;
                float dx = curPos.x * 10;
                float dy = curPos.y * 10;
                tr[focusIndex].RotateAround(Vector3.zero, Vector3.up, Time.deltaTime * dx);
                tr[focusIndex].RotateAround(Vector3.zero, Vector3.right, Time.deltaTime * dy);
                objectPV.RPC("syncronizeObject", PhotonTargets.Others, tr[focusIndex].position, tr[focusIndex].rotation);
            }
            else if (Input.touchCount == 2
                    && objectCtrl.State == ObjectCtrl.ObjectState.idle)
            {
                Touch touchzero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchzero.position - touchzero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevTouchDeltaMsg = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMsg = (touchzero.position - touchOne.position).magnitude;

                float deltaMagnitudeiff = prevTouchDeltaMsg - touchDeltaMsg;
                objectPV.RPC("zoom", PhotonTargets.All, deltaMagnitudeiff * 0.3f);
            }
        }
    }

    void setConnection(bool con)
    {
        if (con)
        {
            systemPV.RPC("setLeapActive", PhotonTargets.MasterClient, false);
            objectPV.RPC("initObject", PhotonTargets.MasterClient);
            Invoke("connectionOn", 0.5f);

        }
        else
        {
            systemPV.RPC("setLeapActive", PhotonTargets.MasterClient, true);
            connection = false;
        }
    }

    void connectionOn()
    {
        connection = true;
    }

    void OnGUI()
    {
        GUIStyle fontStyle = new GUIStyle(GUI.skin.button);
        fontStyle.fontSize = 40;
        if (!photonInit.active) // 처음 연결할때
        {
            if (GUI.Button(new Rect(50, 50, 300, 200),"Connect",fontStyle))
            {
                photonInit.active = true;
            }
        }
        else
        {
            if (!connection)
            {
                if (GUI.Button(new Rect(50, 50, 300, 200), "Connect", fontStyle))
                {
                    setConnection(true);
                }
            }
            else
            {
                if (GUI.Button(new Rect(50, 50, 300, 200), "Disconnect", fontStyle) && objectCtrl.State == ObjectCtrl.ObjectState.idle)
                {
                    setConnection(false);
                }
                if (GUI.Button(new Rect(50, 400, 300, 200), "Decompose", fontStyle)&& focusIndex == 0 && objectCtrl.State == ObjectCtrl.ObjectState.idle)
                {
                    objectPV.RPC("setObjectState", PhotonTargets.All,2);
                }
                if (GUI.Button(new Rect(50, 750, 300, 200), "Compose", fontStyle) && focusIndex!=0 && objectCtrl.State == ObjectCtrl.ObjectState.idle)
                {
                    objectPV.RPC("setObjectState", PhotonTargets.All, 3);
                }
                if(objectCtrl.State == ObjectCtrl.ObjectState.idle)
                {
                    if (GUI.Button(new Rect(50, 1100, 300, 200), "GyroOn", fontStyle))
                    {
                        objectPV.RPC("setObjectState", PhotonTargets.All, 6);
                    }

                }
                else
                {
                    if (GUI.Button(new Rect(50, 1100, 300, 200), "GyroOff", fontStyle))
                    {
                        objectPV.RPC("setObjectState", PhotonTargets.All, 0);
                        
                    }
                }
                if (GUI.Button(new Rect(2150, 300, 300, 200), "Right", fontStyle) && focusIndex != 0 && objectCtrl.State == ObjectCtrl.ObjectState.idle)
                {
                    objectPV.RPC("setObjectState", PhotonTargets.All, 4);
                }
                if (GUI.Button(new Rect(2150, 600, 300, 200), "Left", fontStyle) && focusIndex != 0 && objectCtrl.State == ObjectCtrl.ObjectState.idle)
                {
                    objectPV.RPC("setObjectState", PhotonTargets.All, 5);
                }
            }
        }
    }
}