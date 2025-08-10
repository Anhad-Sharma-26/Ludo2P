using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("References")]
    public PathManager pathManager;
    public DiceController diceController;
    
    [Header("UI")]
    public Button rollDiceButton;
    public Text rollDiceButtonText;
    
    [Header("Token Prefabs")]
    public GameObject redTokenPrefab;
    public GameObject blueTokenPrefab;
    
    [Header("Base Spawn Points")]
    public Transform[] redBasePoints;
    public Transform[] blueBasePoints;
    
    [Header("Entry Points")]
    public Transform[] redEntryPoints;
    public Transform[] blueEntryPoints;
    
    [Header("Player Names")]
    public string[] playerNames = new string[] { "Red", "Blue" };
    
    private List<Token> redTokens = new List<Token>();
    private List<Token> blueTokens = new List<Token>();
    
    private int currentPlayer = 0;
    private int lastRoll = 0;
    private bool waitingForSelection = false;
    private List<Token> selectableTokens = new List<Token>();
    
    void Awake()
    {
        if (Instance == null) 
            Instance = this;
        else 
            Destroy(gameObject);
    }
    
    void Start()
    {
        if (rollDiceButton != null)
        {
            rollDiceButton.onClick.RemoveAllListeners();
            rollDiceButton.onClick.AddListener(OnRollDiceButtonPressed);
        }
        
        SpawnTokens();
        UpdateRollButtonText();
        SetRollButtonInteractable(true);
    }
    
    private void SpawnTokens()
    {
        redTokens.Clear();
        blueTokens.Clear();
        
        if (redTokenPrefab != null)
        {
            for (int i = 0; i < redBasePoints.Length; i++)
            {
                Transform spawn = redBasePoints[i];
                GameObject inst = Instantiate(redTokenPrefab, spawn.position, Quaternion.identity);
                Token t = inst.GetComponent<Token>();
                
                if (t != null)
                {
                    t.playerId = 0;
                    t.SetToBase(spawn.position, -1);
                    
                    if (i < redEntryPoints.Length && redEntryPoints[i] != null && 
                        pathManager != null && pathManager.pathPoints != null)
                    {
                        for (int p = 0; p < pathManager.pathPoints.Length; p++)
                        {
                            if (pathManager.pathPoints[p] == redEntryPoints[i])
                            {
                                t.entryPointIndex = p;
                                break;
                            }
                        }
                    }
                    redTokens.Add(t);
                }
                else
                {
                    Destroy(inst);
                }
            }
        }
        
        if (blueTokenPrefab != null)
        {
            for (int i = 0; i < blueBasePoints.Length; i++)
            {
                Transform spawn = blueBasePoints[i];
                GameObject inst = Instantiate(blueTokenPrefab, spawn.position, Quaternion.identity);
                Token t = inst.GetComponent<Token>();
                
                if (t != null)
                {
                    t.playerId = 1;
                    t.SetToBase(spawn.position, -1);
                    
                    if (i < blueEntryPoints.Length && blueEntryPoints[i] != null && 
                        pathManager != null && pathManager.pathPoints != null)
                    {
                        for (int p = 0; p < pathManager.pathPoints.Length; p++)
                        {
                            if (pathManager.pathPoints[p] == blueEntryPoints[i])
                            {
                                t.entryPointIndex = p;
                                break;
                            }
                        }
                    }
                    blueTokens.Add(t);
                }
                else
                {
                    Destroy(inst);
                }
            }
        }
        
        EnableAllTokensInteraction(false);
    }
    
    private void OnRollDiceButtonPressed()
    {
        if (waitingForSelection || AnyTokenMoving())
            return;
        
        SetRollButtonInteractable(false);
        
        if (diceController != null)
        {
            int finalRollNumber = Random.Range(1, 7);
            diceController.StartRollAnimation(finalRollNumber, currentPlayer);
        }
        else
        {
            SetRollButtonInteractable(true);
        }
    }
    
    public void OnDiceRolled(int roll, int playerId)
    {
        if (playerId == -1) 
            playerId = currentPlayer;
        
        if (playerId != currentPlayer)
        {
            currentPlayer = playerId;
            UpdateRollButtonText();
        }
        
        lastRoll = roll;
        DetermineSelectableTokensForCurrentPlayer();
    }
    
    private void SetRollButtonInteractable(bool enabled)
    {
        if (rollDiceButton != null) 
            rollDiceButton.interactable = enabled;
        
        if (rollDiceButtonText != null)
        {
            rollDiceButtonText.text = (currentPlayer == 0 ? "Roll Dice (Red)" : "Roll Dice (Blue)");
        }
    }
    
    private void DetermineSelectableTokensForCurrentPlayer()
    {
        selectableTokens.Clear();
        List<Token> pool = (currentPlayer == 0) ? redTokens : blueTokens;
        
        if (pathManager == null || pathManager.pathPoints == null || pathManager.pathPoints.Length == 0)
        {
            EndTurn();
            return;
        }
        
        int pathLen = pathManager.pathPoints.Length;
        
        foreach (var t in pool)
        {
            if (t == null) continue;
            
            bool canMove = false;
            
            if (t.currentPathIndex == -1)
            {
                if (lastRoll == 6)
                {
                    if (t.entryPointIndex >= 0 && t.entryPointIndex < pathLen && 
                        pathManager.pathPoints[t.entryPointIndex] != null)
                    {
                        canMove = true;
                    }
                }
            }
            else
            {
                int target = (t.currentPathIndex + lastRoll) % pathLen;
                if (pathManager.pathPoints[target] != null)
                {
                    canMove = true;
                }
            }
            
            if (canMove)
            {
                selectableTokens.Add(t);
                t.SetInteractableVisual(true);
                var col = t.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;
            }
            else
            {
                t.SetInteractableVisual(false);
                var col = t.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
            }
        }
        
        if (selectableTokens.Count == 0)
        {
            Debug.Log("No valid moves available");
            EndTurn();
            return;
        }
        
        waitingForSelection = true;
    }
    
    public void SelectToken(Token token)
    {
        if (!waitingForSelection || token == null) return;
        if (token.playerId != currentPlayer) return;
        if (!selectableTokens.Contains(token)) return;
        
        foreach (var t in (currentPlayer == 0 ? redTokens : blueTokens))
        {
            if (t == null) continue;
            t.SetInteractableVisual(false);
            var col = t.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
        
        waitingForSelection = false;
        token.MoveSteps(lastRoll);
    }
    
    public void OnTokenMoveFinished(Token token)
    {
        if (lastRoll == 6)
        {
            Debug.Log("Extra turn for rolling 6");
            SetRollButtonInteractable(true);
        }
        else
        {
            EndTurn();
        }
    }
    
    private void EndTurn()
    {
        waitingForSelection = false;
        selectableTokens.Clear();
        
        currentPlayer = (currentPlayer + 1) % 2;
        UpdateAllTokensInteractState(false);
        
        SetRollButtonInteractable(true);
        UpdateRollButtonText();
    }
    
    private bool AnyTokenMoving()
    {
        foreach (var t in redTokens) 
            if (t != null && t.isMoving) return true;
        
        foreach (var t in blueTokens) 
            if (t != null && t.isMoving) return true;
        
        return false;
    }
    
    private void EnableAllTokensInteraction(bool enable)
    {
        foreach (var t in redTokens)
        {
            if (t == null) continue;
            var col = t.GetComponent<Collider2D>();
            if (col != null) col.enabled = enable;
            t.SetInteractableVisual(false);
        }
        
        foreach (var t in blueTokens)
        {
            if (t == null) continue;
            var col = t.GetComponent<Collider2D>();
            if (col != null) col.enabled = enable;
            t.SetInteractableVisual(false);
        }
    }
    
    private void UpdateAllTokensInteractState(bool enable)
    {
        var list = (currentPlayer == 0) ? redTokens : blueTokens;
        foreach (var t in list)
        {
            if (t == null) continue;
            var col = t.GetComponent<Collider2D>();
            if (col != null) col.enabled = enable;
            t.SetInteractableVisual(false);
        }
    }
    
    private void UpdateRollButtonText()
    {
        if (rollDiceButtonText != null)
        {
            if (currentPlayer == 0)
                rollDiceButtonText.text = "Red's Turn - Roll the Dice";
            else
                rollDiceButtonText.text = "Blue's Turn - Roll the Dice";
        }
    }
}
