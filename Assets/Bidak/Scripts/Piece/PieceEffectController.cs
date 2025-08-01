using UnityEngine;
using System.Collections;

public class PieceEffectController : MonoBehaviour
{
    [Header("Spawn Effect Settings")]
    public GameObject currentSpawnEffect;
    public float spawnEffectFadeInTime = 0.5f;
    public float spawnEffectFadeOutTime = 0.5f;
    public float pieceBodyVisibilityTime = 0.5f; // New parameter to control piece body visibility timing
    public AnimationCurve spawnEffectScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve spawnEffectFadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Move Effect Settings")]
    public GameObject currentMoveEffect;
    public float moveEffectFadeInTime = 0.3f;
    public float moveEffectFadeOutTime = 0.3f;

    [Header("Capturing Effect Settings")]
    public GameObject currentCapturingEffect;
    public bool isCapturingEffectPlaying = false;
    public float capturingEffectFadeInTime = 0.3f;
    public float capturingEffectFadeOutTime = 0.3f;
    public AnimationCurve capturingEffectScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve capturingEffectFadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Captured Effect Settings")]
    public GameObject currentCapturedEffect;
    public bool isCapturedEffectPlaying = false;
    public float capturedEffectFadeInTime = 0.3f;
    public float capturedEffectFadeOutTime = 0.3f;
    public AnimationCurve capturedEffectScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve capturedEffectFadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Death Effect Settings")]
    public GameObject currentDeathEffect;
    public bool isDeathEffectPlaying = false;
    public float deathEffectFadeInTime = 0.5f;
    public float deathEffectFadeOutTime = 0.5f;
    public AnimationCurve deathEffectScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve deathEffectFadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private ChessPieceData pieceData;
    private PieceController pieceController;

    public void SetPieceData(ChessPieceData data)
    {
        pieceData = data;
        pieceController = GetComponent<PieceController>();
    }

    public void PlaySpawnEffect()
    {
        if (pieceData == null || pieceData.spawnEffectPrefab == null)
        {
            Debug.LogWarning("No spawn effect prefab found for this piece.");
            ShowPieceBody();
            return;
        }

        // Find spawn position child with SpawnEffect tag
        Transform spawnEffectPosition = null;
        foreach (Transform child in transform)
        {
            if (child.CompareTag("SpawnEffect"))
            {
                spawnEffectPosition = child;
                break;
            }
        }

        // Fallback to piece's transform position if no tagged child found
        Vector3 spawnPosition = spawnEffectPosition != null 
            ? spawnEffectPosition.position 
            : transform.position;

        // Instantiate the spawn effect
        currentSpawnEffect = Instantiate(
            pieceData.spawnEffectPrefab, 
            spawnPosition, 
            Quaternion.identity, 
            transform
        );

        // Apply color if possible
        Renderer effectRenderer = currentSpawnEffect.GetComponent<Renderer>();
        if (effectRenderer != null)
        {
            effectRenderer.material.color = pieceData.spawnEffectColor;
        }

        // Start spawn effect coroutine
        StartCoroutine(SpawnEffectAnimation());
    }

