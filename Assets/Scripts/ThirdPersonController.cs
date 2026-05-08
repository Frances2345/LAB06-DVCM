using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ThirdPersonController : MonoBehaviour
{
    [Header ("References")]
    public InputSystem_Actions inputs;
    private CharacterController controller;
    public CinemachineCamera characterCamera;
    public Animator animator;

    [Header("Stats")]
    public float health = 100;
    public int currentHealth;
    [SerializeField] private TextMeshProUGUI healthText;
    public bool isDead = false;

    [Header ("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 200f;
    public float verticalVelocity = 0;
    public float jumpForce = 10;
    public float pushForce = 4;

    [Header ("Dash")]
    public bool IsDashing;
    private bool canDash = true;
    public float dashForce;
    public float dashDuration = 0.2f;
    private float dashTimer;
    private float dashCooldown = 8f;

    [Header ("WallRun")]
    public float rayLenght = 1.2f;
    public float cameraTilt = 15;
    private bool isWallRunning;

    public float wallJumpUpForce = 9f;
    public float wallJumpSideForce = 12f;
    public float wallJumpCooldown = 0.4f;
    private bool canWallJump = true;

    private Vector3 wallNormal;
    private Vector3 impactPoint;
    private Vector3 crossResult;
    private Vector3 wallJumpVelocity;

    [Header ("Impulse")]
    [SerializeField] private CinemachineImpulseSource hitSource;
    [SerializeField] private CinemachineImpulseSource source;

    [SerializeField] private Vector2 moveInput;

    private void Awake()
    {
        inputs = new();
        controller = GetComponent<CharacterController>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }
    private void OnEnable()
    {
        inputs.Enable();
        inputs.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputs.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        inputs.Player.Jump.performed += OnJump;
        inputs.Player.Sprint.performed += OnDash;
    }

    private void Start()
    {
        currentHealth = (int)health;
        UpdateHealthUI();
    }

    void Update()
    {
        if (isDead) return;

        CheckWallRun();
        OnMove();
    }

    public void OnMove()
    {
        Vector3 cameraForwardDir = characterCamera.transform.forward;
        cameraForwardDir.y = 0;
        cameraForwardDir.Normalize();


        if(moveInput != Vector2.zero)
        {
            Quaternion targetQuaternion = Quaternion.LookRotation(cameraForwardDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetQuaternion, rotationSpeed * Time.deltaTime);
        }

        Vector3 moveDir;
        if (!isWallRunning)
        {
            moveDir = (cameraForwardDir * moveInput.y + transform.right * moveInput.x) * moveSpeed;
        }
        else
        {
            moveDir = (crossResult * moveInput.y) * moveSpeed;
        }

        if (isWallRunning && canWallJump)
        {
            verticalVelocity = 0;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
            if (controller.isGrounded && verticalVelocity < 0)
                verticalVelocity = -2f;
        }

        moveDir.y = verticalVelocity;
        moveDir += wallJumpVelocity;

        if (IsDashing)
        {
            moveDir = transform.forward * dashForce * (dashTimer / dashDuration);
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0) IsDashing = false;
        }
        controller.Move(moveDir * Time.deltaTime);
        wallJumpVelocity = Vector3.Lerp(wallJumpVelocity, Vector3.zero, Time.deltaTime * 5f);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isWallRunning && canWallJump)
        {
            PerformWallJump();
            return;
        }

        if (controller.isGrounded)
        {
            verticalVelocity = jumpForce;
            if (animator != null) animator.SetTrigger("Jump");
            if (source != null) source.GenerateImpulse();
        }
    }

    private void PerformWallJump()
    {
        canWallJump = false;
        isWallRunning = false;
        verticalVelocity = wallJumpUpForce;

        Vector3 jumpDir = wallNormal + Vector3.up;
        wallJumpVelocity = jumpDir.normalized * wallJumpSideForce;

        if (source != null)
        {
            source.GenerateImpulse();
        }

        StartCoroutine(WallJumpCooldown());
    }

    IEnumerator WallJumpCooldown()
    {
        yield return new WaitForSeconds(wallJumpCooldown);
        canWallJump = true;
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (canDash && !isDead)
        {
            StartCoroutine(DashRoutine());
        }
    }

    IEnumerator DashRoutine()
    {
        canDash = false;
        IsDashing = true;
        dashTimer = dashDuration;

        yield return new WaitForSeconds(dashDuration);
        IsDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void CheckWallRun()
    {
        RaycastHit hit;
        bool hitRight = Physics.Raycast(transform.position, transform.right, out hit, rayLenght);
        bool hitLeft = Physics.Raycast(transform.position, -transform.right, out hit, rayLenght);

        if ((hitRight || hitLeft) && hit.collider.CompareTag("Wall") && !controller.isGrounded)
        {
            isWallRunning = true;
            wallNormal = hit.normal;
            impactPoint = hit.point;

            characterCamera.Lens.Dutch = hitRight ? cameraTilt : -cameraTilt;
            crossResult = Vector3.Cross(wallNormal, transform.up);

            if (Vector3.Dot(crossResult, transform.forward) < 0)
            {
                crossResult *= -1;
            }
        }
        else
        {
            isWallRunning = false;
            characterCamera.Lens.Dutch = 0;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateHealthUI();

        if (hitSource != null) hitSource.GenerateImpulse();

        if (currentHealth <= 0) Die();
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = "HP: " + currentHealth.ToString();
    }

    private void Die()
    {
        isDead = true;
        health = 0;

        inputs.Disable();
        Invoke("RestartLevel", 2f);
    }

    private void RestartLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.rigidbody != null && hit.rigidbody.linearVelocity == Vector3.zero)
        {
            Vector3 pushDir = (hit.transform.position - transform.position).normalized;
            pushDir.y = 0;

            print(hit.gameObject.name);
            hit.rigidbody.AddForce(pushDir * pushForce, ForceMode.Impulse);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.purple;
        Gizmos.DrawRay(transform.position, transform.right * rayLenght);
        Gizmos.color = Color.navyBlue;
        Gizmos.DrawRay(transform.position, -transform.right * rayLenght);
    }
}
