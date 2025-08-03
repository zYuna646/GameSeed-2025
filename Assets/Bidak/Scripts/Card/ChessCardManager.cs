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

        // Events for card-related actions
        public event Action<int, ChessCardData> OnCardActivated;
        public event Action<int, ChessCardData> OnCardDeactivated;
        public event Action<int, ChessCardData> OnCardAddedToStorage;
        public event Action<int, ChessCardData> OnCardRemovedFromStorage;
        public event Action<int> OnPlayerCardsChanged;

        // Tracking for changes
        private List<ChessCardData> player1LastActiveCards = new List<ChessCardData>();
        private List<ChessCardData> player2LastActiveCards = new List<ChessCardData>();

        private void Update()
        {
            // Check for changes in Player 1 active cards
            CheckForCardChanges(1, player1ActiveCards, player1LastActiveCards);

            // Check for changes in Player 2 active cards
            CheckForCardChanges(2, player2ActiveCards, player2LastActiveCards);
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
    }
}