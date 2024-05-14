using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Graffity.ARCloud;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR.ARFoundation;

public class ARCloudSampleView : MonoBehaviour
{
    [SerializeField]
    public Button startARButton;
    [SerializeField]
    public Text statusText;
    [SerializeField]
    public ARSessionOrigin arSessionOrigin;
    [SerializeField]
    public VideoPlayer arGuidelinePlayer;
    [SerializeField]
    public GameObject arContents;

    private void Start()
    {
        arContents.SetActive(false);
        arGuidelinePlayer.gameObject.SetActive(false);
        statusText.enabled = false;
        startARButton.onClick.AddListener(async () =>
        {
            startARButton.gameObject.SetActive(false);
            arGuidelinePlayer.gameObject.SetActive(true);
            statusText.enabled = true;
            await ARCloudSession.instance.Init(new PositionGps()
            {
                Latitude = 0.0,
                Longitude = 0.0,
                Altitude = 0.0
            });

            ARCloudSession.instance.StartLocalize();
        });
    }

    private void Update()
    {
        statusText.text = $"A process of {ARCloudSession.instance.localizeProgress * 100}% complete\nPlease walk a bit while scanning";

        if (ARCloudSession.instance.isInitialized)
        {
            arContents.SetActive(true);
            arGuidelinePlayer.Stop();
            arGuidelinePlayer.gameObject.SetActive(false);
            statusText.enabled = false;
        }
    }
}
