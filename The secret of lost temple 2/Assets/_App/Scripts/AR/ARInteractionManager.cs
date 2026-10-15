using System;
using System.Collections.Generic;
using UnityEngine;

namespace SecretOfLostTemple.AR
{
    /// <summary>
    /// Central controller for the MainARScene.
    /// Manages AR plane detection, tap-to-place workflow, and delegates touch events.
    /// Uses reflection/abstractions where AR Foundation may not be present in editor.
    /// </summary>
    public class ARInteractionManager : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private SimpleMachine leverPrefab;
        [SerializeField] private SimpleMachine pulleyPrefab;
        [SerializeField] private SimpleMachine inclinedPlanePrefab;
        [SerializeField] private SimpleMachine wheelAxlePrefab;
        
        [Header("AR References")]
        [SerializeField] private GameObject placementIndicator;
        [SerializeField] private ARModeSwitcher modeSwitcher;

        private List<SimpleMachine> spawnedMachines = new List<SimpleMachine>();
        private SimpleMachine currentSelectedMachine;
        private bool isSessionActive;

        public event Action<SimpleMachine> OnMachineSpawned;
        public event Action<SimpleMachine> OnMachineSelected;
        public event Action OnSessionStarted;
        public event Action OnSessionEnded;

        private void Start()
        {
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false);
            }
        }

        private void Update()
        {
            if (!isSessionActive) return;

            // Handle touch inputs (single drag, two finger pinch, long press)
            HandleTouchInput();
        }

        /// <summary>
        /// Starts the AR session.
        /// </summary>
        public void StartSession()
        {
            isSessionActive = true;
            OnSessionStarted?.Invoke();
            CognitiveTelemetryLogger.Instance.StartSession(SecretOfLostTemple.Research.Research.Store.ParticipantCode);
        }

        /// <summary>
        /// Ends the AR session.
        /// </summary>
        public void EndSession()
        {
            isSessionActive = false;
            OnSessionEnded?.Invoke();
            CognitiveTelemetryLogger.Instance.EndSession();
        }

        /// <summary>
        /// Spawns a specific machine type at a position and rotation.
        /// </summary>
        public SimpleMachine SpawnMachine(MachineType type, Vector3 position, Quaternion rotation)
        {
            SimpleMachine prefab = GetPrefabForType(type);
            if (prefab == null) return null;

            SimpleMachine instance = Instantiate(prefab, position, rotation);
            spawnedMachines.Add(instance);
            
            CognitiveTelemetryLogger.Instance.LogInteraction("spawn_machine", instance, $"{{\"type\":\"{type}\"}}");
            
            OnMachineSpawned?.Invoke(instance);
            SelectMachine(instance);
            return instance;
        }

        public SimpleMachine GetCurrentMachine()
        {
            return currentSelectedMachine;
        }

        private void SelectMachine(SimpleMachine machine)
        {
            currentSelectedMachine = machine;
            OnMachineSelected?.Invoke(machine);
        }

        private SimpleMachine GetPrefabForType(MachineType type)
        {
            switch (type)
            {
                case MachineType.Lever: return leverPrefab;
                case MachineType.Pulley: return pulleyPrefab;
                case MachineType.InclinedPlane: return inclinedPlanePrefab;
                case MachineType.WheelAndAxle: return wheelAxlePrefab;
                default: return null;
            }
        }

        private void HandleTouchInput()
        {
            if (!StudyPolicy.InputAllowed) return;
            if (Input.touchCount == 0) return;

            Touch touch = Input.GetTouch(0);
            
            if (currentSelectedMachine == null) return;

            // Basic touch translation logic (placeholder for actual touch processing)
            if (Input.touchCount == 1 && touch.phase == TouchPhase.Moved)
            {
                if (currentSelectedMachine.IsInteractive)
                {
                    CognitiveTelemetryLogger.Instance.LogInteraction("drag", currentSelectedMachine);
                }
                else
                {
                    CognitiveTelemetryLogger.Instance.LogInteraction("rotate", currentSelectedMachine);
                }
            }
        }

        // Methods to be called by AR Foundation delegates
        public void OnPlaneDetected(Vector3 position, Quaternion rotation)
        {
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(true);
                placementIndicator.transform.SetPositionAndRotation(position, rotation);
            }
        }

        public void OnTapToPlace(Vector3 position, Quaternion rotation)
        {
            // By default, spawn a lever if nothing is selected
            SpawnMachine(MachineType.Lever, position, rotation);
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false);
            }
        }
    }
}
