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
    public Button StartARButton;
    [SerializeField]
    public Text statusText;
    [SerializeField]
    public ARSessionOrigin arSessionOrigin;
    [SerializeField]
    public VideoPlayer arGuidelinePlayer;

    private void Start()
    {
        arGuidelinePlayer.gameObject.SetActive(false);
        statusText.enabled = false;
        StartARButton.onClick.AddListener(async () =>
        {
            arGuidelinePlayer.gameObject.SetActive(true);
            statusText.enabled = true;
            await ARCloudSession.instance.Init(new PositionGps()
            {
                Latitude = 0.0,
                Longitude = 0.0,
                Altitude = 0.0
            });

            ARCloudSession.instance.StartLocalize(LocalizeStrategy.LAST_POINT_DIFF_MEDPRECISION);
        });
    }

    private void Update()
    {
        statusText.text = $"A process of {ARCloudSession.instance.localizeProgress * 100}% complete\nPlease walk a bit while scanning";

        if (ARCloudSession.instance.localizeProgress >= 1.0)
        {
            arGuidelinePlayer.Stop();
            arGuidelinePlayer.gameObject.SetActive(false);
            statusText.enabled = false;
        }
    }
}
