using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public InputSystem_Actions inputs;
    private CharacterController controller;
    public CinemachineImpulseSource impulseSource;

    public float moveSpeed = 5f;
    public float rotationSpeed = 200f;
    public float verticalVelocity = 0;
    public float jumpForce = 10;
    public float pushForce = 4;

    private bool IsDashing;

    public float dashForce;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.5f;
    private float dashTimer;
    private bool canDash = true;

    public float health = 100f;
    private bool isDead = false;

    public float rayLength = 1f;
    public float wallJumpUpForce = 8f;
    public float wallJumpSideForce = 12f;
    public float wallJumpCooldownTime = 0.5f;
    private bool canWallJump = true;
    private bool isTouchingWall;
    private Vector3 wallNormal;

    [SerializeField]private Vector2 moveInput;


    private void Awake()
    {
        inputs = new();
        controller = GetComponent<CharacterController>();
        if (impulseSource == null) impulseSource = GetComponent<CinemachineImpulseSource>();
    }
    private void OnEnable()
    {
        inputs.Enable();

        inputs.Player.Move.performed += ctx =>  moveInput = ctx.ReadValue<Vector2>();
        inputs.Player.Move.canceled += ctx => moveInput = Vector2.zero;


        inputs.Player.Jump.performed += OnJump;

        inputs.Player.Sprint.performed += OnDash;

        

    }
    void Update()
    {
        if (isDead)
        {
            return;
        }

        CheckWallStatus();
        OnMove();
    }

    public void OnMove()
    {
        transform.Rotate(Vector3.up * moveInput.x * rotationSpeed * Time.deltaTime);
        Vector3 moveDir = transform.forward * moveSpeed * moveInput.y;

        verticalVelocity += Physics.gravity.y * Time.deltaTime;

        if(controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;


        moveDir.y = verticalVelocity;

        if(IsDashing)
        {
            //->convertir el dash a un barrido por el piso! dash con gravedad integrada omaegoto!
            moveDir = transform.forward * dashForce * (dashTimer/dashDuration) ;

            dashTimer -= Time.deltaTime;

            if(dashTimer <= 0)
                IsDashing = false;
        }
        controller.Move(moveDir * Time.deltaTime);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isDead) return;

        if (controller.isGrounded)
        {
            verticalVelocity = jumpForce;
            impulseSource.GenerateImpulse(Vector3.up * 0.2f);
        }
        else if (isTouchingWall && canWallJump)
        {
            StartCoroutine(WallJumpCoroutine());
        }

    }

    private IEnumerator WallJumpCoroutine()
    {
        canWallJump = false;

        verticalVelocity = wallJumpUpForce;
        Vector3 jumpForceVec = wallNormal * wallJumpSideForce;
        controller.Move(jumpForceVec * Time.deltaTime);

        impulseSource.GenerateImpulse(wallNormal * 0.3f);
        yield return new WaitForSeconds(wallJumpCooldownTime);
        canWallJump = true;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;

        impulseSource.GenerateImpulse(Random.insideUnitSphere * 0.6f);

        if (health <= 0) Die();
    }

    public void OnSimpleMove()
    {
        transform.Rotate(Vector3.up * moveInput.x * rotationSpeed * Time.deltaTime);
        Vector3 moveDir = transform.forward * moveSpeed * moveInput.y ;
        controller.SimpleMove(moveDir);
    }


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
            StartCoroutine(DashCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        canDash = false;
        IsDashing = true;
        dashTimer = dashDuration;

        yield return new WaitForSeconds(dashDuration);
        IsDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void CheckWallStatus()
    {
        RaycastHit hit;

        bool hitRight = Physics.Raycast(transform.position, transform.right, out hit, rayLength);
        bool hitLeft = Physics.Raycast(transform.position, -transform.right, out hit, rayLength);

        if ((hitRight || hitLeft) && hit.collider.CompareTag("Wall"))
        {
            isTouchingWall = true;
            wallNormal = hit.normal;
        }
        else
        {
            isTouchingWall = false;
        }
    }

    private void Die()
    {
        isDead = true;
        health = 0;

        Invoke("RestartScene", 2.0f);
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
