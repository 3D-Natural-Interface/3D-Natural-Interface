using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Leap;
using System.Collections.Generic;

public class GestureCtrl : MonoBehaviour
{
    public Controller controller;
    public GameObject movie;
    private GrabCtrl grabCtrl;
    private ObjectCtrl objectCtrl;
    private SceneChangeCtrl sceneChangeCtrl;
    private RawImage rawimage;

    public GameObject mainObject, sub1, sub2, sub3;
    public Transform point;
    public Transform palm;

    private Frame frame;
    private Finger leftFinger, rightFinger;
    private Hand startHand, leftHand, rightHand, currentHand = null;
    private List<Finger> startFingers, fingers;
    private List<Hand> hands;
    private float crtMove, prevMove;

    public bool startZoom = false;
    private bool startRotate = false;
    public bool startGrab = false;
    public bool startGather = false;
    public bool stayGrab = false, scatter = false, startScatter = false;
    public bool idleTurn = false, stayIdle = false;
    public float twoHandDif = float.MaxValue, startTime = float.MaxValue;
    public float endTime = float.MinValue;

    private Vector3 originalSub1 = new Vector3(-0.04426686f, -0.3077977f, 0.06664629f);
    private Vector3 originalSub2 = new Vector3(-0.04426686f, -0.3077977f, 0.06664629f);
    private Vector3 originalSub3 = new Vector3(0, 0, 0);

    private Vector3 centerSub1 = new Vector3(-0.04426686f, -0.2077977f, 0.06664629f);
    private Vector3 centerSub2 = new Vector3(-0.05f, -0.2577977f, 0.06664629f);
    private Vector3 centerSub3 = new Vector3(0, 0, 0);

    private float dist = 2.0f;
    private float height = 0.5f;
    private float theta = 0.0f;

    private PhotonView objectPV;
    public PhotonView mobilePV;
    private int focusIndex = 0;
    private Transform[] tr;
    // Use this for initialization
    void Awake()
    {
        controller = new Controller();
        objectPV = mainObject.GetComponent<PhotonView>();
        mainObject.GetComponent<ObjectCtrl>().enabled = true;
        objectCtrl = mainObject.GetComponent<ObjectCtrl>();
        sceneChangeCtrl = mainObject.GetComponent<SceneChangeCtrl>();
        tr = new Transform[4];
        tr[0] = mainObject.GetComponent<Transform>();
        tr[1] = sub1.GetComponent<Transform>();
        tr[2] = sub2.GetComponent<Transform>();
        tr[3] = sub3.GetComponent<Transform>();
    }
    void OnDisable()
    {
        Debug.Log("disable");

        if (idleTurn)
        {
            objectPV.RPC("setIdleTurn", PhotonTargets.All, false);
            idleTurn = false;
            stayIdle = false;
        }
    }

    void setHand()
    {
        if (frame.Hands[0].IsLeft)
        {
            leftHand = frame.Hands[0];
            rightHand = frame.Hands[1];
        }
        else
        {
            leftHand = frame.Hands[1];
            rightHand = frame.Hands[0];
        }
    }
    void Update()
    {
        frame = controller.Frame(); // controller is a Controller object
        if (frame.Hands.Count > 0)
        {
            hands = frame.Hands;
            currentHand = hands[0];
            fingers = currentHand.Fingers;
            if (idleTurn || stayIdle)
            {
                idleTurn = false;
                stayIdle = false;
                endTime = frame.Timestamp;
                objectPV.RPC("setIdleTurn", PhotonTargets.All, false);
            }
            palm = GameObject.FindGameObjectWithTag("Hand").GetComponent<Transform>();
            if (currentHand.Confidence > 0.98) // 98퍼센트이상의 정확도로 현재손을 인식하였을 때
            {
                if (!movie.active)
                {
                    focusIndex = objectCtrl.focusIndex;
                    if ((palm.position - Vector3.zero).magnitude < 0.26f && !stayGrab && !startGrab)
                    {
                        point.position = Vector3.Slerp(point.position, palm.position - (palm.up * dist) + (Vector3.up * (currentHand.PalmNormal.y < 0 ? height : -height)), Time.deltaTime * 20.0f);
                        point.LookAt(palm.position);
                        tr[focusIndex].transform.LookAt(point.position);
                        objectPV.RPC("syncronizeObject", PhotonTargets.Others, tr[focusIndex].position, tr[focusIndex].rotation);
                    }
                    else
                    {
                        if (!scatter && !startZoom && frame.Timestamp - endTime > 1500000) checkScatter();
                        if (scatter && frame.Hands.Count == 1 && frame.Timestamp - endTime > 1000000) checkGrab();
                        if (!scatter && frame.Hands.Count == 1) checkSceneChange();
                        if (frame.Hands.Count == 2)
                        {
                            setHand();
                            checkZoom();
                            if (scatter)
                            {
                                checkGather();
                            }
                        }
                    }
                }
                else // movie isn't active 
                {
                    checkSceneChange();
                }
            } // confidence
        }
        else
        {
            if (startScatter || startGather || startGrab || startZoom || stayGrab)
            {
                startScatter = false;
                startGather = false;
                startGrab = false;
                stayGrab = false;
                startZoom = false;
            }

            if (!idleTurn)
            {
                if (!stayIdle)
                {
                    stayIdle = true;
                    startTime = frame.Timestamp;
                }
                else
                {
                    if (frame.Timestamp - startTime > 3000000)
                    {
                        idleTurn = true;
                        objectPV.RPC("setIdleTurn", PhotonTargets.All, true);
                    }
                }
            }
        }
    }

