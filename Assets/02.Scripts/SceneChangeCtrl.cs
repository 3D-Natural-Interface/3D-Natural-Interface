using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;
public class SceneChangeCtrl : MonoBehaviour
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
    public GameObject movie;
    // Use this for initialization
    void OnEnable()
    {
        controller = new Controller();
        tr = GetComponent<Transform>();
        pv = GetComponent<PhotonView>();
        objectCtrl = GetComponent<ObjectCtrl>();
        scaleVec = tr.localScale.x / 20;
        tr.localScale = new Vector3(tr.localScale.x + scaleVec, tr.localScale.x + scaleVec, tr.localScale.x + scaleVec);
        pv.RPC("syncronizeObject", PhotonTargets.Others, tr.position, tr.rotation);
    }
    void OnDisable()
    {
        tr.localScale = new Vector3(tr.localScale.x - scaleVec, tr.localScale.x - scaleVec, tr.localScale.x - scaleVec);
        pv.RPC("syncronizeObject", PhotonTargets.Others, tr.position, tr.rotation);
        if ((tr.position.y < -0.2f || tr.position.y > 0.2f) && tr.position.x < 0.4f && tr.position.x > -0.4f)
        {
            if (!movie.active)
            {
                gestureCtrl.SendMessage("setMovieCtrl", true);
            }
            else
            {
                gestureCtrl.SendMessage("setMovieCtrl", false);
            }
        }
        else if (movie.active && tr.position.y > -0.2f && tr.position.y < 0.2f && (tr.position.x > 0.4f || tr.position.x < -0.4f))
        {
            if (tr.position.x < -0.4f)
                movie.GetComponent<MovieCtrl>().SendMessage("moviePreviousChange");
            else
                movie.GetComponent<MovieCtrl>().SendMessage("movieChange");
        }
        tr.position = Vector3.zero;
    }

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
                tr.position = new Vector3(-(startHand.PalmPosition.x - currentHand.PalmPosition.x) / 200,
                        (currentHand.Fingers[1].TipPosition.y - startHand.Fingers[1].TipPosition.y) / 200, tr.position.z);
                pv.RPC("syncronizeObject", PhotonTargets.Others, tr.position, tr.rotation);
                if (fingers[1].IsExtended
                    || tr.position.x < -0.6f || tr.position.x > 0.6f || tr.position.y < -0.4f || tr.position.x > 0.4f)
                {
                    gestureCtrl.SendMessage("setSceneChangeCtrl", false);
                    startHand = null;
                }
            }
        }
    }
}

