using System;
using System.Numerics;
using KinematicCharacterController;
using MonkeyBusiness.Misc;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Sirenix.OdinInspector;

namespace MonkeyBusiness.Player
{
    public enum CrouchInput
    {
        None, Toggle, Crouch, Uncrouch
    }

    public enum Stance
    {
        Stand, Crouch, Slide, Swing
    }

    public struct CharacterState
    {
        public bool Grounded;
        public Stance Stance;
        public Vector3 Velocity;
        public Vector3 Acceleration;
    }

    public struct CharacterInput
    {
        public Quaternion Rotation;
        public Vector2 Move;
        public bool Swing;
        public bool Jump;
        public bool JumpSustain;
        public CrouchInput Crouch;
    }

    public class PlayerCharacter : MonoBehaviour, ICharacterController, ITargetable
    {
        [SerializeField] private KinematicCharacterMotor motor;
        [SerializeField] private Transform root;
        [SerializeField] private Transform cameraTarget;

        [SerializeField] private LayerMask whatIsSwingable;
        
        [field:SerializeField] public float WalkSpeed { get; set; } = 20f;
        [SerializeField] private float crouchSpeed = 7f;
        [SerializeField] private float walkResponse = 25f;
        [SerializeField] private float crouchResponse = 20f;
        [Space]
        [SerializeField] private float airSpeed = 15f;
        [SerializeField] private float airAcceleration = 70f;
        [Space]
        [SerializeField] private float jumpSpeed = 20f;
        [SerializeField] private float coyoteTime = 0.2f;
        [Range(0f, 1f)]
        [SerializeField] private float jumpSustainGravity = 0.4f;
        [SerializeField] private float gravity = -90f;
        [Space] 
        [SerializeField] private float slideStartSpeed = 25f;
        [SerializeField] private float slideEndSpeed = 15f;
        [SerializeField] private float slideFriction = 0.8f;
        [SerializeField] private float slideSteerAcceleration = 5f;
        [SerializeField] private float slideGravity = -90f;
        [Space]
        [SerializeField] private float standHeight = 2f;
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float crouchHeightResponse = 15f;
        [Range(0f, 1f)]
        [SerializeField] private float cameraStandHeight = .9f;
        [Range(0f, 1f)]
        [SerializeField] private float cameraCrouchHeight = .7f;

        [field: SerializeField]
        [field: Tooltip("Object that should be targeted by attacks.")]
        [field: Required]
        public GameObject Target { get; private set; }

        public bool canUseRope { get; set; } = false;
        [SerializeField] private float swingForce = 30f;
        [SerializeField] private float swingSpring = 4.5f;
        [SerializeField] private float swingDamping = 7f;
        [SerializeField] private float swingMassScale = 4.5f;

        private Vector3 _swingVelocity;
        
        private CharacterState _state;    
        private CharacterState _lastState;    
        private CharacterState _tempState;    
        
        private Quaternion _requestedRotation;
        private Vector3 _requestedMovement;
        private bool _requestedJump;
        private bool _requestedSustainedJump;
        private bool _requestedCrouch;
        private bool _requestedCrouchInAir;

        private float _timeSinceUngrounded;
        private float _timeSinceJumpRequest;
        private bool _ungroundedDueToJump;
        
        private SpringJoint _swingJoint;
        private Rigidbody _rb;
        private LineRenderer _lineRenderer;
        private Vector3 _ropeEnd;

        private Collider[] _uncrouchOverlapResults;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        public void Initialize()
        {
            _state.Stance = Stance.Stand;
            _lastState = _state;
            
            _uncrouchOverlapResults = new Collider[8];
            
            motor.CharacterController = this;
        }

        public void UpdateInput(CharacterInput input)
        {
            if (input.Swing && _state.Stance != Stance.Swing) StartSwing();
            if (!input.Swing && _state.Stance == Stance.Swing) StopSwing();
            
            _requestedRotation = input.Rotation;    
            // Take the 2D input vector and create a 3D movement vector on the XZ plane
            _requestedMovement = new Vector3(input.Move.x, 0, input.Move.y);
            // Clamp the length to 1 to prevent moving faster on diagonals
            _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);
            // Orient the input so it's relative to the direction the player is facing
            _requestedMovement = input.Rotation * _requestedMovement;
            
            var wasRequestingJump = _requestedJump;
            _requestedJump = _requestedJump || input.Jump;
            if(_requestedJump && !wasRequestingJump)
                _timeSinceJumpRequest = 0f;
            
            _requestedSustainedJump = input.JumpSustain;
            
