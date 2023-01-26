using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class FPS_Controller : MonoBehaviour
{
    [SerializeField] LayerMask WallLayer;


    [Header("Components")]
    [SerializeField] CharacterController controller;
    [SerializeField] Camera cam;
    [SerializeField] Slider staminaSlider;

    [Header("wasd inputs")]
    [SerializeField] float xInput;
    [SerializeField] float zInput;
    [SerializeField] float velocity;
    [SerializeField] Vector3 movementVec;

    [Header("Mouse inputs")]
    [SerializeField] float xMouse;
    [SerializeField] float yMouse;

    [Header("Other inputs")]
    [SerializeField] bool isRunning;
    [SerializeField] bool isInterrupted;
    [SerializeField] bool isCrouching;

    [Header("Stats")]
    [SerializeField] Stats playerStats;
    [SerializeField] float gravityScale;
    [SerializeField] float dashTime;
    [SerializeField] float dashDistance;
    [SerializeField] float staminaRegenRate;
    float maxStamina;
    float currentStamina;
    float timeSinceRun;
    float timeSinceDash;
    bool doingStaminaRegen;

    [Header("GroundCheck (ChildTransform needed!)")]
    [SerializeField] Transform groundCheckPos;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] bool isGround;

    [Header("Camera")]
    [SerializeField] float sensitivity;
    [SerializeField] float targetXRotation;
    [SerializeField] float targetYRotation;
    


    // Start is called before the first frame update
    void Start()
    {
        maxStamina = playerStats.stamina;
        currentStamina = maxStamina;
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInterrupted)
        {
            HandleInput();
            HandleJump();
            

           
        }
        HandleCrouch();
        HandleCamera();
        StaminaConsumption();

        staminaSlider.value = currentStamina / maxStamina;
    }

    private void FixedUpdate()
    {
        if (!isInterrupted)
        {
            HandleMovemenvt();
        }
        
    }

    void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        zInput = Input.GetAxisRaw("Vertical");
        isRunning = Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) && currentStamina > 0;

        



        if (Input.GetKeyDown(KeyCode.C) && isRunning && !isCrouching)
        {
            isInterrupted = true;
            StartCoroutine(Slide(movementVec));

        }

        if (!isInterrupted)
        {
            if (!isCrouching && Input.GetKeyDown(KeyCode.C))
                isCrouching = true;
            else if (isCrouching && Input.GetKeyDown(KeyCode.C))
                isCrouching = false;
        }


        timeSinceDash += Time.deltaTime;

            if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.D) && isGrounded() && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && timeSinceDash > 0.25f && (currentStamina - 25) > 0)
            StartCoroutine(Dash(transform.right));
        else if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.A) && isGrounded() && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && timeSinceDash > 0.25f && (currentStamina - 25) > 0)
            StartCoroutine(Dash(-transform.right));
        else if(Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.S) && isGrounded() && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && timeSinceDash > 0.25f && (currentStamina-25) > 0)
            StartCoroutine(Dash(-transform.forward));


    }

    void HandleMovemenvt()
    {
        
        movementVec = transform.forward * zInput + transform.right * xInput;
        movementVec.Normalize();

        if (!isCrouching)
            movementVec *= !isRunning ? playerStats.speed : playerStats.sprintSpeed;
        else
            movementVec *= playerStats.speed * 0.5f;

        movementVec += Vector3.up * velocity;
        controller.Move(movementVec * Time.fixedDeltaTime);
        isGround = isGrounded();
        if (!isGround)
            velocity += gravityScale * Time.fixedDeltaTime;
        else
            velocity = -2f;

        
    }
    void HandleJump()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.right, out hit, 0.7f, WallLayer) && Input.GetKey(KeyCode.Space) && isRunning)
        {
            StartCoroutine(Wallrun(hit.normal, true, Mathf.Sqrt(playerStats.jumpHeight * -2f * (gravityScale*0.25f))));
            return;
        } 
        else if (Physics.Raycast(transform.position, -transform.right, out hit, 0.7f, WallLayer) && Input.GetKey(KeyCode.Space) && isRunning)
        {
            StartCoroutine(Wallrun(-hit.normal, false, Mathf.Sqrt(playerStats.jumpHeight * -2f * (gravityScale * 0.25f))));
            return;
        }
            

        if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.D) && isGrounded() && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
            return;
        else if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.A) && isGrounded() && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
            return;
        else if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.S) && isGrounded() && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
            return;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded())
        {
            velocity = Mathf.Sqrt(playerStats.jumpHeight * -2f * gravityScale);
        }
    }

    void HandleCrouch()
    {
        if (isCrouching)
        {
            Vector3 camPos = new Vector3(0, 0.0f, 0);
            Vector3 refVec = Vector3.zero;
            cam.transform.localPosition = Vector3.SmoothDamp(cam.transform.localPosition, camPos, ref refVec, 0.015f);
            controller.height = 1.4f;
            controller.center = new Vector3(0, -0.3f, 0);
        }
        else
        {
            Vector3 camPos = new Vector3(0, 0.5f, 0);
            Vector3 refVec = Vector3.zero;
            cam.transform.localPosition = Vector3.SmoothDamp(cam.transform.localPosition, camPos, ref refVec, 0.015f);
            controller.height = 1.8f;
            controller.center = new Vector3(0, -0.1f, 0);
        }
    }

    void HandleCamera()
    {
        targetXRotation -= Input.GetAxisRaw("Mouse Y") * sensitivity;
        targetYRotation += Input.GetAxisRaw("Mouse X") * sensitivity;

        targetXRotation = Mathf.Clamp(targetXRotation, -90, 90);
        cam.transform.localEulerAngles = new Vector3(targetXRotation, 0, 0);
        
        transform.eulerAngles = new Vector3(0, targetYRotation, 0);
    }

    void StaminaConsumption()
    {
        if(isRunning && currentStamina > 0)
        {
            if(currentStamina <= 0)
            {
                currentStamina = 0;
                return;
            }
            timeSinceRun = 0;
            doingStaminaRegen = false;
            currentStamina -= Time.deltaTime * 30f;
        }
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        if (!isRunning)
            timeSinceRun += Time.deltaTime;

        if(timeSinceRun >= 2 && !doingStaminaRegen)
        {
            StartCoroutine(StaminaRegen());
            doingStaminaRegen = true;
            
        }
            
    }

    bool isGrounded()
    {
        return Physics.CheckBox(groundCheckPos.position, new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity, groundLayer);
    }

    IEnumerator Dash(Vector3 dir)
    {
        isInterrupted = true;

        timeSinceRun = 0;
        doingStaminaRegen = false;

        float originStamina = currentStamina;
        float targetStamina = (originStamina - 25f);

        Vector3 originPos = transform.position;
        Vector3 targetPos = originPos + dir * dashDistance;

        float elapsedTime = 0;
        while (elapsedTime < dashTime)
        {
            velocity = -2f;
            float t = elapsedTime / dashTime;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(originPos, targetPos, t);
            currentStamina = Mathf.Lerp(originStamina, targetStamina,t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
        currentStamina = targetStamina;
        timeSinceDash = 0;
        isInterrupted = false;
    }

    IEnumerator Slide(Vector3 dir)
    {


        isInterrupted = true;
        isCrouching = true;

        
        Vector3 direction = new Vector3(dir.x, 0, dir.z);
        direction.Normalize();

        Vector3 originPos = transform.position;
        Vector3 targetPos = originPos + direction * dashDistance;

        float elapsedTime = 0;
        while (elapsedTime < dashTime * 2)
        {
            velocity = -2f;
            float t = elapsedTime / (dashTime * 2);
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(originPos, targetPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        isInterrupted = false;
    }

    IEnumerator StaminaRegen()
    {
        
        float originStamina = currentStamina;
        float targetStamina = maxStamina;



        float timeToRegen = (targetStamina-originStamina)/staminaRegenRate;
        float timeElapsed = 0;

        

        while (timeElapsed < timeToRegen)
        {
            float t = timeElapsed / timeToRegen;
            t = t * t * (3f - 2f * t);

            if (isRunning || isInterrupted)
            {
                doingStaminaRegen = false;
                yield break;
                
            }

            currentStamina = Mathf.Lerp(originStamina, targetStamina, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        currentStamina = targetStamina;
        doingStaminaRegen = false;
        
    }

    IEnumerator Wallrun(Vector3 dir, bool isRight,float yVelocity)
    {
        isInterrupted = true;
        Vector3 targetDir = Vector3.Cross(transform.up, dir);
        if (isRight)
        {
            
            while ((Physics.Raycast(transform.position, transform.right, 0.6f, WallLayer)))
            {
                targetDir = Vector3.Cross(transform.up, dir);
                movementVec.Normalize();
                if (Input.GetKeyUp(KeyCode.Space) || currentStamina <= 0)
                {
                    isInterrupted = false;
                    yield break;
                }

                targetDir *= playerStats.speed;

                targetDir += Vector3.up * yVelocity;
                controller.Move(targetDir * Time.deltaTime);
                velocity = 0;
                yVelocity += (gravityScale*0.25f) * Time.deltaTime;
                yield return null;
            }
            isInterrupted = false;
        }
        else
        {
            while ((Physics.Raycast(transform.position, -transform.right, 0.6f, WallLayer)))
            {
                targetDir = Vector3.Cross(transform.up, dir);
                movementVec.Normalize();
                if (Input.GetKeyUp(KeyCode.Space) || currentStamina <= 0)
                {
                    isInterrupted = false;
                    yield break;
                }

                targetDir *= playerStats.speed;

                targetDir += Vector3.up * yVelocity;
                controller.Move(targetDir * Time.deltaTime);
                velocity = 0;
                yVelocity += (gravityScale * 0.25f) * Time.deltaTime;
                yield return null;
            }
            isInterrupted = false;
        }
        
        
    }
}


