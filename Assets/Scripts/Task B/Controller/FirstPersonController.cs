#region Assembly References

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using System.Net;
#endif

#endregion

public class FirstPersonController : MonoBehaviour
{
    #region Rigidbody Definition

    private Rigidbody rb;

    #endregion

    #region Camera Movement Variables

    public Camera playerCamera;
    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;

    // Crosshair
    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;

    // Internal Variables
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private Image crosshairObject;

    #region Camera Zoom Variables

    public bool enableZoom = true;
    public bool holdToZoom = false;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;

    // Internal Variables
    private bool isZoomed = false;

    #endregion

    #endregion

    #region Movement Variables

    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;

    // Internal Variables
    private bool isWalking = false;

    #region Sprint

    public bool enableSprint = true;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintSpeed = 7f;
    public float sprintFOV = 80f;
    public float sprintFOVStepTime = 10f;

    // Internal Variables
    private bool isSprinting = false;

    #endregion

    #region Jump

    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;

    // Internal Variables
    private bool isGrounded = false;

    #endregion

    #region Crouch

    public bool enableCrouch = true;
    public bool holdToCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = .75f;
    public float speedReduction = .5f;

    // Internal Variables
    private bool isCrouched = false;
    private Vector3 originalScale;

    #endregion

    #endregion

    #region Head Bob

    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(.15f, .05f, 0f);

    // Internal Variables
    private Vector3 jointOriginalPos;
    private float timer = 0;

    #endregion

    #region Weapon Manager

    public Item[] items;
    [HideInInspector] public int itemIndex;
    [HideInInspector] public int previousItemIndex = -1;

    #endregion

    #region Private Variables

    float camRotation;
    VisualsUI visuals;
    float recoil;
    float revert;

    #endregion

    #region Awake Function

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        crosshairObject = GetComponentInChildren<Image>();
        visuals = GetComponent<VisualsUI>();

        // Set internal variables
        playerCamera.fieldOfView = fov;
        originalScale = transform.localScale;
        jointOriginalPos = joint.localPosition;

