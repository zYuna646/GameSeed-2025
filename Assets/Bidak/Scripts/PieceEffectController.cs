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
} 