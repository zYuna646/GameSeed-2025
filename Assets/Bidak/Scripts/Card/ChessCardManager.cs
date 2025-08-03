using System;
using System.Collections.Generic;
using UnityEngine;
using Bidak.Data;

namespace Bidak.Manager
{
    public class ChessCardManager : MonoBehaviour
    {
        [Header("Player 1 Card Management")]
        [SerializeField] private List<ChessCardData> player1ActiveCards = new List<ChessCardData>();
        [SerializeField] private List<ChessCardData> player1StorageCards = new List<ChessCardData>();

        [Header("Player 2 Card Management")]
        [SerializeField] private List<ChessCardData> player2ActiveCards = new List<ChessCardData>();
        [SerializeField] private List<ChessCardData> player2StorageCards = new List<ChessCardData>();

        [Header("Card Management Settings")]
        [SerializeField] private int maxActiveCards = 3;
        [SerializeField] private int maxStorageCards = 5;
        
        [Header("Card Audio Settings")]
        [SerializeField] private AudioClip cardHoverSound;
        [SerializeField] private AudioClip cardSelectSound;
        [SerializeField] private AudioClip cardActivateSound;
        [SerializeField][Range(0f, 1f)] private float cardSoundVolume = 0.5f;

        // Events for card-related actions
        public event Action<int, ChessCardData> OnCardActivated;
        public event Action<int, ChessCardData> OnCardDeactivated;
        public event Action<int, ChessCardData> OnCardAddedToStorage;
        public event Action<int, ChessCardData> OnCardRemovedFromStorage;
        public event Action<int> OnPlayerCardsChanged;
        public event Action<bool> OnCardModeChanged; // New event for card mode changes
        
        [Header("Card Mode Settings")]
        public bool isCardModeActive = false;
        public ChessCardHoverEffect currentSelectedCard = null;

        // Tracking for changes
        private List<ChessCardData> player1LastActiveCards = new List<ChessCardData>();
        private List<ChessCardData> player2LastActiveCards = new List<ChessCardData>();

        private void Start()
        {
            // Ensure all cards are properly initialized
            Update();
        }

        private void Update()
        {
            // Check for changes in Player 1 active cards
            CheckForCardChanges(1, player1ActiveCards, player1LastActiveCards);

            // Check for changes in Player 2 active cards
            CheckForCardChanges(2, player2ActiveCards, player2LastActiveCards);
            
            // Check for Ctrl+C to exit card mode
            if (Input.GetKeyDown(KeyCode.C) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                if (isCardModeActive)
                {
                    ExitCardMode();
                    Debug.Log("Exited card mode via Ctrl+C");
                }
            }
        }

        private void CheckForCardChanges(int playerIndex, List<ChessCardData> currentCards, List<ChessCardData> lastCards)
        {
            // Check if card count has changed
            if (currentCards.Count != lastCards.Count)
            {
                // Trigger update event
                OnPlayerCardsChanged?.Invoke(playerIndex);

                // Update last known cards
                lastCards.Clear();
                lastCards.AddRange(currentCards);
                return;
            }

            // Check if card contents have changed
            for (int i = 0; i < currentCards.Count; i++)
            {
                if (currentCards[i] != lastCards[i])
                {
                    // Trigger update event
                    OnPlayerCardsChanged?.Invoke(playerIndex);

                    // Update last known cards
                    lastCards.Clear();
                    lastCards.AddRange(currentCards);
                    return;
                }
            }
        }

