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
} 