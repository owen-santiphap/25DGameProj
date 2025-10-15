using System;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    [Header("Skill Setup")]
    [SerializeField] private SkillData deflectSkill;
    [SerializeField] private SkillData aimedShotSkill;
    [SerializeField] private SkillData dashSkill;
    
    public SkillData DeflectSkill => deflectSkill;
    
    [Header("UI References")]
    [SerializeField] private UISkillDisplay deflectSkillUI;
    [SerializeField] private UISkillDisplay aimedShotSkillUI;
    [SerializeField] private UISkillDisplay dashSkillUI;
    [SerializeField] private GameObject deflectIconPrefab;
    private GameObject _deflectIconInstance;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform skillCastPoint;
    [SerializeField] private Transform dashContainer;
    [SerializeField] private PlayerController movement;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Camera mainCamera;

    [Header("Aiming")] 
    [SerializeField] private Transform aimOrigin;
    [SerializeField] private GameObject aimIndicatorPrefab;
    [SerializeField] private LayerMask groundLayer;
    private GameObject _aimIndicatorInstance;

    private float _deflectCooldownTimer;
    private float _aimedShotCooldownTimer;
    private float _dashCooldownTimer;
    private HealthSystem _healthSystem;

    // --- State Management ---
    private bool _isDeflecting = false;
    private bool _isAiming = false;
    private float _deflectTimer = 0f;
    private Vector2 _aimInput;

    public bool IsDeflecting => _isDeflecting;
    public bool IsAiming => _isAiming;

    private void Awake()
    {
        _healthSystem = GetComponent<HealthSystem>();
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Start()
    {
        // Initialize UI icons at the start of the game
        if (deflectSkillUI != null && deflectSkill.icon != null)
        {
            deflectSkillUI.SetIcon(deflectSkill.icon);
        }
        if (aimedShotSkillUI != null && aimedShotSkill.icon != null)
        {
            aimedShotSkillUI.SetIcon(aimedShotSkill.icon);
        }

        if (dashSkillUI != null && dashSkill.icon != null)
        {
            dashSkillUI.SetIcon(dashSkill.icon);
        }
    }

    private void Update()
    {
        UpdateCooldowns();

        // Handle the duration of the Deflect skill
        if (_isDeflecting)
        {
            _deflectTimer += Time.deltaTime;
            
            // End deflect according to timer
            if (_deflectTimer >= deflectSkill.deflectDuration)
            {
                EndDeflect();
            }
        }

        // Handle aiming direction
        if (_isAiming)
        {
            HandleAiming();
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && !_isAiming && !_isDeflecting && _dashCooldownTimer <= 0)
        {
            StartDash();
        }
    }
    
    public void OnDeflect(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_isDeflecting || _isAiming) return; // Already busy
            if (_deflectCooldownTimer > 0)
            {
                Debug.Log($"Cannot Deflect. Cooldown remaining: {_deflectCooldownTimer:F1}s");
                return;
            }
            StartDeflect();
        }
    }
    
    public void OnAimedShot(InputAction.CallbackContext context)
    {
        if (context.started && !_isDeflecting && !_isAiming && _aimedShotCooldownTimer <= 0)
        {
            StartAiming();
        }
        
        if (context.canceled && _isAiming)
        {
            FireAimedProjectile(); // Fire the shot instead of just ending the aim
        }
    }

    // Bound to Mouse Position / Right Stick
    public void OnAim(InputAction.CallbackContext context)
    {
        _aimInput = context.ReadValue<Vector2>();
    }   

    // --- SKILL IMPLEMENTATION ---

    private void StartDash()
    {
        _dashCooldownTimer = dashSkill.cooldownTime;
        animator.SetTrigger("Dash"); 

        if (dashSkill.effectPrefab != null)
        {
            var spawnPosition = transform.position + new Vector3(0, 0.1f, 0);
            
            var vfxInstance = Instantiate(dashSkill.effectPrefab, spawnPosition, Quaternion.identity);

            // Check which way the player sprite is facing.
            if (dashContainer != null && dashContainer.localScale.x < 0)
            {
                // If facing left, flip the VFX's scale on the x-axis.
                vfxInstance.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            // No 'else' is needed, as the prefab's default scale (1, 1, 1) is correct for facing right.
        }
    
        movement.PerformDash(dashSkill.dashDistance, dashSkill.dashDuration);
    }

    private void StartDeflect()
    {
        _isDeflecting = true;
        _deflectTimer = 0f;
        _deflectCooldownTimer = deflectSkill.cooldownTime;
        animator.SetBool("IsRunning", false);

        movement.enabled = false; // This correctly stops the player from moving
        _healthSystem.IsDeflecting = true;
        animator.Play(deflectSkill.animationName, 0, 0f);

        // --- Spawn the icon above the player ---
        if (deflectIconPrefab != null)
        {
            // Position it 2 units above the player's pivot point
            var iconPosition = transform.position + Vector3.up * 2f;
            _deflectIconInstance = Instantiate(deflectIconPrefab, iconPosition, Quaternion.identity, transform);
        }
    }

    private void EndDeflect()
    {
        _isDeflecting = false;
        _healthSystem.IsDeflecting = false;
        movement.enabled = true; // This correctly allows the player to move again
        animator.Play("Idle");

        // --- Destroy the icon ---
        if (_deflectIconInstance != null)
        {
            Destroy(_deflectIconInstance);
        }
    }

    private void StartAiming()
    {
        _isAiming = true;
        _aimedShotCooldownTimer = aimedShotSkill.cooldownTime;
        movement.enabled = false;
        animator.SetBool("IsRunning", false);
        animator.Play("Idle");
        if (aimIndicatorPrefab != null)
        {
            _aimIndicatorInstance = Instantiate(aimIndicatorPrefab, aimOrigin.position, Quaternion.identity, transform);
        }
    }

    private void EndAiming()
    {
        _isAiming = false;
        movement.enabled = true;
        if (_aimIndicatorInstance != null)
        {
            Destroy(_aimIndicatorInstance);
        }
        animator.Play("Idle");
    }

    private void FireAimedProjectile()
    {
        animator.Play(aimedShotSkill.animationName, 0, 0f);

        if (aimedShotSkill.effectPrefab != null)
        {
            var spawnPosition = skillCastPoint.position;
            var spawnRotation = _aimIndicatorInstance != null ? _aimIndicatorInstance.transform.rotation : transform.rotation;
            var projectile = Instantiate(aimedShotSkill.effectPrefab, spawnPosition, spawnRotation);

            var projScript = projectile.GetComponent<SkillProjectile>();
            if (projScript != null)
            {
                projScript.Initialize();
            }
        }

        EndAiming(); // This will clean up the indicator and re-enable movement
    }

    private void HandleAiming()
    {
        if (_aimIndicatorInstance == null) return;
        var direction = Vector3.zero;

        if (playerInput.currentControlScheme == "Gamepad")
        {
            direction = new Vector3(_aimInput.x, 0, _aimInput.y).normalized;
        }
        else // Keyboard & Mouse
        {
            var ray = mainCamera.ScreenPointToRay(_aimInput);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, groundLayer))
            {
                var targetPoint = hitInfo.point;
                direction = (targetPoint - transform.position).normalized;
                direction.y = 0;
            }
        }

        if (direction.sqrMagnitude > 0.1f)
        {
            _aimIndicatorInstance.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }

    private void UpdateCooldowns()
    {
        if (_dashCooldownTimer > 0)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }
        if (_deflectCooldownTimer > 0)
        {
            _deflectCooldownTimer -= Time.deltaTime;
        }
        if (_aimedShotCooldownTimer > 0)
        {
            _aimedShotCooldownTimer -= Time.deltaTime;
        }

        // Update the UI displays
        if (dashSkillUI != null)
        {
            dashSkillUI.UpdateCooldown(_dashCooldownTimer, dashSkill.cooldownTime);
        }
        if (deflectSkillUI != null)
        {
            deflectSkillUI.UpdateCooldown(_deflectCooldownTimer, deflectSkill.cooldownTime);
        }
        if (aimedShotSkillUI != null)
        {
            aimedShotSkillUI.UpdateCooldown(_aimedShotCooldownTimer, aimedShotSkill.cooldownTime);
        }
    }
}