        UpdateAmmo();
    }

    #endregion

    #region Start Function

    void Start()
    {
        #region Cursor Lock

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        #endregion

        #region Crosshair Enabler

        if (crosshair)
        {
            crosshairObject.sprite = crosshairImage;
            crosshairObject.color = crosshairColor;
        }
        else
        {
            crosshairObject.gameObject.SetActive(false);
        }

        #endregion

        #region Equipping Weapons

        EquipItems(0);

        #endregion
    }

    #endregion

    #region Update Function

    private void Update()
    {
        #region Camera

        // Control camera movement
        if (cameraCanMove)
        {
            yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

            if (!invertCamera)
            {
                pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");
            }
            else
            {
                // Inverted Y
                pitch += mouseSensitivity * Input.GetAxis("Mouse Y");
            }

            // Clamp pitch between lookAngle
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

            transform.localEulerAngles = new Vector3(0, yaw, 0);
            playerCamera.transform.localEulerAngles = new Vector3(pitch + -recoil, 0, 0);

            if (recoil > 0f)
            {
                recoil -= Time.deltaTime * revert;
            }
            else
            {
                recoil = 0f;
            }
        }

        #region Camera Zoom

        if (enableZoom)
        {
            // Changes isZoomed when key is pressed
            // Behavior for toogle zoom
            if (Input.GetKeyDown(zoomKey) && !holdToZoom && !isSprinting)
            {
                if (!isZoomed)
                {
                    isZoomed = true;
                }
                else
                {
                    isZoomed = false;
                }
            }

            // Changes isZoomed when key is pressed
            // Behavior for hold to zoom
            if (holdToZoom && !isSprinting)
            {
                if (Input.GetKeyDown(zoomKey))
                {
                    isZoomed = true;
                }
                else if (Input.GetKeyUp(zoomKey))
                {
                    isZoomed = false;
                }
            }

            // Lerps camera.fieldOfView to allow for a smooth transistion
            if (isZoomed)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, zoomFOV, zoomStepTime * Time.deltaTime);
            }
            else if (!isZoomed && !isSprinting)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, zoomStepTime * Time.deltaTime);
            }
        }

        #endregion

        #endregion

        #region Sprint

        if (enableSprint)
        {
            if (isSprinting)
            {
                isZoomed = false;
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, sprintFOVStepTime * Time.deltaTime);
            }
            else
            {
                isZoomed = true;
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, sprintFOVStepTime * Time.deltaTime);
            }
        }

        #endregion

        #region Jump

        // Gets input and calls jump method
        if (enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }

        #endregion

        #region Crouch

        if (enableCrouch)
        {
            if (Input.GetKeyDown(crouchKey) && !holdToCrouch)
            {
                Crouch();
            }

            if (Input.GetKeyDown(crouchKey) && holdToCrouch)
            {
                isCrouched = false;
                Crouch();
            }
            else if (Input.GetKeyUp(crouchKey) && holdToCrouch)
            {
                isCrouched = true;
                Crouch();
            }
        }

        #endregion

        #region Ground Check

        CheckGround();

        #endregion

        #region HeadBob Enabler

        if (enableHeadBob)
        {
            HeadBob();
        }

        #endregion

        #region Weapon Switcher

        SwitchWeapon();

        #endregion

        #region Use Weapon

        items[itemIndex].UseWeapon();

        #endregion

        #region Update HUD

        UpdateUI();

        #endregion
    }

    #endregion

    #region Fixed Update Function For RigidBody Physics

    void FixedUpdate()
    {
        #region Movement

        if (playerCanMove)
        {
            // Calculate how fast the player should be moving
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            // Checks if player is walking and isGrounded
            // Will allow head bob
            if (targetVelocity.x != 0 || targetVelocity.z != 0 && isGrounded)
            {
                isWalking = true;
            }
            else
            {
                isWalking = false;
            }

            // All movement calculations shile sprint is active
            if (enableSprint && Input.GetKey(sprintKey))
            {
                targetVelocity = transform.TransformDirection(targetVelocity) * sprintSpeed;

                // Apply a force that attempts to reach the target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                // Player is only moving when valocity change != 0
                // Makes sure fov change only happens during movement
                if (velocityChange.x != 0 || velocityChange.z != 0)
                {
                    isSprinting = true;

                    if (isCrouched)
                    {
                        Crouch();
                    }
                }

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }

            // All movement calculations while walking
            else
            {
                isSprinting = false;

                targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;

                // Apply a force that attempts to reach the target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
        }

        #endregion
    }

    #endregion

    #region Ground Check Raycast

    // Sets isGrounded based on a raycast sent straigth down from the player object
    private void CheckGround()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * .5f), transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = .75f;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            Debug.DrawRay(origin, direction * distance, Color.red);
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    #endregion

    #region Weapon Recoil

    public void Recoil(float amount, float revertRecoil)
    {
        recoil = amount;
        revert = revertRecoil;

        for (int i = 0; i <= items.Length - 1; i++)
        {
            if (items[i].gameObject.activeSelf)
            {
                ((WeaponVariables)items[i].weaponData).recoil = amount;
                ((WeaponVariables)items[i].weaponData).revert = revertRecoil;
            }
            else
            {
                return;
            }
        }
    }

    #endregion

    #region Jump

    private void Jump()
    {
        // Adds force to the player rigidbody to jump
        if (isGrounded)
        {
            rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
            isGrounded = false;
        }

        // When crouched and using toggle system, will uncrouch for a jump
        if (isCrouched && !holdToCrouch)
        {
            Crouch();
        }
    }

    #endregion

    #region Crouch

    private void Crouch()
    {
        // Stands player up to full height
        // Brings walkSpeed back up to original speed
        if (isCrouched)
        {
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
            walkSpeed /= speedReduction;

            isCrouched = false;
        }
        // Crouches player down to set height
        // Reduces walkSpeed
        else
        {
            transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
            walkSpeed *= speedReduction;

            isCrouched = true;
        }
    }

    #endregion

    #region Head Bobbing

    private void HeadBob()
    {
        if (isWalking)
        {
            // Calculates HeadBob speed during sprint
            if (isSprinting)
            {
                timer += Time.deltaTime * (bobSpeed + sprintSpeed);
            }
            // Calculates HeadBob speed during crouched movement
            else if (isCrouched)
            {
                timer += Time.deltaTime * (bobSpeed * speedReduction);
            }
            // Calculates HeadBob speed during walking
            else
            {
                timer += Time.deltaTime * bobSpeed;
            }
            // Applies HeadBob movement
            joint.localPosition = new Vector3(jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x, jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y, jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z);
        }
        else
        {
            // Resets when play stops moving
            timer = 0;
            joint.localPosition = new Vector3(Mathf.Lerp(joint.localPosition.x, jointOriginalPos.x, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.y, jointOriginalPos.y, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.z, jointOriginalPos.z, Time.deltaTime * bobSpeed));
        }
    }

    #endregion

    #region Equip Weapons

    public void EquipItems(int _index)
    {
        if (_index == previousItemIndex)
            return;

        itemIndex = _index;
        items[itemIndex].weaponGameObject.SetActive(true);
        visuals.ShowElement(itemIndex);
        visuals.weaponName.text = items[itemIndex].weaponGameObject.name;

        if (previousItemIndex != -1)
        {
            items[previousItemIndex].weaponGameObject.SetActive(false);
            visuals.HideElement(previousItemIndex);
        } 

        previousItemIndex = itemIndex;
    }

    #endregion

    #region Weapon Switcher

    public void SwitchWeapon()
    {
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if (itemIndex <= 0)
            {
                EquipItems(items.Length - 1);
            }
            else
            {
                EquipItems(itemIndex - 1);
            }
        }

        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex >= items.Length - 1)
            {
                EquipItems(0);
            }
            else
            {
                EquipItems(itemIndex + 1);
            }
        }
    }

    #endregion

    #region Show Ammunition Data

    public void UpdateUI()
    {
        for (int i = 0; i <= items.Length - 1; i++)
        {
            if (items[i].gameObject.activeSelf)
            {
                items[i].gameObject.GetComponent<UpdateHUD>().ammoUpdateCounter.text = ((WeaponVariables)items[i].weaponData).bulletsLeft + " / " + (((WeaponVariables)items[i].weaponData).totalAmmoCount).ToString();

                //if (((WeaponVariables)items[i].weaponData).totalAmmoCount > ((WeaponVariables)items[i].weaponData).magazineSize)
                //{
                //    items[i].gameObject.GetComponent<UpdateHUD>().ammoUpdateCounter.text = ((WeaponVariables)items[i].weaponData).bulletsLeft + " / " + (((WeaponVariables)items[i].weaponData).magazineSize).ToString();
                //}
                //else
                //{
                //    items[i].gameObject.GetComponent<UpdateHUD>().ammoUpdateCounter.text = ((WeaponVariables)items[i].weaponData).bulletsLeft + " / " + 0.ToString();
                //}
            }
            else
            {
                items[i].gameObject.GetComponent<UpdateHUD>().ammoUpdateCounter.text = ((WeaponVariables)items[i].weaponData).totalAmmoCount.ToString();
            }   
        }
    }

    #endregion

    #region Update Total Ammunition Count

    public void UpdateAmmo()
    {
        for (int i = 0; i <= items.Length - 1; i++)
        {
            ((WeaponVariables)items[i].weaponData).totalAmmoCount = ((WeaponVariables)items[i].weaponData).magazineSize * ((WeaponVariables)items[i].weaponData).numberOfMags;
        }
    }

    #endregion
}


