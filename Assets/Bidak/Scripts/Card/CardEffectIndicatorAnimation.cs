using UnityEngine;

namespace Bidak.Card
{
    /// <summary>
    /// Handles animation for card effect indicators above pieces
    /// Provides floating, rotating, and bobbing effects
    /// </summary>
    public class CardEffectIndicatorAnimation : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float rotationSpeed = 30f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.3f;
        [SerializeField] private bool enableRotation = true;
        [SerializeField] private bool enableBobbing = true;
        [SerializeField] private bool enablePulse = true;
        
        [Header("Pulse Settings")]
        [SerializeField] private float pulseSpeed = 1.5f;
        [SerializeField] private float pulseIntensity = 0.2f;
        
        // Animation state
        private Vector3 startPosition;
        private Vector3 originalScale;
        private float timeOffset;
        private bool isInitialized = false;
        
        private void Start()
        {
            if (!isInitialized)
            {
                Initialize(rotationSpeed, bobSpeed, bobHeight);
            }
        }
        
        /// <summary>
        /// Initialize the animation with custom parameters
        /// </summary>
        public void Initialize(float rotSpeed, float bobSpd, float bobHgt)
        {
            rotationSpeed = rotSpeed;
            bobSpeed = bobSpd;
            bobHeight = bobHgt;
            
            startPosition = transform.position;
            originalScale = transform.localScale;
            
            // Add random time offset to prevent synchronized animations
            timeOffset = Random.Range(0f, Mathf.PI * 2f);
            
            isInitialized = true;
        }
        
        private void Update()
        {
            if (!isInitialized)
                return;
            
            float time = Time.time + timeOffset;
            
            // Rotation animation
            if (enableRotation)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }
            
            // Bobbing animation
            if (enableBobbing)
            {
                float bobOffset = Mathf.Sin(time * bobSpeed) * bobHeight;
                Vector3 newPosition = startPosition;
                newPosition.y += bobOffset;
                transform.position = newPosition;
            }
            
            // Pulse animation
            if (enablePulse)
            {
                float pulseScale = 1f + Mathf.Sin(time * pulseSpeed) * pulseIntensity;
                transform.localScale = originalScale * pulseScale;
            }
        }
        
        /// <summary>
        /// Update the start position (useful when piece moves)
        /// </summary>
        public void UpdateStartPosition(Vector3 newPosition)
        {
            startPosition = newPosition;
        }
        
        /// <summary>
        /// Enable or disable specific animation types
        /// </summary>
        public void SetAnimationEnabled(bool rotation, bool bobbing, bool pulse)
        {
            enableRotation = rotation;
            enableBobbing = bobbing;
            enablePulse = pulse;
        }
        
        /// <summary>
        /// Set animation speeds
        /// </summary>
        public void SetAnimationSpeeds(float rotSpeed, float bobSpd, float pulseSpd)
        {
            rotationSpeed = rotSpeed;
            bobSpeed = bobSpd;
            pulseSpeed = pulseSpd;
        }
        
        /// <summary>
        /// Play a special emphasis animation (e.g., when effect is about to expire)
        /// </summary>
        public void PlayEmphasisAnimation()
        {
            StartCoroutine(EmphasisAnimationCoroutine());
        }
        
        private System.Collections.IEnumerator EmphasisAnimationCoroutine()
        {
            // Store original values
            float originalRotSpeed = rotationSpeed;
            float originalBobSpeed = bobSpeed;
            float originalPulseSpeed = pulseSpeed;
            
            // Increase animation speeds
            rotationSpeed *= 2f;
            bobSpeed *= 1.5f;
            pulseSpeed *= 2f;
            
            // Play for 1 second
            yield return new WaitForSeconds(1f);
            
            // Restore original values
            rotationSpeed = originalRotSpeed;
            bobSpeed = originalBobSpeed;
            pulseSpeed = originalPulseSpeed;
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Validate settings
        /// </summary>
        private void OnValidate()
        {
            rotationSpeed = Mathf.Max(0f, rotationSpeed);
            bobSpeed = Mathf.Max(0f, bobSpeed);
            bobHeight = Mathf.Max(0f, bobHeight);
            pulseSpeed = Mathf.Max(0f, pulseSpeed);
            pulseIntensity = Mathf.Clamp01(pulseIntensity);
        }
        
        /// <summary>
        /// Draw gizmos in editor
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying && isInitialized)
            {
                // Draw bob range
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(startPosition + Vector3.up * bobHeight * 0.5f, 
                    new Vector3(0.1f, bobHeight, 0.1f));
            }
        }
        #endif
    }
}