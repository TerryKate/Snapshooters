using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GridUnit : MonoBehaviour
{    
    [Header("Unit Settings")]
    public string unitName;
    public float health;
    public float damage;
    public int moveRange = 1;
    public int attackRange = 1;
    public bool shootAferMove;

    public bool[,] moves;
    public bool[,] actions;
    public bool[,] actionsArea;

    [Header("Componenets")]
    public LineRenderer lineRenderer;
    public Transform laserPoint;
    public GameObject PartToRotate;
    public GridStatsUI stats;
    public Animator animator;
    public GameObject destroyEffect;
    public GameObject shootEffect;

    [Header("Optional")]
    public LineRenderer secondRenderer;
    public Transform laserSecondPoint;

    [HideInInspector]
    public bool isEnemy;

    [HideInInspector]
    public float CurrentHealth { set; get; }

    public int CurrentX { set; get; }
    public int CurrentY { set; get; }

    public bool CanMove { set; get; }
    public bool CanShoot { set; get; }

    public bool IsAiMoved { set; get; }
    
    public int AITargetX { set; get; }
    public int AITargetY { set; get; }

    public int AIMoveX { set; get; }
    public int AIMoveY { set; get; }

    public int AIPrefferedMoveX { set; get; }
    public int AIPrefferedMoveY { set; get; }

    private bool isMoving;
    private bool isRotating;
    private bool isAttacking;

    private int laserFrameCount;
    private Vector3 targetPoint;
    private Quaternion targetRotation;
    private Vector3 moveDestination;
    private Vector3 laserTarget;
    private Vector3 PartToRotateTransform;
    private GameObject effectInstance;

    private static readonly string IS_MOVING = "isMoving";
    private static readonly string IS_SHOOTING = "isShooting";

    [HideInInspector]
    public int gridSize;

    private void Start()
    {
        gridSize = GridManager.Instance.GetGridLength();
        PartToRotateTransform = Vector3.zero;
        moveDestination = transform.position;
        lineRenderer.enabled = false;
        isMoving = false;
        isRotating = false;
        isAttacking = false;

        if(secondRenderer)
        {
            secondRenderer.enabled = false;
        }
    }

    public void Setup(int x, int y, bool enemy)
    {
        SetPosition(x,y);

        isEnemy = enemy;
        CurrentHealth = health;

        stats.gameObject.SetActive(false);
        stats.SetHealth(health);

        AIPrefferedMoveX = -1;
        AIPrefferedMoveY = -1;
    }

    private void Update()
    {
        UpdateUnitFocus();
        UpdateMovement();
        UpdateLasers();
        UpdateStats();
    }

    public void UpdateUnitFocus()
    {
        isRotating = false;

        // Prepare rotate to move;
        if (moveDestination != transform.position || attackTartget)
        {
            targetPoint = new Vector3(moveDestination.x, PartToRotateTransform.y, moveDestination.z) - PartToRotateTransform;            
            targetRotation = Quaternion.LookRotation(attackTartget ? -targetPoint: targetPoint);

            if (!Mathf.Approximately(Mathf.Abs(Quaternion.Dot(PartToRotate.transform.rotation, targetRotation)), 1.0f))
            {
                isRotating = true;
                PartToRotate.transform.rotation =
                    Quaternion.Slerp(PartToRotate.transform.rotation, targetRotation, Time.deltaTime * 8.0f);
            }
            else if (attackTartget)
            {
                animator.SetTrigger(IS_SHOOTING);
                attackTartget.TakeDamage(damage);
                attackTartget = null;
                isAttacking = false;
            }
        }
    }

    private void UpdateMovement()
    {
        // If the object is not at the target destination
        if (moveDestination != transform.position && !isRotating)
        {
            // Calculate the next position
            float delta = GridManager.Instance.gridMoveSpeed * Time.deltaTime;

            Vector3 nextPosition = Vector3.MoveTowards(transform.position, moveDestination, delta);

            // Move the object to the next position
            transform.position = nextPosition;

            // Keep shooting if user moves and fires while moving;
            if (laserFrameCount > 0)
            {
                laserFrameCount++;
            }
            isMoving = true;
        }
        else isMoving = false;

        PlayAnim(IS_MOVING, moveDestination != transform.position);
    }

    private void PlayAnim(string anim, bool value)
    {
        if (HasParameter(anim, animator))
        {
            animator.SetBool(anim, value);
        }
        else
        {
            Debug.Log(unitName + " is missing animation bool: " + anim);
        }
    }

    public static bool HasParameter(string paramName, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    private void UpdateLasers()
    {
        // Well yeah, i am not proud witht that "laser";
        if (laserFrameCount >= 1 && !isRotating)
        {
            isAttacking = true;

            laserFrameCount--;

            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, laserPoint.position);
            lineRenderer.SetPosition(1, laserTarget);

            if (secondRenderer)
            {
                secondRenderer.enabled = true;
                secondRenderer.SetPosition(0, laserSecondPoint.position);
                secondRenderer.SetPosition(1, laserTarget);
            }

            if (laserFrameCount <= 0)
            {
                lineRenderer.enabled = false;
                lineRenderer.SetPosition(0, new Vector3(0, 0));
                lineRenderer.SetPosition(1, new Vector3(0, 0));

                if (secondRenderer)
                {
                    secondRenderer.enabled = false;
                    secondRenderer.SetPosition(0, new Vector3(0, 0));
                    secondRenderer.SetPosition(1, new Vector3(0, 0));
                }
            }
        }
        else isAttacking = false;
    }

    private void UpdateStats()
    {
        // Keep unit stats facing the main camera;
        if (stats != null)
        {
            Camera camera = Camera.main;

            stats.transform.LookAt(stats.transform.position + camera.transform.rotation * Vector3.back, camera.transform.rotation * Vector3.up);
        }
    }

    public void LaserTarget(GameObject t)
    {
        laserTarget = new Vector3(t.transform.position.x, 0.5f, t.transform.position.z);
        laserFrameCount = 20;
    }

    public void ShowStats()
    {
        stats.SetHealth(CurrentHealth / health);
        stats.gameObject.SetActive(true);
    }

    public void HideStats()
    {
        stats.gameObject.SetActive(CurrentHealth != health);
    }

    public void ResetTurn(bool isPlayer)
    {
        // Only the player gets auto action;
        CanMove = isPlayer;
        CanShoot = isPlayer;       

        // Ai must determine it's action before moved;
        IsAiMoved = false;
    }

    public void ProjectPath(Vector2 direction, bool target = false, bool prefered = false)
    {
        GridUnit gUnit;

        int tempX = CurrentX;
        int tempY = CurrentY;

        int steps = target ? attackRange : moveRange;

        for (int i = 1; i <= steps; i++)
        {
            // Turning the movement for enemy units here;
            tempX = tempX + (int)(isEnemy ? -direction.x : direction.x);
            tempY = tempY + (int)(isEnemy ? -direction.y : direction.y);

            if (tempX >= 0 && tempX <= 7 && tempY >= 0 && tempY <= 7)
            {
                gUnit = GridManager.Instance.GridUnits[tempX, tempY];

                if(target)
                {
                    if(gUnit)
                    {
                        // Cover units blocking shooting;
                        if (gUnit.isEnemy != isEnemy)
                        {
                            actionsArea[tempX, tempY] = true;
                        }                       
                    }
                    else
                    {
                        // Add to shooting range;
                        actionsArea[tempX, tempY] = true;
                    }

                }
                
                if (!gUnit)
                {
                    if(!target)
                    {
                        // Tile is empty, adding possible move;
                        moves[tempX, tempY] = true;

                        if(prefered)
                        {
                            AIPrefferedMoveX = tempX;
                            AIPrefferedMoveY = tempY;
                        }
                    }
                }
                else
                {
                    if(target)
                    {                        
                        if (GridManager.Instance.isPlayerTurn && gUnit.isEnemy)
                        {
                            // Enemy target found;
                            actions[tempX, tempY] = true;
                        }
                        else if (!GridManager.Instance.isPlayerTurn && !gUnit.isEnemy)
                        {
                            // Player target found;
                            actions[tempX, tempY] = true;
                        }                        
                    }
                    break;
                }
            }
        }
    }

    public void DetermineAiAction()
    {
        FindPossibleMoves();
        FindPossibleActions();

        int _currentX = CurrentX;
        int _currentY = CurrentY;

        bool targetFound = false;

        GetAiMove();
        GetAiTarget();

        if (!CanShoot && CanMove)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (targetFound) break;

                for (int x = 0; x < gridSize; x++)
                {
                    // Look for possible target after move;
                    if (moves[x, y])
                    {
                        CurrentX = x;
                        CurrentY = y;
                        FindPossibleActions();
                        GetAiTarget();

                        if (CanShoot)
                        {
                            AIMoveX = x;
                            AIMoveY = y;
                            CanShoot = false;
                            targetFound = true;
                            break;
                        }
                    }
                }
            }
        }
        CurrentX = _currentX;
        CurrentY = _currentY;
    }

    private void GetAiMove()
    {
        if(AIPrefferedMoveX != -1 && AIPrefferedMoveY != -1)
        {
            // Set the preffered AI move [Attack/Move];
            AIMoveX = AIPrefferedMoveX;
            AIMoveY = AIPrefferedMoveY;

            CanMove = true;
        }
        else
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    // If there ware no preffered move get the default one [forward];
                    if (moves[x, y])
                    {
                        AIMoveX = x;
                        AIMoveY = y;

                        CanMove = true;

                        break;
                    }
                }
            }
        }
    }

    private void GetAiTarget()
    {
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // Default get the first possible;
                if (actions[x, y])
                {
                    AITargetX = x;
                    AITargetY = y;

                    CanShoot = true;
                }
            }
        }
    }

    public void SetPosition(int x, int y)
    {
        CurrentX = x;
        CurrentY = y;
    }

    public void SetMoveDestination(Vector3 value)
    {
        moveDestination = value;
        PartToRotateTransform = new Vector3(PartToRotate.transform.position.x, PartToRotate.transform.position.y, PartToRotate.transform.position.z);
  
    }

    private GridUnit attackTartget;
    public void Attack(GridUnit gUnit)
    {
        attackTartget = gUnit;
        PartToRotateTransform = new Vector3(attackTartget.transform.position.x, attackTartget.transform.position.y, attackTartget.transform.position.z);
    }

    public bool IsBusy()
    {
        return (isMoving || isAttacking || isRotating);
    }

    public virtual bool[,] FindPossibleMoves()
    {
        return new bool[gridSize, gridSize];
    }

    public virtual bool[,] FindPossibleActions()
    {
        return new bool[gridSize, gridSize];
    }

    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;

        if(CurrentHealth <= 0)
        {
            if(destroyEffect)
            {
                GridManager.Instance.level.DestroyUnit(this);
                FindObjectOfType<AudioManager>().Play("destroy");
                effectInstance = Instantiate(destroyEffect, transform.position, transform.rotation);
                Destroy(effectInstance, effectInstance.GetComponent<ParticleSystem>().main.duration);
            }            
        }
        else
        {
            FindObjectOfType<AudioManager>().Play("hit");
            effectInstance = Instantiate(shootEffect ? shootEffect : destroyEffect, transform.position, transform.rotation);
            Destroy(effectInstance, effectInstance.GetComponent<ParticleSystem>().main.duration);
        }

        ShowStats();
    }

    public bool IsAlive()
    {
        return CurrentHealth > 0;
    }
}
