using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;
using System;

public class RotateCtrl : MonoBehaviour
{
    public GameObject mainObject;
    public GameObject sub1,sub2,sub3;
    public Controller controller;
    private Transform mainTr, subTr1, subTr2, subTr3;
    private Transform focusTr;

    // Use this for initialization
    void Awake()
    {
        mainTr = mainObject.GetComponent<Transform>();
        subTr1 = sub1.GetComponent<Transform>();
        subTr2 = sub2.GetComponent<Transform>();
        subTr3 = sub3.GetComponent<Transform>();
        setFocus(1);
        controller = new Controller();
    }

    void setFocus(int num)
    {
        switch(num)
        {
            case 1 :
                {
                    focusTr = mainTr;
                    break;
                }
            case 2:
                {
                    focusTr = subTr1;
                    break;
                }
            case 3:
                {
                    focusTr = subTr2;
                    break;
                }
            case 4:
                {
                    focusTr = subTr3;
                    break;
                }
        }
    }

    void OnTriggerStay(Collider other)
    {
        Frame frame = controller.Frame(); // controller is a Controller object
        if (frame.Hands.Count > 0)
        {

            List<Hand> hands = frame.Hands;
            Hand currentHand = hands[0];
            if (currentHand.Confidence > 0.98) // 98퍼센트이상의 정확도로 현재손을 인식하였을 때
            {
                float dx = currentHand.PalmVelocity.x / 100.0f;
                float dy = currentHand.PalmVelocity.z / 100.0f;
                focusTr.RotateAround(Vector3.zero, Vector3.down, Time.deltaTime * dx);
                focusTr.RotateAround(Vector3.zero, Vector3.left, Time.deltaTime * dy);
            }
        }
    }
}