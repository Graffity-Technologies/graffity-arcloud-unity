using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Graffity.ARCloud;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ARCloudSampleView : MonoBehaviour
{
    public Button InitButton;
    public Button StartARButton;
    public Text StatusText;
    public ARSessionOrigin arSessionOrigin;

    private void Start()
    {
        InitButton.onClick.AddListener(() =>
        {
            ARCloudSession.instance.Init(new PositionGps()
            {
                Latitude = 0.0,
                Longitude = 0.0,
                Altitude = 0.0
            });
        });
        StartARButton.onClick.AddListener(() =>
        {
            ARCloudSession.instance.StartLocalize(LocalizeStrategy.LAST_POINT_DIFF_MEDPRECISION);
        });
    }

    private void Update()
    {
        StatusText.text =
            $"localizeState: {ARCloudSession.instance.localizeState}\n" +
            $"localizeProgress: {ARCloudSession.instance.localizeProgress * 100}%\n";
    }
}
