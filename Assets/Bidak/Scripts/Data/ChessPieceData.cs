using UnityEngine;
using System.Collections.Generic; // Added for List

[CreateAssetMenu(fileName = "New Chess Piece", menuName = "Chess/Piece Data")]
public class ChessPieceData : ScriptableObject
{
    [Header("Piece Identification")]
    public string pieceName;
    public PieceType pieceType;
    
    [Header("Player Identification")]
    public Color playerType = Color.white;
    
    [Header("Material Selection")]
    public List<Material> availableMaterials = new List<Material>();
    public int selectedMaterialIndex = 0;

    // Enum to define player types

    [Header("Visual References")]
    public GameObject modelPrefab;
    public Sprite icon;

    [Header("Positioning")]
    public Vector3 startPosition;
    public Vector3 currentPosition;
    public string currentTileNotation; // New property to track current tile

    [Header("Piece Attributes")]
    public int points;
    
    [Header("Movement")]
    public bool canCastle;
    public bool canEnPassant;
    public bool isFirstMove = true;

    [Header("State")]
    public bool isDead = false;

    [Header("Animations")]
    // Idle Animation
    public AnimationClip idleAnimation;
    public float idleAnimationSpeed = 1f;
    public AnimationCurve idleAnimationCurve;

    // Movement Animation
    public AnimationClip moveAnimation;
    public float moveAnimationSpeed = 1f;
    public AnimationCurve moveAnimationCurve;

    // Spawn Animation
    public AnimationClip spawnAnimation;
    public float spawnAnimationSpeed = 1f;
    public AnimationCurve spawnAnimationCurve;

    // Capture Animations
    // Animation when this piece is capturing another piece
    public AnimationClip capturingAnimation;
    public float capturingAnimationSpeed = 1f;
    public AnimationCurve capturingAnimationCurve;

    // Animation when this piece is being captured
    public AnimationClip capturedAnimation;
    public float capturedAnimationSpeed = 1f;
    public AnimationCurve capturedAnimationCurve;

    // Death/Destruction Animation
    public AnimationClip deathAnimation;
    public float deathAnimationSpeed = 1f;
    public AnimationCurve deathAnimationCurve;

    // Special Move Animations
    public AnimationClip castleAnimation;
    public AnimationClip enPassantAnimation;

    [Header("Spawn Effects")]
    public GameObject spawnEffectPrefab;
    public Color spawnEffectColor = Color.white;
    public float spawnEffectDuration = 1f;
    public float spawnEffectIntensity = 1f;

    [Header("Move Effects")]
    public GameObject moveEffectPrefab;
    public Color moveEffectColor = Color.white;
    public float moveEffectDuration = 0.5f;
    public float moveEffectIntensity = 1f;
    public bool loopMoveEffect = true;
    
    // New field for precise scale control
    public Vector3 moveEffectScale = Vector3.one * 0.5f; // Default to half size

    [Header("Sound Effects")]
    public AudioClip moveSoundEffect;
    public float moveSoundVolume = 1f;
    public AudioClip captureSoundEffect;
    public float captureSoundVolume = 1f;
    public AudioClip spawnSoundEffect;
    public float spawnSoundVolume = 1f;
    public float spawnSoundDelay = 0f; // Delay before playing spawn sound
    public AudioClip deathSoundEffect;
    public float deathSoundVolume = 1f;

    [Header("Capture Effects")]
    public GameObject capturingEffectPrefab;
    public Color capturingEffectColor = Color.white;
    public float capturingEffectDuration = 0.5f;
    public float capturingEffectIntensity = 1f;

    [Header("Captured Effects")]
    public GameObject capturedEffectPrefab;
    public Color capturedEffectColor = Color.white;
    public float capturedEffectDuration = 0.5f;
    public float capturedEffectIntensity = 1f;

    [Header("Death Effects")]
    public GameObject deathEffectPrefab;
    public Color deathEffectColor = Color.white;
    public float deathEffectDuration = 1f;
    public float deathEffectIntensity = 1f;