    void checkSceneChange()
    {
        if (currentHand.PinchStrength > 0.9 && !fingers[1].IsExtended && fingers[2].IsExtended
                                    && fingers[2].StabilizedTipPosition.DistanceTo(fingers[3].StabilizedTipPosition) > 30.0f)
        {
            if (!startGrab)
            {
                startGrab = true;
                startTime = frame.Timestamp;
            }
            else
            {
                if (frame.Timestamp - startTime > 750000
                    && objectCtrl.State == ObjectCtrl.ObjectState.idle)
                {
                    setSceneChangeCtrl(true);
                }
            }
        }
    }

    void checkScatter()
    {
        if (!startScatter)
        {
            if (currentHand.GrabStrength == 1 && currentHand.PalmVelocity.Magnitude < 350.0f)
            {
                if (!stayGrab)
                {
                    stayGrab = true;
                    startTime = frame.Timestamp;
                }
                else
                {
                    if (frame.Timestamp - startTime > 500000)
                    {
                        startScatter = true;
                    }
                }
            }
            else
            {
                stayGrab = false;
            }
        }
        else
        {
            if (currentHand.GrabStrength == 0 && stayGrab)
            {
                objectPV.RPC("setObjectState", PhotonTargets.All, 2);
                stayGrab = false;
                scatter = true;
                startScatter = false;
                startTime = float.MaxValue;
                endTime = frame.Timestamp;
            }
            else if (currentHand.PalmVelocity.Magnitude > 700.0f)
            {
                startScatter = false;
            }
        }
    }

    void checkGather()
    {
        if (!startGather)
        {
            if (leftHand.GrabStrength == 1 && rightHand.GrabStrength == 1)
            {
                if (!stayGrab)
                {
                    stayGrab = true;
                    startTime = frame.Timestamp;
                }
                else
                {
                    if (frame.Timestamp - startTime > 750000)
                    {
                        twoHandDif = leftHand.PalmPosition.DistanceTo(rightHand.PalmPosition);
                        startGather = true;
                    }
                }
            }
            else
            {
                stayGrab = false;
            }

        }
        else
        {
            if (twoHandDif - leftHand.PalmPosition.DistanceTo(rightHand.PalmPosition) > 100.0f && stayGrab)
            {
                objectPV.RPC("setObjectState", PhotonTargets.All, 3);
                scatter = false;
                twoHandDif = float.MaxValue;
                startTime = float.MaxValue;
                stayGrab = false;
                startGather = false;
                endTime = frame.Timestamp;
            }
            if (leftHand.GrabStrength == 0 || rightHand.GrabStrength == 0)
            {
                startGather = false;
            }
        }

    }

    void checkGrab()
    {
        if (currentHand.PinchStrength > 0.9 && !fingers[1].IsExtended && fingers[2].IsExtended
                               && fingers[2].StabilizedTipPosition.DistanceTo(fingers[3].StabilizedTipPosition) > 30.0f)
        {
            if (!startGrab)
            {
                startGrab = true;
                startTime = frame.Timestamp;
            }
            else
            {
                if (frame.Timestamp - startTime > 750000
                    && objectCtrl.State == ObjectCtrl.ObjectState.idle)
                {
                   setGrabCtrl(true);
                }
            }
        }
    }

    void rightObject()
    {
        objectPV.RPC("setObjectState", PhotonTargets.All, 4);
    }

    void leftObject()
    {
        objectPV.RPC("setObjectState", PhotonTargets.All, 5);
    }

    void checkZoom()
    {
        if (leftHand.PinchStrength > 0.9f && rightHand.PinchStrength > 0.9f && leftHand.Fingers[2].IsExtended && leftHand.Fingers[3].IsExtended && leftHand.Fingers[4].IsExtended
                    && rightHand.Fingers[2].IsExtended && rightHand.Fingers[3].IsExtended && rightHand.Fingers[4].IsExtended)
        {
            if (!startZoom)
            {

                startZoom = true;
                startTime = frame.Timestamp;
            }
            else
            {
                if (frame.Timestamp - startTime > 500000)
                {
                    float leftMove, rightMove;
                    leftMove = leftHand.Fingers[1].TipPosition.x;
                    rightMove = rightHand.Fingers[1].TipPosition.x;
                    crtMove = rightMove - leftMove;
                    if (prevMove == 0)
                    {
                        prevMove = rightMove - leftMove;
                    }
                    else
                    {
                        if (prevMove < crtMove && (crtMove - prevMove) > 5)
                        {
                            objectPV.RPC("zoom", PhotonTargets.All, -1.0f);
                            prevMove = crtMove;

                        }
                        else if (prevMove > crtMove && (prevMove - crtMove) > 5)
                        {
                            objectPV.RPC("zoom", PhotonTargets.All, 1.0f);
                            prevMove = crtMove;

                        }
                    }
                }
            }
        }
        else
        {
            startZoom = false;
            endTime = frame.Timestamp;
        }
    }

    void setGrabCtrl(bool active)
    {

        if (!active)
        {
            tr[focusIndex].GetComponent<GrabCtrl>().enabled = false;
            startGrab = false;
            endTime = frame.Timestamp;
        }
        else
        {
            tr[focusIndex].GetComponent<GrabCtrl>().enabled = true;
        }
    }

    void setSceneChangeCtrl(bool active)
    {
        if (!active)
        {
             sceneChangeCtrl.enabled = false;
            startGrab = false;
            endTime = frame.Timestamp;
        }
        else
        {
            sceneChangeCtrl.enabled = true;
        }
    }

    void setMovieCtrl(bool active) 
    {
        if (active) movie.active = true;
        else movie.active = false;
    }
}// class