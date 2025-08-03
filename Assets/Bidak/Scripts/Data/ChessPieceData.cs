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

    public bool isWhite = true;

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

    [Header("Card Effects")]
    public List<Bidak.Data.CardEffectData> appliedEffects = new List<Bidak.Data.CardEffectData>();
    
    [Header("Enhanced Movement Capabilities")]
    public bool canMoveMultipleTimes = false;
    public int remainingMoves = 1;
    public bool canAttackDiagonally = false;
    public bool canMoveBackward = false;
    public bool canMoveSideways = false;
    
    [Header("Protection Status")]
    public bool hasLightProtection = false;
    public bool hasFullProtection = false;
    public int protectionTurnsRemaining = 0;
    public bool isBlockaded = false;
    public int blockadeTurnsRemaining = 0;
    
    [Header("Special Abilities")]
    public bool canPromoteInPlace = false;
    public bool canRevive = false;
    public bool canIgnoreBlocking = false;
    public int immobilityTurns = 0; // For tracking how long piece hasn't moved
    
    [Header("Turn Tracking")]
    public int lastMoveTurn = -1;
    public int turnsWithoutMoving = 0;
    
    [Header("Card Effect Properties")]
    public int? cardEffectMoveCountBonus = null;
    public bool cardEffectCanMoveStraight = false;
    public bool cardEffectCanMoveDiagonally = false;
    public bool cardEffectCanAttackDiagonally = false;
    public bool cardEffectCanAttackStraight = false;
    public bool cardEffectHasLightProtection = false;
    public bool cardEffectHasFullProtection = false;
    public bool cardEffectCanSwapPositions = false;
    public bool cardEffectCanRevive = false;
    public bool cardEffectCanPromoteInPlace = false;
    public bool cardEffectCanCreateBlockade = false;
    public bool cardEffectCanJump = false;
    public bool cardEffectCanMoveBackward = false;
    public bool cardEffectCanMoveSideways = false;
    public bool cardEffectIsFrozen = false;
    public int cardEffectStepsForward = 0;
    public int cardEffectProtectionDuration = 0;
    public int cardEffectDuration = 0;

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

    // Card Effect Management Methods
    
    /// <summary>
    /// Apply a card effect to this piece
    /// </summary>
    public void ApplyCardEffect(Bidak.Data.CardEffectData effectData)
    {
        if (effectData == null || !effectData.isEnabled)
            return;
            
        // Add to applied effects
        appliedEffects.Add(effectData.Clone());
        
        // Apply the specific effect based on type
        ApplySpecificEffect(effectData);
    }
    
    /// <summary>
    /// Remove a specific card effect
    /// </summary>
    public void RemoveCardEffect(Bidak.Data.CardEffectType effectType)
    {
        appliedEffects.RemoveAll(effect => effect.effectType == effectType);
        RemoveSpecificEffect(effectType);
    }
    
    /// <summary>
    /// Update all active card effects (called each turn)
    /// </summary>
    public void UpdateCardEffects(int currentTurn)
    {
        lastMoveTurn = currentTurn;
        
        // Update turn-based effects
        for (int i = appliedEffects.Count - 1; i >= 0; i--)
        {
            var effect = appliedEffects[i];
            
            // Decrease duration if it's turn-based
            if (effect.parameters.turnDuration > 0)
            {
                effect.parameters.turnDuration--;
                if (effect.parameters.turnDuration <= 0)
                {
                    RemoveCardEffect(effect.effectType);
                }
            }
        }
        
        // Update protection timers
        if (protectionTurnsRemaining > 0)
        {
            protectionTurnsRemaining--;
            if (protectionTurnsRemaining <= 0)
            {
                hasLightProtection = false;
                hasFullProtection = false;
            }
        }
        
        // Update blockade timers
        if (blockadeTurnsRemaining > 0)
        {
            blockadeTurnsRemaining--;
            if (blockadeTurnsRemaining <= 0)
            {
                isBlockaded = false;
            }
        }
    }
    
    /// <summary>
    /// Check if piece has a specific card effect active
    /// </summary>
    public bool HasCardEffect(Bidak.Data.CardEffectType effectType)
    {
        return appliedEffects.Exists(effect => effect.effectType == effectType);
    }
    
    /// <summary>
    /// Get parameters for a specific card effect
    /// </summary>
    public Bidak.Data.CardEffectParameters GetEffectParameters(Bidak.Data.CardEffectType effectType)
    {
        var effect = appliedEffects.Find(e => e.effectType == effectType);
        return effect?.parameters;
    }
    
    /// <summary>
    /// Reset movement for a new turn
    /// </summary>
    public void ResetMovementForTurn()
    {
        if (canMoveMultipleTimes)
        {
            // Reset remaining moves based on active effects
            remainingMoves = 1; // Default
            
            if (HasCardEffect(Bidak.Data.CardEffectType.DoubleMove))
                remainingMoves = 2;
            else if (HasCardEffect(Bidak.Data.CardEffectType.TripleMove))
                remainingMoves = 3;
        }
        else
        {
            remainingMoves = 1;
        }
    }
    
    /// <summary>
    /// Use one move
    /// </summary>
    public void UseMove()
    {
        if (remainingMoves > 0)
        {
            remainingMoves--;
            turnsWithoutMoving = 0;
        }
    }
    
    /// <summary>
    /// Check if piece can move this turn
    /// </summary>
    public bool CanMoveThisTurn()
    {
        return remainingMoves > 0 && !isBlockaded;
    }
    
    private void ApplySpecificEffect(Bidak.Data.CardEffectData effectData)
    {
        switch (effectData.effectType)
        {
            case Bidak.Data.CardEffectType.DoubleMove:
                canMoveMultipleTimes = true;
                remainingMoves = 2;
                break;
                
            case Bidak.Data.CardEffectType.TripleMove:
                canMoveMultipleTimes = true;
                remainingMoves = 3;
                break;
                
            case Bidak.Data.CardEffectType.DiagonalAttack:
                canAttackDiagonally = true;
                break;
                
            case Bidak.Data.CardEffectType.StraightMove:
                // Special logic for straight moves without attack
                break;
                
            case Bidak.Data.CardEffectType.ProtectedRing:
                hasLightProtection = true;
                protectionTurnsRemaining = effectData.parameters.protectionDuration;
                break;
                
            case Bidak.Data.CardEffectType.BlockadeMove:
                isBlockaded = true;
                blockadeTurnsRemaining = 2;
                break;
                
            case Bidak.Data.CardEffectType.ForwardTwoMoves:
                canMoveBackward = true;
                break;
                
            case Bidak.Data.CardEffectType.PowerfulMove:
                canIgnoreBlocking = true;
                break;
                
            case Bidak.Data.CardEffectType.BackMove:
                canRevive = true;
                break;
                
            case Bidak.Data.CardEffectType.LeapMove:
                canMoveSideways = true;
                break;
                
            case Bidak.Data.CardEffectType.RestoreMove:
                canMoveSideways = true;
                break;
                
            case Bidak.Data.CardEffectType.NotToday:
                hasFullProtection = true;
                protectionTurnsRemaining = 1;
                break;
                
            case Bidak.Data.CardEffectType.UnstoppableForce:
                hasFullProtection = true;
                protectionTurnsRemaining = 2;
                break;
        }
    }
    
    private void RemoveSpecificEffect(Bidak.Data.CardEffectType effectType)
    {
        switch (effectType)
        {
            case Bidak.Data.CardEffectType.DoubleMove:
            case Bidak.Data.CardEffectType.TripleMove:
                canMoveMultipleTimes = false;
                remainingMoves = 1;
                break;
                
            case Bidak.Data.CardEffectType.DiagonalAttack:
                canAttackDiagonally = false;
                break;
                
            case Bidak.Data.CardEffectType.ProtectedRing:
                hasLightProtection = false;
                break;
                
            case Bidak.Data.CardEffectType.BlockadeMove:
                isBlockaded = false;
                break;
                
            case Bidak.Data.CardEffectType.ForwardTwoMoves:
                canMoveBackward = false;
                break;
                
            case Bidak.Data.CardEffectType.PowerfulMove:
                canIgnoreBlocking = false;
                break;
                
            case Bidak.Data.CardEffectType.BackMove:
                canRevive = false;
                break;
                
            case Bidak.Data.CardEffectType.LeapMove:
            case Bidak.Data.CardEffectType.RestoreMove:
                canMoveSideways = false;
                break;
                
            case Bidak.Data.CardEffectType.NotToday:
            case Bidak.Data.CardEffectType.UnstoppableForce:
                hasFullProtection = false;
                break;
        }
    }
}