    // Enum to define different chess piece types
    public enum PieceType
    {
        Pawn,
        Rook,
        Knight,
        Bishop,
        Queen,
        King
    }

    // Method to set default movement for each piece type
    public void SetDefaultMovement()
    {
        switch (pieceType)
        {
            case PieceType.Pawn:
                pieceName = "Pawn";
                points = 1;
                canEnPassant = true;
                break;

            case PieceType.Rook:
                pieceName = "Rook";
                points = 5;
                canCastle = true;
                break;

            case PieceType.Knight:
                pieceName = "Knight";
                points = 3;
                break;

            case PieceType.Bishop:
                pieceName = "Bishop";
                points = 3;
                break;

            case PieceType.Queen:
                pieceName = "Queen";
                points = 9;
                break;

            case PieceType.King:
                pieceName = "King";
                points = 0;
                canCastle = true;
                break;
        }
    }

    // Method to update piece position
    public void UpdatePosition(Vector3 newPosition, string tileNotation)
    {
        currentPosition = newPosition;
        currentTileNotation = tileNotation;
        isFirstMove = false;
    }

    // Method to reset piece state
    public void ResetPiece()
    {
        currentPosition = startPosition;
        currentTileNotation = null;
        isDead = false;
        isFirstMove = true;
    }

    // Animation-related methods
    public void SetIdleAnimation(AnimationClip clip, float speed = 1f, AnimationCurve curve = null)
    {
        idleAnimation = clip;
        idleAnimationSpeed = speed;
        idleAnimationCurve = curve ?? AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }

    public void SetMoveAnimation(AnimationClip clip, float speed = 1f, AnimationCurve curve = null)
    {
        moveAnimation = clip;
        moveAnimationSpeed = speed;
        moveAnimationCurve = curve ?? AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }

    public void SetSpawnAnimation(AnimationClip clip, float speed = 1f, AnimationCurve curve = null)
    {
        spawnAnimation = clip;
        spawnAnimationSpeed = speed;
        spawnAnimationCurve = curve ?? AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    }

    // New methods for capturing animations
    public void SetCapturingAnimation(AnimationClip clip, float speed = 1f, AnimationCurve curve = null)
    {
        capturingAnimation = clip;
        capturingAnimationSpeed = speed;
        capturingAnimationCurve = curve ?? AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }

    public void SetCapturedAnimation(AnimationClip clip, float speed = 1f, AnimationCurve curve = null)
    {
        capturedAnimation = clip;
        capturedAnimationSpeed = speed;
        capturedAnimationCurve = curve ?? AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    }

    public void SetDeathAnimation(AnimationClip clip, float speed = 1f, AnimationCurve curve = null)
    {
        deathAnimation = clip;
        deathAnimationSpeed = speed;
        deathAnimationCurve = curve ?? AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    }

    // Special move animations
    public void SetCastleAnimation(AnimationClip clip)
    {
        castleAnimation = clip;
    }

    public void SetEnPassantAnimation(AnimationClip clip)
    {
        enPassantAnimation = clip;
    }

    // Methods to set effects
    public void SetCapturingEffect(GameObject effectPrefab, Color? color = null, float duration = 0.5f, float intensity = 1f)
    {
        capturingEffectPrefab = effectPrefab;
        capturingEffectColor = color ?? Color.white;
        capturingEffectDuration = duration;
        capturingEffectIntensity = intensity;
    }

    public void SetCapturedEffect(GameObject effectPrefab, Color? color = null, float duration = 0.5f, float intensity = 1f)
    {
        capturedEffectPrefab = effectPrefab;
        capturedEffectColor = color ?? Color.white;
        capturedEffectDuration = duration;
        capturedEffectIntensity = intensity;
    }

    public void SetDeathEffect(GameObject effectPrefab, Color? color = null, float duration = 1f, float intensity = 1f)
    {
        deathEffectPrefab = effectPrefab;
        deathEffectColor = color ?? Color.white;
        deathEffectDuration = duration;
        deathEffectIntensity = intensity;
    }
}
