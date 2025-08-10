using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DiceController : MonoBehaviour
{
    [Header("Dice Display")]
    [SerializeField] private SpriteRenderer diceRenderer;
    [SerializeField] private Image diceImage;
    
    [Header("Dice Faces")]
    [SerializeField] private Sprite[] diceFaceSprites;
    
    private Coroutine rollCoroutine;
    
    void Start()
    {
        if (diceFaceSprites != null && diceFaceSprites.Length >= 1)
        {
            Sprite firstFace = diceFaceSprites[0];
            if (diceImage != null && diceImage.sprite == null) 
                diceImage.sprite = firstFace;
            if (diceRenderer != null && diceRenderer.sprite == null) 
                diceRenderer.sprite = firstFace;
        }
    }
    
    public void StartRollAnimation(int finalRoll, int playerId)
    {
        if (rollCoroutine != null)
            StopCoroutine(rollCoroutine);
        rollCoroutine = StartCoroutine(RollAnimation(finalRoll, playerId));
    }
    
    public void StartRollAnimation(int finalRoll)
    {
        StartRollAnimation(finalRoll, -1);
    }
    
    private IEnumerator RollAnimation(int finalRoll, int playerId)
    {
        for (int i = 0; i < 15; i++)
        {
            int randomFace = Random.Range(1, 7);
            UpdateDiceFace(randomFace);
            yield return new WaitForSeconds(0.05f);
        }
        
        UpdateDiceFace(finalRoll);
        rollCoroutine = null;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDiceRolled(finalRoll, playerId);
        }
    }
    
    private void UpdateDiceFace(int faceNumber)
    {
        if (diceFaceSprites == null || diceFaceSprites.Length < 6)
            return;
        
        int index = Mathf.Clamp(faceNumber - 1, 0, diceFaceSprites.Length - 1);
        Sprite face = diceFaceSprites[index];
        
        if (diceImage != null)
        {
            diceImage.sprite = face;
            return;
        }
        
        if (diceRenderer != null)
        {
            diceRenderer.sprite = face;
        }
    }
}
