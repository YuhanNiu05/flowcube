using System;
using System.Collections;
using UnityEngine;

namespace Flowcube.Hardware
{
    /// <summary>
    /// Communicates with the M5Stack device over BLE (or USB-Serial as fallback).
    ///
    /// The M5Stack firmware is expected to broadcast a JSON packet whenever its
    /// IMU detects a stable face change:
    ///   {"face": 0}   // learning/focus face up
    ///   {"face": 1}   // exercise face up
    ///   {"face": 2}   // rest / other face
    ///
    /// In the Editor / on a device without BLE this class falls back to
    /// keyboard shortcuts (F1 / F2 / F3) so the rest of the app can be tested.
    /// </summary>
    public class M5StackConnector : MonoBehaviour
    {
        // ─── Events ────────────────────────────────────────────────────────────
        /// <summary>Fired whenever the active face changes. Arg = face index (0-based).</summary>
        public event Action<int> OnFaceChanged;

        // ─── Inspector ─────────────────────────────────────────────────────────
        [Header("BLE Settings")]
        [Tooltip("BLE service UUID advertised by the M5Stack firmware.")]
        [SerializeField] private string serviceUUID = "4fafc201-1fb5-459e-8fcc-c5c9c331914b";

        [Tooltip("BLE characteristic UUID for face-change notifications.")]
        [SerializeField] private string characteristicUUID = "beb5483e-36e1-4688-b7f5-ea07361b26a8";

        [Tooltip("Seconds between connection retry attempts.")]
        [SerializeField] private float retryIntervalSeconds = 5f;

        [Header("Simulation (Editor / No Hardware)")]
        [Tooltip("When true, the app uses keyboard shortcuts instead of BLE.")]
        [SerializeField] private bool useKeyboardFallback = true;

        // ─── State ─────────────────────────────────────────────────────────────
        public int CurrentFace { get; private set; } = -1;
        public bool IsConnected { get; private set; }

        // ─── Unity lifecycle ───────────────────────────────────────────────────

        void Start()
        {
            if (!useKeyboardFallback)
                StartCoroutine(ConnectBLE());
        }

        void Update()
        {
            if (useKeyboardFallback)
                HandleKeyboardFallback();
        }

        // ─── BLE (stub – wire up to your platform BLE plugin) ──────────────────

        private IEnumerator ConnectBLE()
        {
            while (!IsConnected)
            {
                Debug.Log("[M5StackConnector] Attempting BLE connection...");

                // TODO: replace with your platform BLE plugin calls, e.g.:
                //   BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(...)
                // For now we just simulate a successful connection after a delay.
                yield return new WaitForSeconds(retryIntervalSeconds);

                // Placeholder: assume connected
                IsConnected = true;
                Debug.Log("[M5StackConnector] BLE connected (stub).");

                // In a real integration you would subscribe to characteristic
                // notifications and call NotifyFaceChanged() from the callback.
            }
        }

        /// <summary>
        /// Call this from your BLE notification callback when a face-change
        /// JSON packet is received from the M5Stack.
        /// Expected payload: {"face": N}
        /// </summary>
        public void OnBLEDataReceived(string jsonPayload)
        {
            try
            {
                var packet = JsonUtility.FromJson<FacePacket>(jsonPayload);
                NotifyFaceChanged(packet.face);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[M5StackConnector] Failed to parse BLE payload '{jsonPayload}': {e.Message}");
            }
        }

        // ─── Private ───────────────────────────────────────────────────────────

        private void NotifyFaceChanged(int face)
        {
            if (face == CurrentFace) return;
            CurrentFace = face;
            Debug.Log($"[M5StackConnector] Face changed to {face}");
            OnFaceChanged?.Invoke(face);
        }

        private void HandleKeyboardFallback()
        {
            if (Input.GetKeyDown(KeyCode.F1)) NotifyFaceChanged(0); // focus
            if (Input.GetKeyDown(KeyCode.F2)) NotifyFaceChanged(1); // exercise
            if (Input.GetKeyDown(KeyCode.F3)) NotifyFaceChanged(2); // rest
        }

        [Serializable]
        private class FacePacket { public int face; }
    }
}
