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
    
    private Vector2 _moveInput;
    private Vector3 _velocity;
    private bool _isGrounded;
    
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
    
    private void HandleMovement()
    {
        // Don't move during the attack!!
        if (combat != null && combat.IsAttacking)
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
        var isMoving = _moveInput.magnitude > 0.1f;
        animator.SetBool("IsRunning", isMoving);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
}