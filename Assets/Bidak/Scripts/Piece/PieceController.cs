using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Added for List

public class PieceController : MonoBehaviour
{
    [Header("Piece Data")]
    public ChessPieceData pieceData;

    public int selectedMaterialIndex = 0;
    public Color playerType = Color.white;

    [Header("Component References")]
    public PieceMovementController movementController;
    public PieceAnimationController animationController;
    public PieceEffectController effectController;
    public PieceSoundsController soundsController;

    [Header("Piece Body")]
    public GameObject pieceBodyObject;

    [Header("State")]
    public bool isSelected = false;
    public bool isCaptured = false;

    private void Awake()
    {
        if (movementController == null)
            movementController = GetComponent<PieceMovementController>();
        if (animationController == null)
            animationController = GetComponent<PieceAnimationController>();
        if (effectController == null)
            effectController = GetComponent<PieceEffectController>();
        if (soundsController == null)
            soundsController = GetComponent<PieceSoundsController>();
    }

    private void Start()
    {
        // Find PieceBody child
        pieceBodyObject = FindPieceBodyChild();

        if (pieceData == null)
        {
            Debug.LogWarning("No piece data assigned to PieceController");
        }
        else
        {
            if (animationController != null)
            {
                animationController.pieceData = pieceData;
                animationController.pieceController = this;

                // Hide piece body before spawn effect
                if (pieceBodyObject != null)
                {
                    pieceBodyObject.SetActive(false);
                }

                animationController.SpawnPiece();
                if (soundsController != null)
                {
                    soundsController.pieceData = pieceData;
                    soundsController.PlaySpawnSound();
                }
            }

            selectedMaterialIndex = pieceData.selectedMaterialIndex;
            playerType = pieceData.playerType;
        }
    }

    private void Update()
    {
        if (pieceData != null)
        {
            Debug.Log("Updating piece appearance for piece: " + pieceData.pieceName);
            if (pieceData.selectedMaterialIndex != selectedMaterialIndex)
            {
                pieceData.selectedMaterialIndex = selectedMaterialIndex;
                UpdatePieceAppearance();
            }

            if (pieceData.playerType != playerType)
            {
                pieceData.playerType = playerType;
            }
        }
        
        // Removed old pieceColor update logic
    }

    private void UpdatePieceAppearance()
    {
        if (pieceData == null) return;

        // // Update main renderer
        // Renderer mainRenderer = GetComponent<Renderer>();
        // if (mainRenderer != null)
        // {
        //     if (pieceData.availableMaterials[pieceData.selectedMaterialIndex] != null)
        //     {
        //         mainRenderer.material = pieceData.availableMaterials[pieceData.selectedMaterialIndex];
        //     }
        // }

        // Update child objects with PieceBody tag
        UpdateChildObjectMaterials();
    }

    private void UpdateChildObjectMaterials()
    {
        if (pieceData == null || pieceData.availableMaterials[pieceData.selectedMaterialIndex] == null) return;
        // Find all child objects with PieceBody tag
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in allChildren)
        {
            if (child.CompareTag("PieceBody"))
            {
                // Ambil semua Renderer di dalam child tersebut, termasuk nested
                Renderer[] renderers = child.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer renderer in renderers)
                {
                    // Ganti semua material di renderer ini
                    Material[] newMats = new Material[renderer.materials.Length];
                    for (int i = 0; i < newMats.Length; i++)
                    {
                        newMats[i] = pieceData.availableMaterials[pieceData.selectedMaterialIndex];
                    }
                    renderer.materials = newMats;
                }
            }
        }
    }

    public void SetPieceColor(Color color)
    {
        // Removed pieceColor logic
        UpdatePieceAppearance();
    }

    private GameObject FindPieceBodyChild()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("PieceBody"))
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public void ShowPieceBody()
    {
        if (pieceBodyObject != null)
        {
            pieceBodyObject.SetActive(true);
        }
    }

    public void SetPieceData(ChessPieceData data)
    {
        pieceData = data;
        UpdatePieceAppearance();

        if (animationController != null)
        {
            animationController.pieceData = data;
        }
    }

    public void Capture()
    {
        isCaptured = true;

        if (animationController != null)
        {
            animationController.StartCapturing();
            animationController.SetCapturedPiece();
        }

        StartCoroutine(DisablePieceAfterDelay());
    }

    private IEnumerator DisablePieceAfterDelay()
    {
        yield return new WaitForSeconds(pieceData.deathAnimationSpeed);
        gameObject.SetActive(false);
    }

    public void ToggleSelection()
    {
        isSelected = !isSelected;
        transform.localScale = isSelected ? Vector3.one * 1.2f : Vector3.one;
    }

    public void SetPlayerType(Color playerType)
    {
        if (pieceData != null)
        {
            pieceData.playerType = playerType;
        }
    }

    public void CycleSelectedMaterial()
    {
        if (pieceData != null)
        {
            pieceData.selectedMaterialIndex = (pieceData.selectedMaterialIndex + 1) % pieceData.availableMaterials.Count;
            UpdatePieceAppearance();
        }
    }

    public void SetMaterial(int index)
    {
        if (pieceData != null)
        {
            pieceData.selectedMaterialIndex = index;
            UpdatePieceAppearance();
        }
    }
}