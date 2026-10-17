#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEngine;
using Input = StudyInput;

namespace SecretOfLostTemple.AR
{
    /// <summary>
    /// In Unity Editor only: monitors the webcam feed for contrast/testing.
    /// Completely excluded from Android builds so it never triggers false scans.
    /// </summary>
    public class WebCamMarkerDetector : MonoBehaviour
    {
        [SerializeField] private LiveWebCamBackground liveWebCam;
        [SerializeField] private ARTrackedImageMachineAnchor machineAnchor;

        private void Start()
        {
            if (liveWebCam == null) liveWebCam = FindFirstObjectByType<LiveWebCamBackground>();
            if (machineAnchor == null) machineAnchor = FindFirstObjectByType<ARTrackedImageMachineAnchor>();
        }

        private void Update()
        {
            if (liveWebCam == null || machineAnchor == null) return;

            // Spacebar in Editor to simulate marker detection for quick PC testing
            if (Input.GetKeyDown(KeyCode.Space))
            {
                machineAnchor.SimulateMarkerScan();
            }
        }
    }
}
#endif
