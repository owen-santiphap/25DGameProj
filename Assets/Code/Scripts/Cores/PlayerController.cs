using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform spriteTransform; // The 2D sprite child object, Character
    [SerializeField] private CharacterController controller;
    [SerializeField] private CombatSystem combat;
    [SerializeField] private Camera mainCamera;
    
    private Vector2 _moveInput;
    private Vector3 _velocity;
    private bool _isGrounded;
    private bool _isDashing;
    
    private void Reset()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        combat = GetComponent<CombatSystem>();
    }
    
    private void Update()
    {
        CheckGround();
        ApplyGravity();
        HandleMovement();
        UpdateAnimations();
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }
    
    private void CheckGround()
    {
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // Small downward force to keep grounded if floating
        }
    }
    
    private void ApplyGravity()
    {
        _velocity.y += gravity * Time.deltaTime;
        controller.Move(_velocity * Time.deltaTime);
    }

    public void PerformDash(float distance, float duration)
    {
        if (_isDashing) return;

        Vector3 dashDirection;
        
        if (_moveInput.magnitude >= 0.1f)
        {
            var camForward = mainCamera.transform.forward;
            var camRight = mainCamera.transform.right;
            
            camForward.y = 0;
            camRight.y = 0;
            
            dashDirection = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;
        }
        else
        {
            // Fallback: If standing still, dash in the direction the sprite is facing
            dashDirection = new Vector3(spriteTransform.localScale.x, 0, 0).normalized;
        }
        
        StartCoroutine(DashCoroutine(dashDirection, distance, duration));
    }

    private IEnumerator DashCoroutine(Vector3 dashDirection, float distance, float duration)
    {
        _isDashing = true;
        var startTime = Time.time;
        var dashSpeed = distance / duration;

        while (Time.time < startTime + duration)
        {
            controller.Move(dashDirection * (dashSpeed * Time.deltaTime));
            yield return null; // Wait for the next frame
        }

        _isDashing = false;
    }
    
    private void HandleMovement()
    {
        // Prevent movement during attacking or dashing
        if (combat != null && combat.IsAttacking || _isDashing)
        {
            return;
        }
        
        // Convert input to 3D movement (X and Z axes)
        var moveDirection = new Vector3(_moveInput.x, 0f, _moveInput.y);
        
        if (moveDirection.magnitude >= 0.1f)
        {
            // Move the character
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
            
            // Rotate sprite to face the movement direction
            if (spriteTransform != null)
            {
                // Flip sprite based on the X direction
                if (moveDirection.x != 0)
                {
                    Vector3 scale = spriteTransform.localScale;
                    scale.x = moveDirection.x > 0 ? 1 : -1;
                    spriteTransform.localScale = scale;
                }
            }
            else
            {
                // Rotate entire object if no separate sprite transform
                var targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                var targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    private void UpdateAnimations()
    {
        if (animator == null) return;
        
        // Don't override attack animations!!!
        if (combat != null && combat.IsAttacking)
        {
            animator.SetBool("IsRunning", false);
            return;
        }
        
        // Set running animation based on movement
        var isMoving = _moveInput.magnitude > 0.01f;
        animator.SetBool("IsRunning", isMoving);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
}