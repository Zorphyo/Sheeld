using Core.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class MountedWeapon : MonoBehaviour, IInteractable
{
    [Header("Mounting")]
    public Transform playerSnapPoint;
    public float fallbackDistanceBehindWeapon = 2.5f;
    public float playerHeightOffset = 0.9f;

    [Header("Rotation")]
    public float mouseSensitivity = 0.003f;
    public float maxDegreesPerFrame = 1.5f;

    protected PlayerController mountedPlayer;

    public virtual void Interact()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        PlayerController player = playerObj.GetComponent<PlayerController>();
        if (player == null) return;

        mountedPlayer = player;
        mountedPlayer.MountWeapon(this, transform);
    }

    public virtual void HandleMountedUpdate()
    {
        if (mountedPlayer == null)
            return;

        float mouseX = 0f;

        if (Mouse.current != null)
            mouseX = Mouse.current.delta.ReadValue().x;

        float yawAmount = mouseX * mouseSensitivity;
        yawAmount = Mathf.Clamp(yawAmount, -maxDegreesPerFrame, maxDegreesPerFrame);

        transform.Rotate(Vector3.up, yawAmount, Space.World);

        Vector3 desiredPos = GetMountedPlayerPosition();

        mountedPlayer.transform.position = desiredPos;
        mountedPlayer.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
    }

    private Vector3 GetMountedPlayerPosition()
    {
        Vector3 basePos;

        if (playerSnapPoint != null)
            basePos = playerSnapPoint.position;
        else
            basePos = transform.position - transform.forward * fallbackDistanceBehindWeapon;

        if (Physics.Raycast(basePos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 20f, mountedPlayer.groundLayer))
        {
            return hit.point + Vector3.up * playerHeightOffset;
        }

        return basePos + Vector3.up * playerHeightOffset;
    }

    public abstract void UseWeapon();
}