#region Custom Editor

#if UNITY_EDITOR
[CustomEditor(typeof(FirstPersonController)), InitializeOnLoadAttribute]
public class FirstPersonControllerEditor : Editor
{
    #region Internal Variables

    GUISkin guiSkin;
    FirstPersonController fpsController;
    SerializedObject serializedController;

    #endregion

    #region OnEnable Function

    private void OnEnable()
    {
        fpsController = (FirstPersonController)target;
        serializedController = new SerializedObject(fpsController);
        guiSkin = (GUISkin)Resources.Load("GUI Skin/ControllerSkin", typeof(GUISkin));
    }

    #endregion

    #region OnInspectorGUI Function

    public override void OnInspectorGUI()
    {
        GUI.skin = guiSkin;

        serializedController.Update();

        EditorGUILayout.Space();
        GUILayout.Label("Unity Coordination Framework", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 18 });
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("RigidBody Character Controller", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 16 });
        GUILayout.Label("By Ujjwal Vivek", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 12 });
        GUILayout.Label("version 1.0", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 12 });
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Note: This character controller has been developed fully from scratch, solely for the purpose of this assignment. This controller is not viable for production purposes.",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Normal, fontSize = 12, wordWrap = true });
        EditorGUILayout.Space();

        #region Camera Setup

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Camera Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        fpsController.playerCamera = (Camera)EditorGUILayout.ObjectField(new GUIContent("Camera", "Camera attached to the controller."), fpsController.playerCamera, typeof(Camera), true);
        fpsController.fov = EditorGUILayout.Slider(new GUIContent("Field of View", "The camera’s view angle. Changes the player camera directly."), fpsController.fov, fpsController.zoomFOV, 179f);
        fpsController.cameraCanMove = EditorGUILayout.ToggleLeft(new GUIContent("Enable Camera Rotation", "Determines if the camera is allowed to move."), fpsController.cameraCanMove);

        GUI.enabled = fpsController.cameraCanMove;
        fpsController.invertCamera = EditorGUILayout.ToggleLeft(new GUIContent("Invert Camera Rotation", "Inverts the up and down movement of the camera."), fpsController.invertCamera);
        fpsController.mouseSensitivity = EditorGUILayout.Slider(new GUIContent("Look Sensitivity", "Determines how sensitive the mouse movement is."), fpsController.mouseSensitivity, .1f, 10f);
        fpsController.maxLookAngle = EditorGUILayout.Slider(new GUIContent("Max Look Angle", "Determines the max and min angle the player camera is able to look."), fpsController.maxLookAngle, 40, 90);
        GUI.enabled = true;

        fpsController.lockCursor = EditorGUILayout.ToggleLeft(new GUIContent("Lock and Hide Cursor", "Turns off the cursor visibility and locks it to the middle of the screen."), fpsController.lockCursor);

        fpsController.crosshair = EditorGUILayout.ToggleLeft(new GUIContent("Auto Crosshair", "Determines if the basic crosshair will be turned on, and sets is to the center of the screen."), fpsController.crosshair);

        // Only displays crosshair options if crosshair is enabled
        if (fpsController.crosshair)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Crosshair Image", "Sprite to use as the crosshair."));
            fpsController.crosshairImage = (Sprite)EditorGUILayout.ObjectField(fpsController.crosshairImage, typeof(Sprite), false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            fpsController.crosshairColor = EditorGUILayout.ColorField(new GUIContent("Crosshair Color", "Determines the color of the crosshair."), fpsController.crosshairColor);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        #region Camera Zoom Setup

        GUILayout.Label("Zoom", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

        fpsController.enableZoom = EditorGUILayout.ToggleLeft(new GUIContent("Enable Zoom", "Determines if the player is able to zoom in while playing."), fpsController.enableZoom);

        GUI.enabled = fpsController.enableZoom;
        fpsController.holdToZoom = EditorGUILayout.ToggleLeft(new GUIContent("Hold to Zoom", "Requires the player to hold the zoom key instead if pressing to zoom and unzoom."), fpsController.holdToZoom);
        fpsController.zoomKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Zoom Key", "Determines what key is used to zoom."), fpsController.zoomKey);
        fpsController.zoomFOV = EditorGUILayout.Slider(new GUIContent("Zoom FOV", "Determines the field of view the camera zooms to."), fpsController.zoomFOV, .1f, fpsController.fov);
        fpsController.zoomStepTime = EditorGUILayout.Slider(new GUIContent("Step Time", "Determines how fast the FOV transitions while zooming in."), fpsController.zoomStepTime, .1f, 10f);
        EditorGUILayout.Space();
        GUI.enabled = true;

        #endregion

        #endregion

        #region Movement Setup

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Movement Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        fpsController.playerCanMove = EditorGUILayout.ToggleLeft(new GUIContent("Enable Player Movement", "Determines if the player is allowed to move."), fpsController.playerCanMove);

        GUI.enabled = fpsController.playerCanMove;
        fpsController.walkSpeed = EditorGUILayout.Slider(new GUIContent("Walk Speed", "Determines how fast the player will move while walking."), fpsController.walkSpeed, .1f, fpsController.sprintSpeed);
        GUI.enabled = true;

        EditorGUILayout.Space();

        #region Sprint

        GUILayout.Label("Sprint", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

        fpsController.enableSprint = EditorGUILayout.ToggleLeft(new GUIContent("Enable Sprint", "Determines if the player is allowed to sprint."), fpsController.enableSprint);

        GUI.enabled = fpsController.enableSprint;
        fpsController.sprintKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Sprint Key", "Determines what key is used to sprint."), fpsController.sprintKey);
        fpsController.sprintSpeed = EditorGUILayout.Slider(new GUIContent("Sprint Speed", "Determines how fast the player will move while sprinting."), fpsController.sprintSpeed, fpsController.walkSpeed, 20f);

        fpsController.sprintFOV = EditorGUILayout.Slider(new GUIContent("Sprint FOV", "Determines the field of view the camera changes to while sprinting."), fpsController.sprintFOV, fpsController.fov, 179f);
        fpsController.sprintFOVStepTime = EditorGUILayout.Slider(new GUIContent("Step Time", "Determines how fast the FOV transitions while sprinting."), fpsController.sprintFOVStepTime, .1f, 20f);

        GUI.enabled = true;

        EditorGUILayout.Space();

        #endregion

        #region Jump

        GUILayout.Label("Jump", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

        fpsController.enableJump = EditorGUILayout.ToggleLeft(new GUIContent("Enable Jump", "Determines if the player is allowed to jump."), fpsController.enableJump);

        GUI.enabled = fpsController.enableJump;
        fpsController.jumpKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Jump Key", "Determines what key is used to jump."), fpsController.jumpKey);
        fpsController.jumpPower = EditorGUILayout.Slider(new GUIContent("Jump Power", "Determines how high the player will jump."), fpsController.jumpPower, .1f, 20f);
        GUI.enabled = true;

        EditorGUILayout.Space();

        #endregion

        #region Crouch

        GUILayout.Label("Crouch", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

        fpsController.enableCrouch = EditorGUILayout.ToggleLeft(new GUIContent("Enable Crouch", "Determines if the player is allowed to crouch."), fpsController.enableCrouch);

        GUI.enabled = fpsController.enableCrouch;
        fpsController.holdToCrouch = EditorGUILayout.ToggleLeft(new GUIContent("Hold To Crouch", "Requires the player to hold the crouch key instead if pressing to crouch and uncrouch."), fpsController.holdToCrouch);
        fpsController.crouchKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Crouch Key", "Determines what key is used to crouch."), fpsController.crouchKey);
        fpsController.crouchHeight = EditorGUILayout.Slider(new GUIContent("Crouch Height", "Determines the y scale of the player object when crouched."), fpsController.crouchHeight, .1f, 1);
        fpsController.speedReduction = EditorGUILayout.Slider(new GUIContent("Speed Reduction", "Determines the percent 'Walk Speed' is reduced by. 1 being no reduction, and .5 being half."), fpsController.speedReduction, .1f, 1);
        EditorGUILayout.Space();
        GUI.enabled = true;

        #endregion

        #endregion

        #region Head Bobbing

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Head Bob Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        fpsController.enableHeadBob = EditorGUILayout.ToggleLeft(new GUIContent("Enable Head Bob", "Determines if the camera will bob while the player is walking."), fpsController.enableHeadBob);


        GUI.enabled = fpsController.enableHeadBob;
        fpsController.joint = (Transform)EditorGUILayout.ObjectField(new GUIContent("Camera Joint", "Joint object position is moved while head bob is active."), fpsController.joint, typeof(Transform), true);
        fpsController.bobSpeed = EditorGUILayout.Slider(new GUIContent("Speed", "Determines how often a bob rotation is completed."), fpsController.bobSpeed, 1, 20);
        fpsController.bobAmount = EditorGUILayout.Vector3Field(new GUIContent("Bob Amount", "Determines the amount the joint moves in both directions on every axes."), fpsController.bobAmount);
        GUI.enabled = true;

        #endregion

        #region Weapon Manager

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Weapon Manager", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        SerializedProperty guiArray = serializedController.FindProperty("items");

        EditorGUI.indentLevel++;

        guiArray.isExpanded = EditorGUILayout.Foldout(guiArray.isExpanded, "Weapon Loadouts");

        if (guiArray.isExpanded)
        {
            EditorGUI.indentLevel++;

            guiArray.arraySize = EditorGUILayout.IntField("Loadout Size", guiArray.arraySize);

            for (int i = 0; i < guiArray.arraySize; ++i)
            {
                SerializedProperty arrayElement = guiArray.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(arrayElement, new GUIContent("Weapon " + (i + 1)));
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUI.enabled = true;

        /* --------------------------------------------------

            for (int i = 0; i < size; i++)
            {
                guiArray.arraySize++;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(guiArray, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
            EditorGUIUtility.LookLikeControls();

        -------------------------------------------------- */

        #endregion

        #region GUI Changes

        if (GUI.changed)
        {
            EditorUtility.SetDirty(fpsController);
            Undo.RecordObject(fpsController, "First Person Controller Changes");
            serializedController.ApplyModifiedProperties();
        }

        #endregion
    }

    #endregion
}

#endif

#endregion