        /// <summary>
        /// Activate a card from storage for a specific player
        /// </summary>
        /// <param name="playerIndex">1 or 2</param>
        /// <param name="card">Card to activate</param>
        /// <returns>True if activation successful, false otherwise</returns>
        public bool ActivateCard(int playerIndex, ChessCardData card)
        {
            var activeCards = playerIndex == 1 ? player1ActiveCards : player2ActiveCards;
            var storageCards = playerIndex == 1 ? player1StorageCards : player2StorageCards;

            // Check if can activate more cards
            if (activeCards.Count >= maxActiveCards)
            {
                Debug.LogWarning($"Player {playerIndex} cannot activate more cards. Max active cards reached.");
                return false;
            }

            // Remove from storage if present
            if (storageCards.Contains(card))
            {
                storageCards.Remove(card);
                OnCardRemovedFromStorage?.Invoke(playerIndex, card);
            }

            // Add to active cards
            activeCards.Add(card);
            OnCardActivated?.Invoke(playerIndex, card);
            return true;
        }

        /// <summary>
        /// Deactivate a card for a specific player
        /// </summary>
        /// <param name="playerIndex">1 or 2</param>
        /// <param name="card">Card to deactivate</param>
        /// <returns>True if deactivation successful, false otherwise</returns>
        public bool DeactivateCard(int playerIndex, ChessCardData card)
        {
            var activeCards = playerIndex == 1 ? player1ActiveCards : player2ActiveCards;
            var storageCards = playerIndex == 1 ? player1StorageCards : player2StorageCards;

            if (!activeCards.Contains(card))
            {
                Debug.LogWarning($"Card not found in player {playerIndex} active cards.");
                return false;
            }

            // Check storage capacity
            if (storageCards.Count >= maxStorageCards)
            {
                Debug.LogWarning($"Player {playerIndex} storage is full. Cannot deactivate card.");
                return false;
            }

            // Remove from active cards
            activeCards.Remove(card);
            OnCardDeactivated?.Invoke(playerIndex, card);

            // Add to storage
            storageCards.Add(card);
            OnCardAddedToStorage?.Invoke(playerIndex, card);

            return true;
        }

        /// <summary>
        /// Add a card directly to a player's storage
        /// </summary>
        /// <param name="playerIndex">1 or 2</param>
        /// <param name="card">Card to add to storage</param>
        /// <returns>True if added successfully, false otherwise</returns>
        public bool AddCardToStorage(int playerIndex, ChessCardData card)
        {
            var storageCards = playerIndex == 1 ? player1StorageCards : player2StorageCards;

            if (storageCards.Count >= maxStorageCards)
            {
                Debug.LogWarning($"Player {playerIndex} storage is full. Cannot add more cards.");
                return false;
            }

            storageCards.Add(card);
            OnCardAddedToStorage?.Invoke(playerIndex, card);
            return true;
        }

        /// <summary>
        /// Get active cards for a specific player
        /// </summary>
        /// <param name="playerIndex">1 or 2</param>
        /// <returns>List of active cards</returns>
        public List<ChessCardData> GetActiveCards(int playerIndex)
        {
            return playerIndex == 1 ? new List<ChessCardData>(player1ActiveCards) : new List<ChessCardData>(player2ActiveCards);
        }

        /// <summary>
        /// Get storage cards for a specific player
        /// </summary>
        /// <param name="playerIndex">1 or 2</param>
        /// <returns>List of storage cards</returns>
        public List<ChessCardData> GetStorageCards(int playerIndex)
        {
            return playerIndex == 1 ? new List<ChessCardData>(player1StorageCards) : new List<ChessCardData>(player2StorageCards);
        }

        /// <summary>
        /// Clear all cards for a specific player
        /// </summary>
        /// <param name="playerIndex">1 or 2</param>
        public void ClearPlayerCards(int playerIndex)
        {
            if (playerIndex == 1)
            {
                player1ActiveCards.Clear();
                player1StorageCards.Clear();
            }
            else if (playerIndex == 2)
            {
                player2ActiveCards.Clear();
                player2StorageCards.Clear();
            }
        }
        
