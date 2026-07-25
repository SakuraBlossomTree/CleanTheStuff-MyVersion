using UnityEngine;
using System;

public class TrashThrower : MonoBehaviour
{
    [Header("Throw Settings")]
    public Transform throwOrigin;        // OPTIONAL: leave empty to use the camera
    public float throwForce = 18f;
    public float spawnDistance = 1f;
    public GameObject thrownTrashPrefab;

    [Header("Selection")]
    public TrashType selectedType = TrashType.Red;

    private UnityEngine.Camera playerCamera;
    private Grapple grapple;

    void Start()
    {
        playerCamera = GetComponentInChildren<UnityEngine.Camera>();
        if (playerCamera == null)
            playerCamera = UnityEngine.Camera.main;

        grapple = GetComponent<Grapple>();
    }

    void Update()
    {
        HandleSelection();
        HandleThrow();
    }

    void HandleSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) selectedType = TrashType.Red;
        if (Input.GetKeyDown(KeyCode.Alpha2)) selectedType = TrashType.Blue;
        if (Input.GetKeyDown(KeyCode.Alpha3)) selectedType = TrashType.Yellow;
        if (Input.GetKeyDown(KeyCode.Alpha4)) selectedType = TrashType.Green;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            int current = (int)selectedType;
            int count = Enum.GetValues(typeof(TrashType)).Length;
            int next = (current + (scroll > 0f ? 1 : count - 1)) % count;
            selectedType = (TrashType)next;
        }
    }

    void HandleThrow()
    {
        if (!Input.GetMouseButtonDown(1)) return; // Right click

        if (BackpackManager.Instance == null || !BackpackManager.Instance.HasTrash(selectedType))
        {
            if (BackpackManager.Instance != null)
                BackpackManager.Instance.ShowNotification("No " + selectedType + " trash");
            return;
        }

        // BLUE = camera-based grapple (no projectile). Only consume trash if it connects.
        if (selectedType == TrashType.Blue)
        {
            if (grapple != null && grapple.TryGrapple())
            {
                BackpackManager.Instance.RemoveTrash(selectedType);
            }
            else
            {
                BackpackManager.Instance.ShowNotification("No surface in range");
            }
            return;
        }

        // OTHER TYPES = thrown projectile
        BackpackManager.Instance.RemoveTrash(selectedType);

        Transform origin = throwOrigin != null ? throwOrigin : playerCamera.transform;
        Vector3 spawnPos = origin.position + origin.forward * spawnDistance;
        Quaternion spawnRot = origin.rotation;

        GameObject thrown = Instantiate(thrownTrashPrefab, spawnPos, spawnRot);
        ThrownTrash comp = thrown.GetComponent<ThrownTrash>();
        if (comp != null)
        {
            comp.trashType = selectedType;
            comp.Launch(origin.forward, throwForce);
        }
    }
}