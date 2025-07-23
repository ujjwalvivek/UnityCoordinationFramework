using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerMovement : MonoBehaviour
{
    [Header("Component References")]
    public Rigidbody localRB;

    [Header("Movement Variables")]
    public float moveSpeed;
    public float turnSpeed;

    /* --------- Public Accessors --------- */
    public float ForwardInput { get; set; }
    public float TurnInput { get; set; }

    private void Awake()
    {
        if (!localRB)
        {
            localRB = GetComponent<Rigidbody>();
        }
    }

    // Much better for Physics Calculations
    private void FixedUpdate()
    {
        ProcessActions();
    }

    private void ProcessActions()
    {
        if (TurnInput != 0f)
        {
            float angle = Mathf.Clamp(TurnInput, -1f, 1f) * turnSpeed;
            transform.Rotate(Vector3.up, Time.fixedDeltaTime * angle);
        }

        Vector3 move = Mathf.Clamp(ForwardInput, -1f, 1f) * moveSpeed * Time.fixedDeltaTime * transform.forward.normalized;
        localRB.MovePosition(transform.position + move);
    }

    public Vector2 LocalPlayerPosition()
    {
        Debug.Log("Co-ordinates Sent (X and Z): " + new Vector2(localRB.gameObject.transform.position.x, localRB.gameObject.transform.position.z) + " with a size of " + (sizeof(float) * 2) + " Bytes");
        return new Vector2(localRB.gameObject.transform.position.x, localRB.gameObject.transform.position.z);
    }

    public float LocalPlayerRotation()
    {
        Debug.Log("Angles Sent (Y): " + localRB.gameObject.transform.rotation.eulerAngles.y + " with a size of " + sizeof(float) + " Bytes");
        return localRB.gameObject.transform.rotation.eulerAngles.y;
    }
}
