using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerInput : MonoBehaviour
{
    private LocalPlayerMovement localPlayerController;

    void Awake()
    {
        localPlayerController = GetComponent<LocalPlayerMovement>();
    }

    private void Update()
    {
        localPlayerController.ForwardInput = Input.GetAxisRaw("Vertical");
        localPlayerController.TurnInput = Input.GetAxisRaw("Horizontal");
    }
}
