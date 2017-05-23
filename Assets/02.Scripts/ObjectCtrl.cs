using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectCtrl : MonoBehaviour
{
    public GameObject sub1, sub2, sub3;
    public GameObject systemInit;
    public GameObject mobileCtrl;
    private Transform[] tr;
    private PhotonView pv;

    private Vector3 originalSub1 = new Vector3(-0.04426686f, -0.3077977f, 0.06664629f);
    private Vector3 originalSub2 = new Vector3(-0.04426686f, -0.3077977f, 0.06664629f);

    private Vector3 centerSub1 = new Vector3(-0.04426686f, -0.2077977f, 0.06664629f);
    private Vector3 centerSub2 = new Vector3(-0.05f, -0.2577977f, 0.06664629f);

    public int focusIndex = 0;
    private float speed = 5.0f;
    private bool rotateIdentity = false;
    private bool idleturn = false;
    public enum ObjectState { idle, rotate, decompose, compose, right, left, gyro };
    public ObjectState State;
    private float theta = 0.0f;
    private AndroidJavaClass jc;
    private AndroidJavaObject _activity;
    private bool gyro = false;
    // Use this for initialization
    void Awake()
    {
        tr = new Transform[4];
        tr[0] = GetComponent<Transform>();
        tr[1] = sub1.GetComponent<Transform>();
        tr[2] = sub2.GetComponent<Transform>();
        tr[3] = sub3.GetComponent<Transform>();
        pv = GetComponent<PhotonView>();
        State = ObjectState.idle;
        if (SystemInfo.deviceType.ToString().Equals("Handheld"))
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            _activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }

    void Update()
    {
        if (State == ObjectState.idle)
        {
            if (gyro)
            {
                setGyro(false);
            }
            if (idleturn)
            {
                idleTurn();
            }
        }
        else
        {
            if (State == ObjectState.decompose)
            {
                decomposeObejct();
            }
            else if (State == ObjectState.compose)
            {
                composeObject();
            }
            else if (State == ObjectState.right)
            {
                rightObject();
            }
            else if (State == ObjectState.left)
            {
                leftObject();
            }
            else if (State == ObjectState.gyro)
            {
                setGyro(true);
            }
        }
    }

    void setGyro(bool on)
    {
        if (on)
        {
            gyro = true;
            if(SystemInfo.deviceType.ToString().Equals("Handheld"))
                _activity.Call("setSendGyro", true);
        }
        else
        {
            gyro = false;
            if (SystemInfo.deviceType.ToString().Equals("Handheld"))
                _activity.Call("setSendGyro", false);
        }
    }

    void getGyro(string orival)
    {
        float x = Input.acceleration.y * 180.0f;
        float z = -Input.acceleration.x * 180.0f;
        string temp = x +" " +z;
        pv.RPC("gyroRotate", PhotonTargets.All, temp);
    }

    [PunRPC]
    void gyroRotate(string values)
    {
        string[] value = values.Split(' ');
        Debug.Log(value[0] + " " + value[1]);
        //tr[focusIndex].localRotation = Quaternion.Slerp(tr[focusIndex].rotation, Quaternion.Euler(float.Parse(value[0]), 0, float.Parse(value[1])), Time.deltaTime * 20.0f);
        tr[focusIndex].rotation = Quaternion.Slerp(tr[focusIndex].rotation, Quaternion.Euler(float.Parse(value[0]), 0, float.Parse(value[1])), Time.deltaTime * 20.0f);
    }

    [PunRPC]
    void setIdleTurn(bool idleturn)
    {
        if(PhotonNetwork.isMasterClient)
            pv.RPC("syncronizeObject", PhotonTargets.Others, tr[focusIndex].position, tr[focusIndex].rotation);
        this.idleturn = idleturn;
    }

    void idleTurn()
    {
        tr[focusIndex].localRotation = Quaternion.Slerp(tr[focusIndex].localRotation,
        Quaternion.Euler(tr[focusIndex].localRotation.x, tr[focusIndex].localRotation.y + theta, tr[focusIndex].localRotation.z), Time.deltaTime);
        theta = (theta + 0.5f) % 360.0f;
    }

    void decomposeObejct()
    {
        if (tr[0].localRotation.x <= 0.001f && tr[0].localRotation.x >= -0.001f && !rotateIdentity)
        {
            tr[0].localRotation = Quaternion.identity;
            rotateIdentity = true;
        }
        else
        {
            tr[0].localRotation = Quaternion.Slerp(tr[0].localRotation, Quaternion.identity, Time.deltaTime * speed);
        }
        if (rotateIdentity)
        {
            tr[1].position = Vector3.Lerp(tr[1].position, new Vector3(-1.5f, tr[1].position.y, tr[1].position.z), Time.deltaTime * speed);
            tr[2].position = Vector3.Lerp(tr[2].position, new Vector3(1.5f, tr[2].position.y, tr[2].position.z), Time.deltaTime * speed);
            if (tr[1].position.x <= -1.4f && tr[2].position.x >= 1.4f)
            {
                tr[1].position = new Vector3(-1.5f, tr[1].position.y, tr[1].position.z);
                tr[2].position = new Vector3(1.5f, tr[2].position.y, tr[2].position.z);
                focusIndex = 3;
                if (PhotonNetwork.isMasterClient)
                {
                    pv.RPC("syncronizeObject", PhotonTargets.Others, tr[focusIndex].position, tr[focusIndex].rotation);
                }
                State = ObjectState.idle;
                rotateIdentity = false;
            }
        }
    }

    void composeObject()
    {
        tr[1].localPosition = Vector3.Lerp(tr[1].localPosition, originalSub1, Time.deltaTime * speed);
        tr[2].localPosition = Vector3.Lerp(tr[2].localPosition, originalSub2, Time.deltaTime * speed);
        tr[3].localPosition = Vector3.Lerp(tr[3].localPosition, Vector3.zero, Time.deltaTime * speed);
        tr[1].localRotation = Quaternion.Slerp(tr[1].localRotation, Quaternion.identity, Time.deltaTime * speed);
        tr[2].localRotation = Quaternion.Slerp(tr[2].localRotation, Quaternion.identity, Time.deltaTime * speed);
        tr[3].localRotation = Quaternion.Slerp(tr[3].localRotation, Quaternion.identity, Time.deltaTime * speed);
        if (tr[1].localPosition.x >= -0.05f && tr[1].localPosition.x <= -0.03f
                && tr[2].localPosition.x >= -0.05f && tr[2].localPosition.x <= -0.03f
                && tr[3].localPosition.x <= 0.1 && tr[3].localPosition.x >= -0.1)
        {
            tr[0].localRotation = Quaternion.Slerp(tr[0].localRotation, Quaternion.identity, Time.deltaTime * speed);
            if (tr[0].localRotation.x <= 0.1f && tr[0].localRotation.x >= -0.1f)
            {
                tr[1].localPosition = originalSub1;
                tr[2].localPosition = originalSub2;
                tr[3].localPosition = Vector3.zero;
                tr[0].localRotation = Quaternion.identity;
                tr[1].localRotation = Quaternion.identity;
                tr[2].localRotation = Quaternion.identity;
                tr[3].localRotation = Quaternion.identity;
                focusIndex = 0;
                if (PhotonNetwork.isMasterClient)
                {
                    pv.RPC("syncronizeObject", PhotonTargets.Others, tr[focusIndex].position, tr[focusIndex].rotation);
                }
                State = ObjectState.idle;
            }
        }
    }

    void rightObject()
    {
        switch (focusIndex)
        {
            case 1:
                tr[1].position = Vector3.Lerp(tr[1].position, new Vector3(1.5f, centerSub1.y, centerSub1.z), Time.deltaTime * speed);
                tr[2].position = Vector3.Lerp(tr[2].position, centerSub2, Time.deltaTime * speed);
                tr[1].localRotation = Quaternion.Slerp(tr[1].localRotation, Quaternion.identity, Time.deltaTime * speed);
                if (tr[1].position.x >= 1.49f)
                {
                    tr[1].position = new Vector3(1.5f, centerSub1.y, centerSub1.z);
                    tr[1].localRotation = Quaternion.identity;
                    tr[2].position = centerSub2;
                    tr[3].position = new Vector3(-1.5f, 0, 0);
                    focusIndex = 2;
                    if (PhotonNetwork.isMasterClient)
                    {
                        pv.RPC("syncronizeObject", PhotonTargets.Others, tr[focusIndex].position, tr[focusIndex].rotation);
                    }
                    State = ObjectState.idle;
                }
                break;
            case 2:
                tr[2].position = Vector3.Lerp(tr[2].position, new Vector3(1.5f, centerSub2.y, centerSub2.z), Time.deltaTime * speed);
                tr[3].position = Vector3.Lerp(tr[3].position, Vector3.zero, Time.deltaTime * speed);
                tr[2].localRotation = Quaternion.Slerp(tr[2].localRotation, Quaternion.identity, Time.deltaTime * speed);
                if (tr[2].position.x >= 1.49f)
                {
                    tr[2].position = new Vector3(1.5f, centerSub2.y, centerSub2.z);
                    tr[2].localRotation = Quaternion.identity;
                    tr[3].position = Vector3.zero;
                    tr[1].position = new Vector3(-1.5f, centerSub1.y, centerSub1.y);
                    focusIndex = 3;
                    if (PhotonNetwork.isMasterClient)
                    {
                        pv.RPC("syncronizeObject", PhotonTargets.Others, tr[focusIndex].position, tr[focusIndex].rotation);
                    }
                    State = ObjectState.idle;
                }
                break;
            case 3:
                tr[3].position = Vector3.Lerp(tr[3].position, new Vector3(1.5f, 0, 0), Time.deltaTime * speed);
                tr[1].position = Vector3.Lerp(tr[1].position, centerSub1, Time.deltaTime * speed);
                tr[3].localRotation = Quaternion.Slerp(tr[3].localRotation, Quaternion.identity, Time.deltaTime * speed);
                if (tr[3].position.x >= 1.49f)
                {
                    tr[3].position = new Vector3(1.5f, 0, 0);
                    tr[3].localRotation = Quaternion.identity;
                    tr[1].position = centerSub1;
                    tr[2].position = new Vector3(-1.5f, centerSub2.y, centerSub2.z);
                    focusIndex = 1;
                    if (PhotonNetwork.isMasterClient)
                    {
                        pv.RPC("syncronizeObject", PhotonTargets.Others, tr[focusIndex].position, tr[focusIndex].rotation);
                    }
                    State = ObjectState.idle;
                }
                break;
        }
    }

    void leftObject()
    {
        switch (focusIndex)
        {
            case 1:
                tr[1].position = Vector3.Lerp(tr[1].position, new Vector3(-1.5f, centerSub1.y, centerSub1.z), Time.deltaTime * speed);
                tr[3].position = Vector3.Lerp(tr[3].position, Vector3.zero, Time.deltaTime * speed);
                tr[1].localRotation = Quaternion.Slerp(tr[1].localRotation, Quaternion.identity, Time.deltaTime * speed);
                if (tr[1].position.x <= -1.49f)
                {
                    tr[1].position = new Vector3(-1.5f, centerSub1.y, centerSub1.z);
                    tr[3].position = Vector3.zero;
                    tr[2].position = new Vector3(1.5f, centerSub2.y, centerSub2.z);
                    focusIndex = 3;
                    if (PhotonNetwork.isMasterClient)
                    {
                        pv.RPC("syncronizeObject", PhotonTargets.Others, tr[focusIndex].position, tr[focusIndex].rotation);
                    }
                    State = ObjectState.idle;
                }
                break;
            case 2:
                tr[2].position = Vector3.Lerp(tr[2].position, new Vector3(-1.5f, centerSub2.y, centerSub2.z), Time.deltaTime * speed);
                tr[1].position = Vector3.Lerp(tr[1].position, centerSub1, Time.deltaTime * speed);
                tr[2].localRotation = Quaternion.Slerp(tr[2].localRotation, Quaternion.identity, Time.deltaTime * speed);
                if (tr[2].position.x <= -1.49f)
                {
                    tr[2].position = new Vector3(-1.5f, centerSub2.y, centerSub2.z);
                    tr[1].position = centerSub1;
                    tr[3].position = new Vector3(1.5f, 0, 0);
                    focusIndex = 1;
                    if (PhotonNetwork.isMasterClient)
                    {
                        pv.RPC("syncronizeObject", PhotonTargets.Others, tr[focusIndex].position, tr[focusIndex].rotation);
                    }
                    State = ObjectState.idle;
                }
                break;
            case 3:
                tr[3].position = Vector3.Lerp(tr[3].position, new Vector3(-1.5f, 0, 0), Time.deltaTime * speed);
                tr[2].position = Vector3.Lerp(tr[2].position, centerSub2, Time.deltaTime * speed);
                tr[3].localRotation = Quaternion.Slerp(tr[3].localRotation, Quaternion.identity, Time.deltaTime * speed);
                if (tr[3].position.x <= -1.49f)
                {
                    tr[3].position = new Vector3(-1.5f, 0, 0);
                    tr[2].position = centerSub2;
                    tr[1].position = new Vector3(1.5f, centerSub1.y, centerSub1.z);
                    focusIndex = 2;
                    if (PhotonNetwork.isMasterClient)
                    {
                        pv.RPC("syncronizeObject", PhotonTargets.Others, tr[focusIndex].position, tr[focusIndex].rotation);
                    }
                    State = ObjectState.idle;
                }
                break;
        }
    }

    [PunRPC]
    void syncronizeObject(Vector3 curPos, Quaternion curRot)
    {
        tr[focusIndex].position = curPos;
        tr[focusIndex].rotation = curRot;
    }

    [PunRPC]
    void syncronizeAllObject(int index,Vector3 curPos0, Quaternion curRot0, Vector3 curPos1, Quaternion curRot1, 
        Vector3 curPos2, Quaternion curRot2,Vector3 curPos3, Quaternion curRot3)
    {
        focusIndex = index;
        tr[0].position = curPos0;
        tr[0].localRotation = curRot0;
        tr[1].position = curPos1;
        tr[1].localRotation = curRot1;
        tr[2].position = curPos2;
        tr[2].localRotation = curRot2;
        tr[3].position = curPos3;
        tr[3].localRotation = curRot3;
        Debug.Log(focusIndex);
    }

    [PunRPC]
    void initObject()
    {
        pv.RPC("syncronizeAllObject", PhotonTargets.Others, focusIndex, tr[0].position, tr[0].localRotation, 
            tr[1].position, tr[1].localRotation, tr[2].position, tr[2].localRotation, tr[3].position, tr[3].localRotation);
        pv.RPC("zoom", PhotonTargets.Others, Camera.main.GetComponent<Camera>().fieldOfView - 60);
    }

    [PunRPC]
    void zoom(float delta)
    {
        if (Camera.main.GetComponent<Camera>().fieldOfView + delta < 56.0f)
            Camera.main.GetComponent<Camera>().fieldOfView = 56.0f;
        else if (Camera.main.GetComponent<Camera>().fieldOfView + delta > 100.0f)
            Camera.main.GetComponent<Camera>().fieldOfView = 100.0f;
        else
            Camera.main.GetComponent<Camera>().fieldOfView += delta;
    }

    [PunRPC]
    void setFocus(int num)
    {
        switch (num)
        {
            case 0:
                {
                    focusIndex = 0;
                    break;
                }
            case 1:
                {
                    focusIndex = 1;
                    break;
                }
            case 2:
                {
                    focusIndex = 2;
                    break;
                }
            case 3:
                {
                    focusIndex = 3;
                    break;
                }
        }
    }

    [PunRPC]
    void setObjectState(int num)
    {
        switch (num)
        {
            case 0:
                State = ObjectState.idle;
                break;
            case 1:
                State = ObjectState.rotate;
                break;
            case 2:
                State = ObjectState.decompose;
                break;
            case 3:
                State = ObjectState.compose;
                break;
            case 4:
                State = ObjectState.right;
                break;
            case 5:
                State = ObjectState.left;
                break;
            case 6:
                State = ObjectState.gyro;
                break;
        }
    }

}