        /// <summary>
        /// Setup ChessCardHoverEffect with audio settings from manager
        /// </summary>
        /// <param name="cardHoverEffect">The ChessCardHoverEffect component to setup</param>
        /// <param name="cardData">Card data to assign</param>
        /// <param name="playerIndex">Player index (1 or 2)</param>
        public void SetupCardHoverEffect(ChessCardHoverEffect cardHoverEffect, ChessCardData cardData, int playerIndex)
        {
            if (cardHoverEffect == null)
            {
                Debug.LogError("ChessCardHoverEffect is null - cannot setup");
                return;
            }
            
            // Set card data and player index
            cardHoverEffect.SetCardData(cardData, playerIndex);
            
            // Apply audio settings from manager
            cardHoverEffect.SetAudioSettings(cardHoverSound, cardSelectSound, cardActivateSound, cardSoundVolume);
            
            Debug.Log($"Setup card hover effect for {cardData?.cardName ?? "Unknown"} (Player {playerIndex}) with audio settings");
        }
        
        /// <summary>
        /// Setup multiple card hover effects with audio settings
        /// </summary>
        /// <param name="cardObjects">Array of GameObjects containing ChessCardHoverEffect components</param>
        /// <param name="playerIndex">Player index (1 or 2)</param>
        public void SetupPlayerCards(GameObject[] cardObjects, int playerIndex)
        {
            if (cardObjects == null || cardObjects.Length == 0)
            {
                Debug.LogWarning($"No card objects provided for player {playerIndex}");
                return;
            }
            
            var activeCards = GetActiveCards(playerIndex);
            
            for (int i = 0; i < cardObjects.Length && i < activeCards.Count; i++)
            {
                if (cardObjects[i] != null)
                {
                    ChessCardHoverEffect hoverEffect = cardObjects[i].GetComponent<ChessCardHoverEffect>();
                    if (hoverEffect == null)
                    {
                        hoverEffect = cardObjects[i].AddComponent<ChessCardHoverEffect>();
                        Debug.Log($"Added ChessCardHoverEffect component to {cardObjects[i].name}");
                    }
                    
                    SetupCardHoverEffect(hoverEffect, activeCards[i], playerIndex);
                }
            }
        }
        
        /// <summary>
        /// Get audio settings for external use
        /// </summary>
        /// <returns>Tuple containing audio clips and volume</returns>
        public (AudioClip hoverSound, AudioClip selectSound, AudioClip activateSound, float volume) GetAudioSettings()
        {
            return (cardHoverSound, cardSelectSound, cardActivateSound, cardSoundVolume);
        }
        
        /// <summary>
        /// Update audio settings for all existing card hover effects
        /// </summary>
        public void UpdateAllCardAudioSettings()
        {
            ChessCardHoverEffect[] allCardEffects = FindObjectsOfType<ChessCardHoverEffect>();
            
            foreach (var cardEffect in allCardEffects)
            {
                cardEffect.SetAudioSettings(cardHoverSound, cardSelectSound, cardActivateSound, cardSoundVolume);
            }
            
            Debug.Log($"Updated audio settings for {allCardEffects.Length} card hover effects");
        }
        
        /// <summary>
        /// Enter card mode when a card is selected
        /// </summary>
        /// <param name="selectedCard">The card that was selected</param>
        public void EnterCardMode(ChessCardHoverEffect selectedCard)
        {
            if (selectedCard == null) return;
            
            // Exit previous card mode if active
            if (isCardModeActive && currentSelectedCard != null)
            {
                currentSelectedCard.Deselect();
            }
            
            isCardModeActive = true;
            currentSelectedCard = selectedCard;
            
            OnCardModeChanged?.Invoke(true);
            
            Debug.Log($"Entered card mode with card: {selectedCard.cardData?.cardName ?? "Unknown"}");
        }
        
