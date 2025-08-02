using UnityEngine;
using System.Collections;

public class PieceSoundsController : MonoBehaviour
{
    [Header("Sound Components")]
    public AudioSource audioSource;
    public ChessPieceData pieceData;

    [Header("Sound Settings")]
    public bool enableSounds = true;

    private void Awake()
    {
        // Find MainCamera and get its AudioSource
        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamera != null)
        {
            audioSource = mainCamera.GetComponent<AudioSource>();
        }

        // Fallback if no AudioSource found on MainCamera
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on MainCamera. Creating a new AudioSource on MainCamera.");
            audioSource = mainCamera != null 
                ? mainCamera.AddComponent<AudioSource>() 
                : Camera.main.gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayMoveSound()
    {
        if (!enableSounds || pieceData == null || pieceData.moveSoundEffect == null) return;

        audioSource.volume = pieceData.moveSoundVolume;
        audioSource.clip = pieceData.moveSoundEffect;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void StopMoveSound()
    {
        if (audioSource != null && audioSource.loop && audioSource.clip == pieceData.moveSoundEffect)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    public void PlayCaptureSound()
    {
        if (!enableSounds || pieceData == null || pieceData.captureSoundEffect == null) return;

        audioSource.volume = pieceData.captureSoundVolume;
        audioSource.PlayOneShot(pieceData.captureSoundEffect);
    }

    public void PlaySpawnSound()
    {
        if (!enableSounds || pieceData == null || pieceData.spawnSoundEffect == null) return;

        // If there's a delay, use a coroutine
        if (pieceData.spawnSoundDelay > 0f)
        {
            StartCoroutine(PlaySpawnSoundWithDelay());
        }
        else
        {
            // Play immediately
            audioSource.volume = pieceData.spawnSoundVolume;
            audioSource.PlayOneShot(pieceData.spawnSoundEffect);
        }
    }

    private IEnumerator PlaySpawnSoundWithDelay()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(pieceData.spawnSoundDelay);

        // Play the sound
        audioSource.volume = pieceData.spawnSoundVolume;
        audioSource.PlayOneShot(pieceData.spawnSoundEffect);
    }

    public void PlayDeathSound()
    {
        if (!enableSounds || pieceData == null || pieceData.deathSoundEffect == null) return;

        audioSource.volume = pieceData.deathSoundVolume;
        audioSource.PlayOneShot(pieceData.deathSoundEffect);
    }
}