            var wasRequestingCrouch = _requestedCrouch;
            _requestedCrouch = input.Crouch switch
            {
                CrouchInput.Crouch => true,
                CrouchInput.Uncrouch => false,
                CrouchInput.Toggle => !_requestedCrouch,
                _ => _requestedCrouch
            };
            if (_requestedCrouch && !wasRequestingCrouch)
                _requestedCrouchInAir = !_state.Grounded;
            else if (!_requestedCrouch && wasRequestingCrouch)
                _requestedCrouchInAir = false;
        }

        void StartSwing()
        {
            if (!canUseRope) return;
            
            var cam = UnityEngine.Camera.main;
            var origin = cam.transform.position;
            var dir = cam.transform.forward;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, 100f, whatIsSwingable))
            {
                _lineRenderer.enabled = true;
                _state.Stance = Stance.Swing;

                _rb = GetComponent<Rigidbody>();
                if (_rb != null)
                {
                    _rb.freezeRotation = true;
                    _rb.linearVelocity = motor.Velocity; // transfer velocity into swing
                }

                if (motor != null)
                {
                    motor.enabled = false;
                }

                _swingJoint = gameObject.AddComponent<SpringJoint>();
                _swingJoint.autoConfigureConnectedAnchor = false;
                _swingJoint.connectedAnchor = hit.point;

                float distance = Vector3.Distance(transform.position, hit.point);

                _swingJoint.maxDistance = distance * 0.6f;
                _swingJoint.minDistance = distance * 0.5f;

                _swingJoint.spring = swingSpring;
                _swingJoint.damper = swingDamping;
                _swingJoint.massScale = swingMassScale;
                
                _ropeEnd = hit.point;;
            }
        }

        void StopSwing()
        {
            _state.Stance = Stance.Stand;

            Vector3 currentPos = transform.position;
            Vector3 exitVelocity = Vector3.zero;

            if (_rb != null)
            {
                exitVelocity = _rb.linearVelocity;
            }

            if (_swingJoint != null)
            {
                Destroy(_swingJoint);
            }

            if (_rb != null)
            {
                _rb.freezeRotation = false;
            }

            if (motor != null)
            {
                motor.enabled = true;
                motor.SetPosition(currentPos);
                motor.BaseVelocity = exitVelocity;
            }

            _lineRenderer.enabled = false;
        }
        
        void FixedUpdate()
        {
            if (_state.Stance != Stance.Swing) return;

            Vector3 inputDir = _requestedMovement;

            if (inputDir.sqrMagnitude > 0f)
            {
                // Project input onto plane perpendicular to world up
                Vector3 swingDir = Vector3.ProjectOnPlane(inputDir, Vector3.up).normalized;

                // Apply force
                _rb.AddForce(swingDir * swingForce, ForceMode.Acceleration);
            }
        }

        private void LateUpdate()
        {
            _lineRenderer.SetPosition(0,transform.position);
            _lineRenderer.SetPosition(1,_ropeEnd);
        }

        public void UpdateBody(float deltaTime)
        {
            var currentHeight = motor.Capsule.height;
            var normalizedHeight = currentHeight / standHeight; 
            
            var cameraTargetHeight = currentHeight * (
                _state.Stance is Stance.Stand ? cameraStandHeight : cameraCrouchHeight
            );
            var rootTargetScale = new Vector3(1f, normalizedHeight, 1f);
            
            cameraTarget.localPosition = Vector3.Lerp(
                a: cameraTarget.localPosition, 
                b: new Vector3(0f, cameraTargetHeight, 0f), 
                t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
            );
            root.localScale = Vector3.Lerp(
                a: root.localScale, 
                b: rootTargetScale, 
                t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
            );
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            // Update the character's rotation to face in the same direction as the
            // requested rotation (camera rotation)
            
            // we don't want the character to pitch up and down, so the direction the character
            // looks should be always "flattened"
            
            // This is done by projecting a vector pointing in the same direction that
            // the player is looking onto a flat ground plane

            var forward = Vector3.ProjectOnPlane(
                _requestedRotation * Vector3.forward,
                motor.CharacterUp
            );
            
            if (forward != Vector3.zero)
                currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            _state.Acceleration = Vector3.zero;
            
            // If on the ground...
            if (motor.GroundingStatus.IsStableOnGround)
            {
                _ungroundedDueToJump = false;
                _timeSinceUngrounded = 0f;
                
                // Snap the requested movement direction to the angle of the surface
                // the character is currently walking on
                var groundedMovement = motor.GetDirectionTangentToSurface(
                    direction: _requestedMovement,
                    surfaceNormal: motor.GroundingStatus.GroundNormal
                ) * _requestedMovement.magnitude;
                
                // Start sliding
                {
                    var moving = groundedMovement.sqrMagnitude > 0f;
                    var crouching = _state.Stance is Stance.Crouch;
                    var wasStanding = _lastState.Stance is Stance.Stand;
                    var wasInAir = !_lastState.Grounded;
                    if (moving && crouching && (wasStanding || wasInAir))
                    {
                        //Debug.DrawRay(transform.position, currentVelocity, Color.red, 5f);
                        //Debug.DrawRay(transform.position, _lastState.Velocity, Color.green, 5f);
                        
                        _state.Stance = Stance.Slide;
                        
                        // When landing on stable ground the character motor projects the velocity onto a flat ground plane.
                        // See: KinematicCharacterMotor.HandleVelocityProjection()
                        // This is normally good, because under normal circumstances the player shouldn't slide when landing on the ground.
                        // In this case, we *want* the player to slide.
                        // Reproject the last frames (falling) velocity onto the ground normal to slide.
                        if (wasInAir)
                        {
                            currentVelocity = Vector3.ProjectOnPlane
                            (
                                vector: _lastState.Velocity,
                                planeNormal: motor.GroundingStatus.GroundNormal
                            );
                        }

                        var effectiveSlideStartSpeed = slideStartSpeed;
                        if (!_lastState.Grounded && !_requestedCrouchInAir)
                        {
                            effectiveSlideStartSpeed = 0f;
                            _requestedCrouchInAir = false;
                        }
                        
                        var slideSpeed = Mathf.Max(effectiveSlideStartSpeed, currentVelocity.magnitude);
                        currentVelocity = motor.GetDirectionTangentToSurface(
                            direction: currentVelocity,
                            surfaceNormal: motor.GroundingStatus.GroundNormal
                        ) * slideSpeed;
                    }
                }
                // Move
                if (_state.Stance is Stance.Stand or Stance.Crouch){
                    
                    var speed = _state.Stance is Stance.Stand ? WalkSpeed : crouchSpeed;
                    var response = _state.Stance is Stance.Stand ? walkResponse : crouchResponse;
                    
                    // And move along the ground in that direction
                    var targetVelocity = groundedMovement * speed;
                    var moveVelocity = Vector3.Lerp(
                        a: currentVelocity, 
                        b: targetVelocity,
                        t: 1f - Mathf.Exp(-response * deltaTime)
                    );
                    _state.Acceleration = moveVelocity - currentVelocity;
                    currentVelocity = moveVelocity;
                }
                // Continue sliding
                else
                {
                    // Friction
                    currentVelocity -= currentVelocity * (slideFriction * deltaTime);
                    
                    // Slope
                    {
                        var force = Vector3.ProjectOnPlane(
                            vector: -motor.CharacterUp,
                            planeNormal: motor.GroundingStatus.GroundNormal
                        ) * slideGravity;
                        
                        currentVelocity -= force * deltaTime;
                    }
                    
                    // Steer
                    {
                        var currentSpeed = currentVelocity.magnitude;
                        var targetVelocity = groundedMovement * currentSpeed;
                        var steerVelocity = currentVelocity;
                        var steerForce = (targetVelocity - steerVelocity) * slideSteerAcceleration * deltaTime;
                        // Add steer force, but clamp velocity so the slide speed doesn't increase due to direct movement input
                        steerVelocity += steerForce;
                        steerVelocity = Vector3.ClampMagnitude(steerVelocity, currentSpeed);
                        
                        _state.Acceleration = (steerVelocity - currentVelocity) / deltaTime;
                        currentVelocity = steerVelocity;
                    }
                    
                    // Stop
                    if (currentVelocity.magnitude < slideEndSpeed)
                    {
                        _state.Stance = Stance.Crouch;
                    }
                }
            }
            // else in the air...
            else
            {
                _timeSinceUngrounded += deltaTime;
                
                // Move
                if (_requestedMovement.sqrMagnitude > 0f)
                {
                    // Requested movement projected onto movement plane (magnitude preserved)
                    var planarMovement = Vector3.ProjectOnPlane(
                        vector: _requestedMovement,
                        planeNormal: motor.CharacterUp
                    ).normalized * _requestedMovement.magnitude;
                    
                    // Current velocity on movement plane
                    var currentPlanarVelocity = Vector3.ProjectOnPlane(
                        vector:  currentVelocity,
                        planeNormal: motor.CharacterUp
                    );
                    
                    var movementForce = planarMovement * airAcceleration * deltaTime;

                    // If moving slower than the max air speed, treat movementForce as a simple steering force 
                    if (currentPlanarVelocity.magnitude < airSpeed)
                    {
                        // Add it to the current planar velocity for a target velocity
                        var targetPlanarVelocity = currentPlanarVelocity + movementForce;
                        
                        // Limit target velocity to air speed
                        targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);
                        
                        // Steer towards target velocity
                        movementForce = targetPlanarVelocity - currentPlanarVelocity;
                    }
                    // Otherwise, nerf the movement force when it is in the direction of the current planar velocity
                    // to prevent accelerating further beyond the max air speed
                    else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f)
                    {
                        // Project movement force onto the plane whose normal is the current planar velocity
                        var constrainedMovementForce = Vector3.ProjectOnPlane(
                            vector: movementForce,
                            planeNormal: currentPlanarVelocity.normalized
                        );
                        movementForce = constrainedMovementForce;
                    }
                    
                    // Prevent air-climbing steep slopes
                    if (motor.GroundingStatus.FoundAnyGround)
                    {
                        // If moving in the same direction as the resultant velocity
                        if (Vector3.Dot(currentPlanarVelocity, currentVelocity + movementForce) > 0f)
                        {
                            // Calculate obstruction normal
                            var obstructionNormal = Vector3.Cross(
                                motor.CharacterUp,
                                Vector3.Cross(
                                    motor.CharacterUp,
                                    motor.GroundingStatus.GroundNormal
                                )
                            ).normalized;
                            
                            // Project movement force onto obstruction plane
                            movementForce = Vector3.ProjectOnPlane(movementForce, obstructionNormal); 
                        }
                    }
                    
                    currentVelocity += movementForce;
                }
                
                //  Gravity
                var effectiveGravity = gravity;
                var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                if (_requestedSustainedJump && verticalSpeed > 0f)
                {
                    effectiveGravity *= jumpSustainGravity;
                }
                currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;
            }

            if (_requestedJump)
            {
                var grounded = motor.GroundingStatus.IsStableOnGround;
                var canCoyoteJump = _timeSinceUngrounded < coyoteTime && !_ungroundedDueToJump;
                
                if (grounded || canCoyoteJump)
                {
                    _requestedJump = false;     // Unset jump request
                    _requestedCrouch = false;   // and request the character uncrouches
                    _requestedCrouchInAir = false;
                    
                    // unstick the player from the ground
                    motor.ForceUnground(time: 0f);
                    _ungroundedDueToJump = true;
                    
                    // set minimum vertical speed to the jump speed
                    var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                    var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
                    // Add the difference in current and target vertical speed to the character's velocity
                    currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
                }
                else
                {
                    _timeSinceJumpRequest += deltaTime;
                    
                    // Defer the jump request until coyote time has passed 
                    var canJumpLater = _timeSinceJumpRequest < coyoteTime;
                    _requestedJump = canJumpLater;
                }
            }
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            _tempState = _state;
            
        // Crouch
        if (_requestedCrouch && _state.Stance is Stance.Stand)
        {
            _state.Stance = Stance.Crouch;
            motor.SetCapsuleDimensions(
                radius: motor.Capsule.radius,
                height: crouchHeight,
                yOffset: crouchHeight * 0.5f
            );
        }
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            if (!motor.GroundingStatus.IsStableOnGround && _state.Stance is Stance.Slide)
                _state.Stance = Stance.Crouch;
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            // Uncrouch
            if (!_requestedCrouch && _state.Stance is not Stance.Stand)
            {
                _state.Stance = Stance.Stand;
                motor.SetCapsuleDimensions(
                    radius: motor.Capsule.radius,
                    height: standHeight,
                    yOffset: standHeight * 0.5f
                );
                
                // Check for collisions
                if (motor.CharacterOverlap(
                        motor.TransientPosition, 
                        motor.TransientRotation, 
                        _uncrouchOverlapResults, 
                        motor.CollidableLayers,  
                        QueryTriggerInteraction.Ignore) > 0)
                {
                    // re-crouch
                    _requestedCrouch = true;
                    motor.SetCapsuleDimensions(
                        radius: motor.Capsule.radius,
                        height: crouchHeight,
                        yOffset: crouchHeight * 0.5f
                    );
                }
                else
                {
                    _state.Stance = Stance.Stand;
                }
            }
            
            // Update state to reflect relevant motor properties
            _state.Grounded = motor.GroundingStatus.IsStableOnGround;
            _state.Velocity = motor.Velocity;
            _lastState = _tempState;
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
        
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
            
        }
        
        public Transform GetCameraTarget() => cameraTarget;
        public CharacterState GetState() => _state;
        public CharacterState GetLastState() => _lastState;

        public void SetPosition(Vector3 position, bool killVelocity = true)
        {
            motor.SetPosition(position);
            if(killVelocity)
                motor.BaseVelocity = Vector3.zero;
        }

    }
}