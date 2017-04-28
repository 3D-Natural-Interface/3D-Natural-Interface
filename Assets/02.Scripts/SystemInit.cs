using UnityEngine;
using System.Collections;

public class SystemInit : MonoBehaviour
{
    public string device;
    public GameObject leapController;
    public GameObject ObjectCtrl;
    public GameObject mobileCtrl;
    public GameObject photonInit;
    private bool connection = false;
    // Use this for initialization
    void Start()
    {
        device = SystemInfo.deviceType.ToString();
        Debug.Log("device = " + device);
        Debug.Log("view" + Camera.main.fieldOfView);
        if (device.Equals("Desktop"))
        {
            photonInit.active = true;
            setConnection(true);
        }
        else if (device.Equals("Handheld"))
        {
            mobileCtrl.active = true;
            GetComponent<CameraFlip>().enabled = false;
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    void OnDisable()
    {
        if(!PhotonNetwork.isMasterClient)
        GetComponent<PhotonView>().RPC("setLeapActive", PhotonTargets.MasterClient, true);
    }

    void setConnection(bool con)
    {
        if (con)
        {
            connection = true;
            if (device.Equals("Handheld"))
            {
                mobileCtrl.GetComponent<HandheldCtrl>().SendMessage("setConnection", true);
                Debug.Log("connetion handheld");
            }
            else if (device.Equals("Desktop") && PhotonNetwork.isMasterClient)
            {
                leapController.active = true;
                Debug.Log("you are master desktop!");
            }
        }
        else
        {
            connection = false;
            if (device.Equals("Handheld"))
            {
                mobileCtrl.GetComponent<HandheldCtrl>().SendMessage("setConnection", false);
                GetComponent<PhotonView>().RPC("setLeapActive", PhotonTargets.MasterClient, true);
            }
        }
    }
    [PunRPC]
    void setLeapActive(bool active)
    {
        if(active)
        {
            leapController.active = true;
        }
        else
        {
            leapController.active = false;
        }
    }
}
