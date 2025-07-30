using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PieceAnimationController : MonoBehaviour
{
    [Header("Animation Components")]
    public Animator animator;
    public Animation animationComponent;

    [Header("Animation References")]
    public ChessPieceData pieceData;
    public PieceController pieceController;

    [Header("Animation States")]
    public bool isAnimating = false;
    public bool isMoving = false;

    [Header("Animation Curves")]
    public AnimationCurve spawnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve moveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public AnimationCurve deathCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    public AnimationCurve idleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve captureCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Animation Parameters")]
    public float animationSpeed = 1f;

    // Animation types
    public enum PieceAnimationType
    {
        Spawn,
        Idle,
        Move,
        Capturing,
        Captured,
        Death,
        Castle,
        EnPassant
    }

    private void Awake()
    {
        // Try to get components if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();
        
        if (animationComponent == null)
            animationComponent = GetComponent<Animation>();

        // Try to get piece controller and piece data
        if (pieceController == null)
            pieceController = GetComponent<PieceController>();
    }

    private void Update()
    {
        // Update animator boolean for move state
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }
    }

    // Main animation method
    public void PlayAnimation(PieceAnimationType animationType)
    {
        if (isAnimating) return;

        StartCoroutine(PlayAnimationCoroutine(animationType));
    }

    // Method to start moving with effects
    public void StartMove()
    {
        isMoving = true;
        PlayAnimation(PieceAnimationType.Move);


        // Try to play move effect through PieceController
        Debug.Log("Starting move effect");
        if (pieceController != null)
        {
            Debug.Log("Piece move controller: " + pieceController);
            PieceEffectController effectController = pieceController.effectController;
            if (effectController != null)
            {
                Debug.Log("Effect move controller: " + effectController);
                effectController.PlayMoveEffect();
            }
        }
    }

    // Method to stop moving (return to idle)
    public void StopMove()
    {
        isMoving = false;
        PlayAnimation(PieceAnimationType.Idle);

        // Stop move effect through PieceController
        if (pieceController != null)
        {   
            PieceEffectController effectController = pieceController.effectController;
            if (effectController != null)
            {
                effectController.StopMoveEffect();
            }
        }
    }

    // Method to start capturing
    public void StartCapturing()
    {
        PlayAnimation(PieceAnimationType.Capturing);
    }

    // Method when piece is captured
    public void SetCaptured()
    {
        PlayAnimation(PieceAnimationType.Captured);
    }

    // Method to reset to idle after capture
    public void ResetToIdle()
    {
        PlayAnimation(PieceAnimationType.Idle);
    }

    private IEnumerator PlayAnimationCoroutine(PieceAnimationType animationType)
    {
        isAnimating = true;

        // Select appropriate animation based on type
        AnimationClip selectedClip = GetAnimationClip(animationType);
        AnimationCurve selectedCurve = GetAnimationCurve(animationType);

        if (selectedClip != null)
        {
            // Animator-based animation
            if (animator != null)
            {
                PlayAnimatorAnimation(animationType);
            }
            // Legacy Animation component
            else if (animationComponent != null)
            {
                PlayLegacyAnimation(selectedClip, animationType);
            }
            // Fallback to manual animation
            else
            {
                yield return StartCoroutine(PlayManualAnimation(animationType, selectedCurve));
            }
        }
        else
        {
            // Fallback to manual animation if no clip
            yield return StartCoroutine(PlayManualAnimation(animationType, selectedCurve));
        }

        isAnimating = false;
    }

    // Get appropriate animation clip
    private AnimationClip GetAnimationClip(PieceAnimationType animationType)
    {
        if (pieceData == null) return null;

        return animationType switch
        {
            PieceAnimationType.Spawn => pieceData.spawnAnimation,
            PieceAnimationType.Idle => pieceData.idleAnimation,
            PieceAnimationType.Move => pieceData.moveAnimation,
            PieceAnimationType.Capturing => pieceData.capturingAnimation,
            PieceAnimationType.Captured => pieceData.capturedAnimation,
            PieceAnimationType.Death => pieceData.deathAnimation,
            PieceAnimationType.Castle => pieceData.castleAnimation,
            PieceAnimationType.EnPassant => pieceData.enPassantAnimation,
            _ => null
        };
    }

    // Get appropriate animation curve
    private AnimationCurve GetAnimationCurve(PieceAnimationType animationType)
    {
        if (pieceData == null) return AnimationCurve.Linear(0f, 0f, 1f, 1f);

        return animationType switch
        {
            PieceAnimationType.Spawn => pieceData.spawnAnimationCurve ?? spawnCurve,
            PieceAnimationType.Idle => pieceData.idleAnimationCurve ?? idleCurve,
            PieceAnimationType.Move => pieceData.moveAnimationCurve ?? moveCurve,
            PieceAnimationType.Capturing => pieceData.capturingAnimationCurve ?? captureCurve,
            PieceAnimationType.Captured => pieceData.capturedAnimationCurve ?? deathCurve,
            PieceAnimationType.Death => pieceData.deathAnimationCurve ?? deathCurve,
            _ => AnimationCurve.Linear(0f, 0f, 1f, 1f)
        };
    }

    // Play animation through Animator
    private void PlayAnimatorAnimation(PieceAnimationType animationType)
    {
        string triggerName = animationType switch
        {
            PieceAnimationType.Spawn => "Spawn",
            PieceAnimationType.Idle => "Idle",
            PieceAnimationType.Move => "Move",
            PieceAnimationType.Capturing => "Capturing",
            PieceAnimationType.Captured => "Captured",
            PieceAnimationType.Death => "Death",
            PieceAnimationType.Castle => "Castle",
            PieceAnimationType.EnPassant => "EnPassant",
            _ => ""
        };

        if (!string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName);
        }
    }

    // Play animation through Legacy Animation component
    private void PlayLegacyAnimation(AnimationClip clip, PieceAnimationType animationType)
    {
        string animationName = animationType.ToString() + "Animation";
        animationComponent.AddClip(clip, animationName);
        animationComponent.Play(animationName);
    }

    // Manual animation fallback
    private IEnumerator PlayManualAnimation(PieceAnimationType animationType, AnimationCurve curve)
    {
        float animationTime = 0f;
        Vector3 originalScale = transform.localScale;
        
        // Store the initial local position to maintain exact height
        Vector3 initialLocalPosition = transform.localPosition;

        while (animationTime < animationSpeed)
        {
            float t = curve.Evaluate(animationTime / animationSpeed);

            switch (animationType)
            {
                case PieceAnimationType.Spawn:
                    transform.localScale = originalScale * t;
                    break;
                case PieceAnimationType.Death:
                    transform.localScale = originalScale * (1f - t);
                    break;
                case PieceAnimationType.Move:
                    // Move horizontally while maintaining exact local height
                    if (pieceController != null && pieceController.currentTile != null)
                    {
                        // Get target world position
                        Vector3 targetWorldPosition = pieceController.currentTile.transform.position;
                        
                        // Interpolate X and Z positions
                        Vector3 currentLocalPosition = transform.localPosition;
                        Vector3 targetLocalPosition = transform.parent 
                            ? transform.parent.InverseTransformPoint(targetWorldPosition) 
                            : targetWorldPosition;
                        
                        // Interpolate only X and Z, keep Y constant
                        Vector3 newLocalPosition = new Vector3(
                            Mathf.Lerp(currentLocalPosition.x, targetLocalPosition.x, t),
                            initialLocalPosition.y,
                            Mathf.Lerp(currentLocalPosition.z, targetLocalPosition.z, t)
                        );
                        
                        // Set the new local position
                        transform.localPosition = newLocalPosition;
                    }
                    break;
                case PieceAnimationType.Capturing:
                    // Capturing animation effect (different from being captured)
                    transform.localScale = originalScale * (1f + t * 0.2f);
                    break;
                case PieceAnimationType.Captured:
                    // Being captured animation effect
                    transform.localScale = originalScale * (1f - t * 0.5f);
                    break;
                case PieceAnimationType.Idle:
                    // Subtle idle animation without position change
                    float idleOffset = Mathf.Sin(t * Mathf.PI * 2) * 0.05f;
                    Vector3 idleLocalPosition = initialLocalPosition;
                    idleLocalPosition.y += transform.up.y * idleOffset;
                    transform.localPosition = idleLocalPosition;
                    break;
            }

            animationTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final local position is exactly at the initial height
        transform.localPosition = initialLocalPosition;

        // Reset to original state
        transform.localScale = originalScale;
    }

    // Public methods for specific animations
    public void SpawnPiece()
    {
        PlayAnimation(PieceAnimationType.Spawn);
        
        // Start a coroutine to transition to Idle after spawn animation
        StartCoroutine(TransitionToIdleAfterSpawn());
    }

    private IEnumerator TransitionToIdleAfterSpawn()
    {
        // Wait for the spawn animation duration
        yield return new WaitForSeconds(pieceData != null ? pieceData.spawnAnimationSpeed : 1f);
        
        // Transition to Idle
        IdlePiece();
    }
    public void IdlePiece() => PlayAnimation(PieceAnimationType.Idle);
    public void MovePiece() => PlayAnimation(PieceAnimationType.Move);
    public void StartCapturePiece() => PlayAnimation(PieceAnimationType.Capturing);
    public void SetCapturedPiece() => PlayAnimation(PieceAnimationType.Captured);
    public void DestroyPiece() => PlayAnimation(PieceAnimationType.Death);
    public void CastlePiece() => PlayAnimation(PieceAnimationType.Castle);
    public void EnPassantPiece() => PlayAnimation(PieceAnimationType.EnPassant);
} 