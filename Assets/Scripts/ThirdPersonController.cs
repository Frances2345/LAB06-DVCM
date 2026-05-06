using Sirenix.OdinInspector;
using System.Collections;
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
    [SerializeField] private CinemachineImpulseSource impulseSource;

    [Header("Stats")]
    public float health = 100;
    public bool isDead;

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
    public float wallJumpUpForce = 9f;
    public float wallJumpSideForce = 12f;
    public float wallJumpCooldown = 0.4f;
    private bool canWallJump = true;
    private bool isWallRunning;
    private Vector3 wallNormal;
    private Vector3 crossResult;

    [SerializeField] private Vector2 moveInput;

    private void Awake()
    {
        inputs = new();
        controller = GetComponent<CharacterController>();

        if (impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }

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

        Vector3 moveDir = (cameraForwardDir * moveInput.y + transform.right * moveInput.x) * moveSpeed;

        if (isWallRunning)
        {
            verticalVelocity = 0;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;

            if (controller.isGrounded && verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }
        }

        moveDir.y = verticalVelocity;

        if (IsDashing)
        {
            moveDir = transform.forward * dashForce * (dashTimer / dashDuration);

            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
                IsDashing = false;
        }
        controller.Move(moveDir * Time.deltaTime);

    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if(isDead) return;

        if (controller.isGrounded)
        {
            verticalVelocity = jumpForce;
            animator.SetTrigger("Jump");

            impulseSource.GenerateImpulse(Vector3.up * 0.2f);
        }
        else if (isWallRunning && canWallJump)
        {
            StartCoroutine(WallJumpRoutine());
        }
    }

    IEnumerator WallJumpRoutine()
    {
        canWallJump = false;
        isWallRunning = false;
        verticalVelocity = wallJumpUpForce;

        Vector3 jumpForceVector = wallNormal * wallJumpSideForce;
        controller.Move(jumpForceVector * Time.deltaTime);

        impulseSource.GenerateImpulse(wallNormal * 0.5f);

        yield return new WaitForSeconds(wallJumpCooldown);
        canWallJump = true;
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
        }
        else
        {
            isWallRunning = false;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;

        impulseSource.GenerateImpulse(Random.insideUnitSphere * 0.8f);

        if (health <= 0) Die();
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
        Vector3 pushDir = (hit.transform.position - transform.position).normalized;

        if (hit.rigidbody != null && hit.rigidbody.linearVelocity == Vector3.zero)
        {
            print(hit.gameObject.name);
            hit.rigidbody.AddForce(pushDir * pushForce, ForceMode.Impulse);
        }
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (canDash && !isDead)
        {
            StartCoroutine(PerformDash());
        }
    }

    IEnumerator PerformDash()
    {
        canDash = false;
        IsDashing = true;
        dashTimer = dashDuration;

        yield return new WaitForSeconds(dashDuration);
        IsDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.purple;
        Gizmos.DrawRay(transform.position, transform.right * rayLenght);
        Gizmos.color = Color.navyBlue;
        Gizmos.DrawRay(transform.position, -transform.right * rayLenght);
    }
}
