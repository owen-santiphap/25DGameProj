using System;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    [Header("Skill Setup")]
    [SerializeField] private SkillData deflectSkill;
    [SerializeField] private SkillData aimedShotSkill;
    
    public SkillData DeflectSkill => deflectSkill;
    
    [Header("UI References")]
    [SerializeField] private UISkillDisplay deflectSkillUI;
    [SerializeField] private UISkillDisplay aimedShotSkillUI;
    [SerializeField] private GameObject deflectIconPrefab;
    private GameObject _deflectIconInstance;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform skillCastPoint;
    [SerializeField] private PlayerController movement;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Camera mainCamera;

    [Header("Aiming")]
    [SerializeField] private GameObject aimIndicatorPrefab;
    [SerializeField] private LayerMask groundLayer;
    private GameObject _aimIndicatorInstance;

    private float _deflectCooldownTimer;
    private float _aimedShotCooldownTimer;
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
    }

    private void Update()
    {
        UpdateCooldowns();

        // Handle the duration of the Deflect skill
        if (_isDeflecting)
        {
            _deflectTimer += Time.deltaTime;
            // --- CHANGE IS HERE ---
            // Use the new deflectDuration field instead of animationDuration
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
    

    // Bound to Q / L1 for a single press
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

    // Bound to E / R1 for hold and release
    public void OnAimedShot(InputAction.CallbackContext context)
    {
        // When the button is first pressed, start aiming
        if (context.started && !_isDeflecting && !_isAiming && _aimedShotCooldownTimer <= 0)
        {
            StartAiming();
        }
        
        // When the button is released, fire the projectile
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
            _aimIndicatorInstance = Instantiate(aimIndicatorPrefab, transform.position, Quaternion.identity, transform);
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
        if (_deflectCooldownTimer > 0)
        {
            _deflectCooldownTimer -= Time.deltaTime;
        }
        if (_aimedShotCooldownTimer > 0)
        {
            _aimedShotCooldownTimer -= Time.deltaTime;
        }

        // Update the UI displays
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