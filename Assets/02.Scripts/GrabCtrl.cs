using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;
public class GrabCtrl : MonoBehaviour
{
    public GestureCtrl gestureCtrl;
    private Controller controller;
    private Frame frame;
    private List<Hand> hands;
    private Hand currentHand, startHand = null;
    private List<Finger> fingers;
    private Transform tr;
    private PhotonView pv;
    private float scaleVec;
    private ObjectCtrl objectCtrl;
    private Vector3 centerSub1 = new Vector3(-0.04426686f, -0.2077977f, 0.06664629f);
    private Vector3 centerSub2 = new Vector3(-0.05f, -0.2577977f, 0.06664629f);
    // Use this for initialization
    void OnEnable()
    {
        controller = new Controller();
        tr = GetComponent<Transform>();
        pv = GetComponentInParent<PhotonView>();
        objectCtrl = GetComponentInParent<ObjectCtrl>();
        scaleVec = tr.localScale.x / 20;
        tr.localScale = new Vector3(tr.localScale.x + scaleVec, tr.localScale.x + scaleVec, tr.localScale.x + scaleVec);
        pv.RPC("syncronizeObject", PhotonTargets.Others, tr.position, tr.rotation);
    }

    void OnDisable()
    {
        tr.localScale = new Vector3(tr.localScale.x - scaleVec, tr.localScale.x - scaleVec, tr.localScale.x - scaleVec);
        pv.RPC("syncronizeObject", PhotonTargets.Others, tr.position, tr.rotation);
        if (tr.position.x < -0.4f)
        {
            pv.RPC("setObjectState", PhotonTargets.All, 5);
        }
        else if (tr.position.x > 0.4f)
        {
            pv.RPC("setObjectState", PhotonTargets.All, 4);
        }
        else
        {
            switch (name)
            {
                case "sub03":
                    tr.position = Vector3.zero;
                    break;
                case "sub02":
                    tr.position = centerSub2;
                    break;
                case "sub01":
                    tr.position = centerSub1;
                    break;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        frame = controller.Frame(); // controller is a Controller object
        if (frame.Hands.Count > 0)
        {
            hands = frame.Hands;
            currentHand = hands[0];
            if (startHand == null)
                startHand = currentHand;
            fingers = currentHand.Fingers;
            if (currentHand.Confidence > 0.98) // 98퍼센트이상의 정확도로 현재손을 인식하였을 때
            {
                tr.position = Vector3.Lerp(tr.position, new Vector3(-(startHand.Fingers[1].TipPosition.x - currentHand.Fingers[1].TipPosition.x) / 300, tr.position.y, tr.position.z), Time.deltaTime * 5.0f);
                pv.RPC("syncronizeObject", PhotonTargets.Others, tr.position, tr.rotation);
                if (fingers[1].IsExtended
                    ||tr.position.x < -0.6f || tr.position.x > 0.6f)
                {
                    gestureCtrl.SendMessage("setGrabCtrl", false);
                    startHand = null;
                }
            }
        }
    }
}
