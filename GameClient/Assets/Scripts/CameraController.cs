using System;
using UnityEngine;
using UnityEngine.Animations;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Player player;

    [SerializeField]
    private float sensitifity = 100f;

    [SerializeField]
    private float clampAngle = 90f;

    private float horizontalRotation;
    private float verticalRotation;

    private void OnValidate()
    {
        if(player == null)
            player = GetComponentInParent<Player>();
    }

    private void Start()
    {
        verticalRotation = transform.localEulerAngles.x;
        horizontalRotation = transform.localEulerAngles.y;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleCursorMode();

        if (Cursor.lockState == CursorLockMode.Locked)
            Look();

        Debug.DrawRay(transform.position, transform.forward * 2f, Color.green);
    }

    private void Look()
    {
        float mouseVertical = -Input.GetAxisRaw("Mouse Y");
        float mouseHorizontal = Input.GetAxisRaw("Mouse X");

        verticalRotation += mouseVertical * sensitifity * Time.deltaTime;
        horizontalRotation += mouseHorizontal * sensitifity * Time.deltaTime;

        verticalRotation = Mathf.Clamp(verticalRotation, -clampAngle, clampAngle);

        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        player.transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
    }

    private void ToggleCursorMode()
    {
        Cursor.visible = !Cursor.visible;

        if (Cursor.lockState == CursorLockMode.None)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }
}
