using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Token : MonoBehaviour
{
    [HideInInspector] public int playerId = -1;
    [HideInInspector] public int currentPathIndex = -1;
    [HideInInspector] public int entryPointIndex = -1;
    [HideInInspector] public bool isMoving = false;
    
    [SerializeField] private float stepMoveDuration = 0.18f;
    [SerializeField] private float liftHeight = 0.15f;
    
    private Collider2D tokenCollider;
    
    void Awake()
    {
        tokenCollider = GetComponent<Collider2D>();
    }
    
    public void SetToBase(Vector3 basePosition, int pathIndex)
    {
        transform.position = basePosition;
        currentPathIndex = pathIndex;
        if (tokenCollider != null) 
            tokenCollider.enabled = true;
    }
    
    public void MoveSteps(int steps)
    {
        if (isMoving) return;
        StartCoroutine(MoveStepsCoroutine(steps));
    }
    
    private IEnumerator MoveStepsCoroutine(int steps)
    {
        var gameManager = GameManager.Instance;
        if (gameManager?.pathManager?.pathPoints == null || gameManager.pathManager.pathPoints.Length == 0)
        {
            if (GameManager.Instance != null) 
                GameManager.Instance.OnTokenMoveFinished(this);
            yield break;
        }
        
        int pathLength = gameManager.pathManager.pathPoints.Length;
        
        if (currentPathIndex == -1)
        {
            if (entryPointIndex >= 0 && entryPointIndex < pathLength)
            {
                Vector3 entryPosition = gameManager.pathManager.pathPoints[entryPointIndex].position;
                if (tokenCollider != null) 
                    tokenCollider.enabled = false;
                
                yield return StartCoroutine(MoveToPoint(entryPosition));
                currentPathIndex = entryPointIndex;
                
                if (tokenCollider != null) 
                    tokenCollider.enabled = true;
            }
            else
            {
                Debug.Log("Cannot leave base - invalid entry point");
                if (GameManager.Instance != null) 
                    GameManager.Instance.OnTokenMoveFinished(this);
                yield break;
            }
        }
        
        isMoving = true;
        
        for (int step = 0; step < steps; step++)
        {
            int nextIndex = (currentPathIndex + 1) % pathLength;
            Transform nextPosition = gameManager.pathManager.pathPoints[nextIndex];
            
            if (nextPosition == null)
            {
                Debug.Log("Move exceeds valid path");
                break;
            }
            
            if (tokenCollider != null) 
                tokenCollider.enabled = false;
            
            yield return StartCoroutine(MoveToPoint(nextPosition.position));
            currentPathIndex = nextIndex;
            yield return new WaitForSeconds(0.02f);
            
            if (tokenCollider != null) 
                tokenCollider.enabled = true;
        }
        
        isMoving = false;
        if (tokenCollider != null) 
            tokenCollider.enabled = true;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTokenMoveFinished(this);
        }
    }
    
    private IEnumerator MoveToPoint(Vector3 destination)
    {
        float duration = stepMoveDuration > 0f ? stepMoveDuration : 0.18f;
        Vector3 startPos = transform.position;
        Vector3 liftedStart = startPos + Vector3.up * liftHeight;
        Vector3 liftedDest = destination + Vector3.up * liftHeight;
        
        float liftTime = duration * 0.2f;
        float t = 0f;
        while (t < liftTime)
        {
            float progress = t / liftTime;
            transform.position = Vector3.Lerp(startPos, liftedStart, progress);
            t += Time.deltaTime;
            yield return null;
        }
        
        float glideTime = duration * 0.6f;
        t = 0f;
        while (t < glideTime)
        {
            float progress = t / glideTime;
            transform.position = Vector3.Lerp(liftedStart, liftedDest, progress);
            t += Time.deltaTime;
            yield return null;
        }
        
        float dropTime = duration * 0.2f;
        t = 0f;
        while (t < dropTime)
        {
            float progress = t / dropTime;
            transform.position = Vector3.Lerp(liftedDest, destination, progress);
            t += Time.deltaTime;
            yield return null;
        }
        
        transform.position = destination;
    }
    
    public void SetInteractableVisual(bool enable)
    {
    }
    
    private void OnMouseDown()
    {
        if (tokenCollider == null || !tokenCollider.enabled) return;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SelectToken(this);
        }
    }
}