    private IEnumerator SpawnEffectAnimation()
    {
        if (currentSpawnEffect == null) yield break;

        float totalEffectDuration = spawnEffectFadeInTime + pieceData.spawnEffectDuration + spawnEffectFadeOutTime;
        float pieceBodyShowTime = totalEffectDuration / 2f; // Show piece body in the middle of the effect

        float elapsedTime = 0f;
        
        // Fade in and scale up
        while (elapsedTime < spawnEffectFadeInTime)
        {
            float normalizedTime = elapsedTime / spawnEffectFadeInTime;
            
            // Scale effect
            float scale = spawnEffectScaleCurve.Evaluate(normalizedTime) * pieceData.spawnEffectIntensity;
            currentSpawnEffect.transform.localScale = Vector3.one * scale;

            // Fade in (if possible)
            Renderer renderer = currentSpawnEffect.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color currentColor = renderer.material.color;
                currentColor.a = spawnEffectFadeCurve.Evaluate(normalizedTime);
                renderer.material.color = currentColor;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Hold effect for duration
        float holdStartTime = elapsedTime;
        while (elapsedTime - holdStartTime < pieceData.spawnEffectDuration)
        {
            // Check if it's time to show piece body
            if (elapsedTime - holdStartTime >= pieceBodyVisibilityTime)
            {
                ShowPieceBody();
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < spawnEffectFadeOutTime)
        {
            float normalizedTime = elapsedTime / spawnEffectFadeOutTime;
            
            // Scale down
            float scale = spawnEffectScaleCurve.Evaluate(1f - normalizedTime) * pieceData.spawnEffectIntensity;
            currentSpawnEffect.transform.localScale = Vector3.one * scale;

            // Fade out
            Renderer renderer = currentSpawnEffect.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color currentColor = renderer.material.color;
                currentColor.a = spawnEffectFadeCurve.Evaluate(1f - normalizedTime);
                renderer.material.color = currentColor;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Destroy effect
        Destroy(currentSpawnEffect);

        // Ensure piece body is visible if it wasn't already shown
        ShowPieceBody();
    }

    public void PlayMoveEffect()
    {
        // Extensive logging for debugging
        Debug.Log("Attempting to play move effect");
        
        // Check piece data
        if (pieceData == null)
        {
            Debug.LogError("PieceData is null. Cannot play move effect.");
            return;
        }

        // Check move effect prefab
        if (pieceData.moveEffectPrefab == null)
        {
            Debug.LogWarning($"No move effect prefab found for piece type: {pieceData.pieceType}");
            return;
        }

        // Find spawn position child with SpawnEffect tag
        Transform spawnEffectPosition = null;
        foreach (Transform child in transform)
        {
            if (child.CompareTag("SpawnEffect"))
            {
                spawnEffectPosition = child;
                break;
            }
        }

        // Fallback to piece's transform position if no tagged child found
        Vector3 spawnPosition = spawnEffectPosition != null 
            ? spawnEffectPosition.position 
            : transform.position;

        Debug.Log($"Move effect spawn position: {spawnPosition}");

        // Instantiate the move effect
        currentMoveEffect = Instantiate(
            pieceData.moveEffectPrefab, 
            spawnPosition, 
            Quaternion.identity, 
            transform
        );

        // Set custom scale from piece data
        if (currentMoveEffect != null)
        {
            currentMoveEffect.transform.localScale = pieceData.moveEffectScale;
            Debug.Log($"Applied move effect scale: {pieceData.moveEffectScale}");
        }

        // Apply color if possible
        Renderer effectRenderer = currentMoveEffect.GetComponent<Renderer>();
        if (effectRenderer != null)
        {
            effectRenderer.material.color = pieceData.moveEffectColor;
            Debug.Log($"Applied move effect color: {pieceData.moveEffectColor}");
        }
        else
        {
            Debug.LogWarning("No renderer found on move effect prefab");
        }

        // Start move effect coroutine
        StartCoroutine(MoveEffectAnimation());
    }

    private IEnumerator MoveEffectAnimation()
    {
        if (currentMoveEffect == null)
        {
            Debug.LogError("Move effect is null in animation coroutine");
            yield break;
        }

        Debug.Log("Starting move effect animation");

        float elapsedTime = 0f;
        
        // Fade in and scale up
        while (elapsedTime < moveEffectFadeInTime)
        {
            float normalizedTime = elapsedTime / moveEffectFadeInTime;
            
            // Scale effect with custom scale
            float scaleMultiplier = spawnEffectScaleCurve.Evaluate(normalizedTime) * pieceData.moveEffectIntensity;
            currentMoveEffect.transform.localScale = pieceData.moveEffectScale * scaleMultiplier;

            // Fade in (if possible)
            Renderer renderer = currentMoveEffect.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color currentColor = renderer.material.color;
                currentColor.a = spawnEffectFadeCurve.Evaluate(normalizedTime);
                renderer.material.color = currentColor;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"Move effect loop setting: {pieceData.loopMoveEffect}");

        // Loop or hold effect
        float moveEffectTime = 0f;
        while (pieceData.loopMoveEffect)
        {
            moveEffectTime += Time.deltaTime;
            
            // Optional: add some subtle animation while looping
            if (currentMoveEffect != null)
            {
                float subtleScale = 1f + Mathf.Sin(moveEffectTime * 2f) * 0.1f;
                currentMoveEffect.transform.localScale = pieceData.moveEffectScale * subtleScale;
            }

            yield return null;
        }

        // If not looping, wait for duration
        if (!pieceData.loopMoveEffect)
        {
            yield return new WaitForSeconds(pieceData.moveEffectDuration);
        }

        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < moveEffectFadeOutTime)
        {
            float normalizedTime = elapsedTime / moveEffectFadeOutTime;
            
            // Scale down with custom scale
            float scaleMultiplier = spawnEffectScaleCurve.Evaluate(1f - normalizedTime) * pieceData.moveEffectIntensity;
            currentMoveEffect.transform.localScale = pieceData.moveEffectScale * scaleMultiplier;

            // Fade out
            Renderer renderer = currentMoveEffect.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color currentColor = renderer.material.color;
                currentColor.a = spawnEffectFadeCurve.Evaluate(1f - normalizedTime);
                renderer.material.color = currentColor;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Destroying move effect");

        // Destroy effect
        Destroy(currentMoveEffect);
    }

    public void PlayCapturingEffect()
    {
        // Prevent multiple effect spawns
        if (isCapturingEffectPlaying || pieceData == null || pieceData.capturingEffectPrefab == null)
        {
            return;
        }

        // Find child with SlashEffect tag
        Transform slashEffectPosition = null;
        foreach (Transform child in transform)
        {
            if (child.CompareTag("SlashEffect"))
            {
                slashEffectPosition = child;
                break;
            }
        }

        // Fallback to piece's transform position if no tagged child found
        Vector3 spawnPosition = slashEffectPosition != null 
            ? slashEffectPosition.position 
            : transform.position;

        // Instantiate the capturing effect
        currentCapturingEffect = Instantiate(
            pieceData.capturingEffectPrefab, 
            spawnPosition, 
            Quaternion.identity, 
            transform
        );

        // Apply color if possible
        Renderer effectRenderer = currentCapturingEffect.GetComponent<Renderer>();
        if (effectRenderer != null)
        {
            effectRenderer.material.color = pieceData.capturingEffectColor;
        }

        // Mark effect as playing
        isCapturingEffectPlaying = true;

        // Start capturing effect coroutine
        StartCoroutine(CapturingEffectAnimation());
    }

    private IEnumerator CapturingEffectAnimation()
    {
        if (currentCapturingEffect == null) 
        {
            isCapturingEffectPlaying = false;
            yield break;
        }

        float totalEffectDuration = capturingEffectFadeInTime + pieceData.capturingEffectDuration + capturingEffectFadeOutTime;
        float elapsedTime = 0f;
        
        // Fade in and scale up
        while (elapsedTime < capturingEffectFadeInTime)
        {
            float normalizedTime = elapsedTime / capturingEffectFadeInTime;
            
            // Scale effect
            float scale = capturingEffectScaleCurve.Evaluate(normalizedTime) * pieceData.capturingEffectIntensity;
            currentCapturingEffect.transform.localScale = Vector3.one * scale;

            // Fade in (if possible)
            Renderer renderer = currentCapturingEffect.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color currentColor = renderer.material.color;
                currentColor.a = capturingEffectFadeCurve.Evaluate(normalizedTime);
                renderer.material.color = currentColor;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Hold effect for duration
        float holdStartTime = elapsedTime;
        while (elapsedTime - holdStartTime < pieceData.capturingEffectDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < capturingEffectFadeOutTime)
        {
            float normalizedTime = elapsedTime / capturingEffectFadeOutTime;
            
            // Scale down
            float scale = capturingEffectScaleCurve.Evaluate(1f - normalizedTime) * pieceData.capturingEffectIntensity;
            currentCapturingEffect.transform.localScale = Vector3.one * scale;

            // Fade out
            Renderer renderer = currentCapturingEffect.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color currentColor = renderer.material.color;
                currentColor.a = capturingEffectFadeCurve.Evaluate(1f - normalizedTime);
                renderer.material.color = currentColor;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Destroy effect
        Destroy(currentCapturingEffect);

        // Mark effect as finished
        isCapturingEffectPlaying = false;
    }

    public void PlayCapturedEffect()
    {
        // Prevent multiple effect spawns
        if (isCapturedEffectPlaying || pieceData == null || pieceData.capturedEffectPrefab == null)
        {
            return;
        }

        // Find child with HitEffect tag
        Transform hitEffectPosition = null;
        foreach (Transform child in transform)
        {
            if (child.CompareTag("HitEffect"))
            {
                hitEffectPosition = child;
                break;
            }
        }

        // Fallback to piece's transform position if no tagged child found
        Vector3 spawnPosition = hitEffectPosition != null 
            ? hitEffectPosition.position 
            : transform.position;

        // Instantiate the captured effect
        currentCapturedEffect = Instantiate(
            pieceData.capturedEffectPrefab, 
            spawnPosition, 
            Quaternion.identity, 
            transform
        );

        // Apply color if possible
        Renderer effectRenderer = currentCapturedEffect.GetComponent<Renderer>();
        if (effectRenderer != null)
        {
            effectRenderer.material.color = pieceData.capturedEffectColor;
        }

        // Mark effect as playing
        isCapturedEffectPlaying = true;

        // Start captured effect coroutine
        StartCoroutine(CapturedEffectAnimation());
    }

    private IEnumerator CapturedEffectAnimation()
    {
        if (currentCapturedEffect == null) 
        {
            isCapturedEffectPlaying = false;
            yield break;
        }

        float totalEffectDuration = capturedEffectFadeInTime + pieceData.capturedEffectDuration + capturedEffectFadeOutTime;
        float elapsedTime = 0f;
        
        // Fade in and scale up
        while (elapsedTime < capturedEffectFadeInTime)
        {
            float normalizedTime = elapsedTime / capturedEffectFadeInTime;
            
            // Scale effect
            float scale = capturedEffectScaleCurve.Evaluate(normalizedTime) * pieceData.capturedEffectIntensity;
            currentCapturedEffect.transform.localScale = Vector3.one * scale;

            // Fade in (if possible)
            Renderer renderer = currentCapturedEffect.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color currentColor = renderer.material.color;
                currentColor.a = capturedEffectFadeCurve.Evaluate(normalizedTime);
                renderer.material.color = currentColor;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Hold effect for duration
        float holdStartTime = elapsedTime;
        while (elapsedTime - holdStartTime < pieceData.capturedEffectDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < capturedEffectFadeOutTime)
        {
            float normalizedTime = elapsedTime / capturedEffectFadeOutTime;
            
            // Scale down
            float scale = capturedEffectScaleCurve.Evaluate(1f - normalizedTime) * pieceData.capturedEffectIntensity;
            currentCapturedEffect.transform.localScale = Vector3.one * scale;

            // Fade out
            Renderer renderer = currentCapturedEffect.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color currentColor = renderer.material.color;
                currentColor.a = capturedEffectFadeCurve.Evaluate(1f - normalizedTime);
                renderer.material.color = currentColor;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Destroy effect
        Destroy(currentCapturedEffect);

        // Mark effect as finished
        isCapturedEffectPlaying = false;
    }

    public void PlayDeathEffect()
    {
        // Prevent multiple effect spawns
        if (isDeathEffectPlaying || pieceData == null || pieceData.deathEffectPrefab == null)
        {
            return;
        }

        // Find spawn position child with SpawnEffect tag
        Transform spawnEffectPosition = null;
        foreach (Transform child in transform)
        {
            if (child.CompareTag("SpawnEffect"))
            {
                spawnEffectPosition = child;
                break;
            }
        }

        // Fallback to piece's transform position if no tagged child found
        Vector3 spawnPosition = spawnEffectPosition != null 
            ? spawnEffectPosition.position 
            : transform.position;

        // Instantiate the death effect
        currentDeathEffect = Instantiate(
            pieceData.deathEffectPrefab, 
            spawnPosition, 
            Quaternion.identity, 
            transform
        );

        // Apply color if possible
        Renderer effectRenderer = currentDeathEffect.GetComponent<Renderer>();
        if (effectRenderer != null)
        {
            effectRenderer.material.color = pieceData.deathEffectColor;
        }

        // Mark effect as playing
        isDeathEffectPlaying = true;

        // Start death effect coroutine
        StartCoroutine(DeathEffectAnimation());
    }

    private IEnumerator DeathEffectAnimation()
    {
        if (currentDeathEffect == null) 
        {
            isDeathEffectPlaying = false;
            yield break;
        }

        float totalEffectDuration = deathEffectFadeInTime + pieceData.deathEffectDuration + deathEffectFadeOutTime;
        float elapsedTime = 0f;
        
        // Fade in and scale up
        while (elapsedTime < deathEffectFadeInTime)
        {
            float normalizedTime = elapsedTime / deathEffectFadeInTime;
            
            // Scale effect
            float scale = deathEffectScaleCurve.Evaluate(normalizedTime) * pieceData.deathEffectIntensity;
            currentDeathEffect.transform.localScale = Vector3.one * scale;

            // Fade in (if possible)
            Renderer renderer = currentDeathEffect.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color currentColor = renderer.material.color;
                currentColor.a = deathEffectFadeCurve.Evaluate(normalizedTime);
                renderer.material.color = currentColor;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Hold effect for duration
        float holdStartTime = elapsedTime;
        while (elapsedTime - holdStartTime < pieceData.deathEffectDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < deathEffectFadeOutTime)
        {
            float normalizedTime = elapsedTime / deathEffectFadeOutTime;
            
            // Scale down
            float scale = deathEffectScaleCurve.Evaluate(1f - normalizedTime) * pieceData.deathEffectIntensity;
            currentDeathEffect.transform.localScale = Vector3.one * scale;

            // Fade out
            Renderer renderer = currentDeathEffect.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color currentColor = renderer.material.color;
                currentColor.a = deathEffectFadeCurve.Evaluate(1f - normalizedTime);
                renderer.material.color = currentColor;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Destroy effect
        Destroy(currentDeathEffect);

        // Hide piece body
        if (pieceController != null)
        {
            // Optional: you might want to actually destroy the piece instead of just hiding it
            pieceController.gameObject.SetActive(false);
        }

        // Mark effect as finished
        isDeathEffectPlaying = false;
    }

    private void ShowPieceBody()
    {
        // Try to show piece body through PieceController
        if (pieceController != null)
        {
            pieceController.ShowPieceBody();
        }
        // Fallback method if PieceController is not available
        else
        {
            Transform pieceBody = transform.Find("PieceBody");
            if (pieceBody != null)
            {
                pieceBody.gameObject.SetActive(true);
            }
        }
    }

    // Optional method to stop and clear spawn effect
    public void StopSpawnEffect()
    {
        if (currentSpawnEffect != null)
        {
            StopAllCoroutines();
            Destroy(currentSpawnEffect);
        }
    }

    public void StopMoveEffect()
    {
        if (currentMoveEffect != null)
        {
            StopAllCoroutines();
            Destroy(currentMoveEffect);
        }
    }

    // Stop methods for new effects
    public void StopCapturingEffect()
    {
        if (currentCapturingEffect != null)
        {
            StopAllCoroutines();
            Destroy(currentCapturingEffect);
            isCapturingEffectPlaying = false;
        }
    }

    public void StopCapturedEffect()
    {
        if (currentCapturedEffect != null)
        {
            StopAllCoroutines();
            Destroy(currentCapturedEffect);
            isCapturedEffectPlaying = false;
        }
    }

    public void StopDeathEffect()
    {
        if (currentDeathEffect != null)
        {
            StopAllCoroutines();
            Destroy(currentDeathEffect);
            isDeathEffectPlaying = false;
        }
    }
} 