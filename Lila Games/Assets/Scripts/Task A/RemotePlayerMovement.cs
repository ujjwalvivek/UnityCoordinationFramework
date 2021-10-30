using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayerMovement : MonoBehaviour
{
    [Header("Component References")]
    public Rigidbody remoteRB;

    private LocalPlayerMovement localPlayer;

    void Awake()
    { 
        if (!localPlayer)
        {
            localPlayer = FindObjectOfType<LocalPlayerMovement>();
        }
    }

    void FixedUpdate()
    {
        if (Input.GetAxisRaw("Vertical") != 0f)
        {
            ProcessMovement();
        }
        
        if (Input.GetAxisRaw("Horizontal") != 0f)
        {
            ProcessRotation();
        }
    }

    void ProcessMovement()
    {
        Vector2 tempPos = localPlayer.LocalPlayerPosition();
        remoteRB.MovePosition(new Vector3(tempPos.x, transform.position.y, tempPos.y + 20f)); 
    }

    void ProcessRotation()
    {
        Vector3 tempRot = new Vector3(transform.rotation.x, localPlayer.LocalPlayerRotation(), transform.rotation.eulerAngles.z);
        remoteRB.MoveRotation(Quaternion.Euler(tempRot));
    }
}