        /// <summary>
        /// Exit card mode
        /// </summary>
        public void ExitCardMode()
        {
            if (!isCardModeActive) return;
            
            // Deselect current card
            if (currentSelectedCard != null)
            {
                // Explicitly reset card state
                currentSelectedCard.isSelected = false;
                currentSelectedCard.Deselect();
                currentSelectedCard = null;
            }
            
            isCardModeActive = false;
            
            OnCardModeChanged?.Invoke(false);
            
            Debug.Log("Exited card mode - resetting all card states");
        }
        
        /// <summary>
        /// Check if we're in card mode
        /// </summary>
        /// <returns>True if card mode is active</returns>
        public bool IsCardModeActive()
        {
            bool result = isCardModeActive && currentSelectedCard != null;
            Debug.Log($"IsCardModeActive check: isCardModeActive={isCardModeActive}, currentSelectedCard={currentSelectedCard != null}, result={result}");
            return result;
        }
        
        /// <summary>
        /// Get the currently selected card in card mode
        /// </summary>
        /// <returns>Currently selected card or null</returns>
        public ChessCardHoverEffect GetSelectedCard()
        {
            return isCardModeActive ? currentSelectedCard : null;
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Editor context menu to update all card audio settings for testing
        /// </summary>
        [ContextMenu("Update All Card Audio Settings")]
        private void EditorUpdateAllCardAudioSettings()
        {
            UpdateAllCardAudioSettings();
        }
        
        /// <summary>
        /// Debug all card hover effects in the scene
        /// </summary>
        [ContextMenu("Debug All Cards")]
        private void EditorDebugAllCards()
        {
            ChessCardHoverEffect[] allCardEffects = FindObjectsOfType<ChessCardHoverEffect>();
            Debug.Log($"Found {allCardEffects.Length} card hover effects in scene:");
            
            for (int i = 0; i < allCardEffects.Length; i++)
            {
                var card = allCardEffects[i];
                Debug.Log($"  {i}: {card.gameObject.name} - Player: {card.playerIndex}, " +
                         $"Card: {card.cardData?.cardName ?? "NULL"}, " +
                         $"CanActivate: {card.canBeActivated}, " +
                         $"Activated: {card.isActivated}, " +
                         $"Selected: {card.isSelected}");
            }
        }
        
        /// <summary>
        /// Editor context menu to setup player 1 cards for testing
        /// </summary>
        [ContextMenu("Setup Player 1 Cards")]
        private void EditorSetupPlayer1Cards()
        {
            ChessCardHoverEffect[] allCardEffects = FindObjectsOfType<ChessCardHoverEffect>();
            List<ChessCardHoverEffect> player1Cards = new List<ChessCardHoverEffect>();
            
            foreach (var cardEffect in allCardEffects)
            {
                if (cardEffect.playerIndex == 1)
                {
                    player1Cards.Add(cardEffect);
                }
            }
            
            foreach (var cardEffect in player1Cards)
            {
                if (cardEffect.cardData != null)
                {
                    SetupCardHoverEffect(cardEffect, cardEffect.cardData, 1);
                }
            }
            
            Debug.Log($"Setup {player1Cards.Count} Player 1 cards with audio settings");
        }
        
        /// <summary>
        /// Editor context menu to setup player 2 cards for testing
        /// </summary>
        [ContextMenu("Setup Player 2 Cards")]
        private void EditorSetupPlayer2Cards()
        {
            ChessCardHoverEffect[] allCardEffects = FindObjectsOfType<ChessCardHoverEffect>();
            List<ChessCardHoverEffect> player2Cards = new List<ChessCardHoverEffect>();
            
            foreach (var cardEffect in allCardEffects)
            {
                if (cardEffect.playerIndex == 2)
                {
                    player2Cards.Add(cardEffect);
                }
            }
            
            foreach (var cardEffect in player2Cards)
            {
                if (cardEffect.cardData != null)
                {
                    SetupCardHoverEffect(cardEffect, cardEffect.cardData, 2);
                }
            }
            
            Debug.Log($"Setup {player2Cards.Count} Player 2 cards with audio settings");
        }
        #endif
    }
}