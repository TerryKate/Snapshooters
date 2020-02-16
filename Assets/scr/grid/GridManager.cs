using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { set; get; }

    [Header("Grid Settings (Defaults)")]
    public float gridMoveSpeed = 2.5f;
    public int turnDuration = 90;
    public GridUnitCamera gridUnitCamera;

    [Header("Grid Levels")]
    public int currentLevel = 0;
    public List<GameObject> levelPrefabs;

    [HideInInspector]
    public GridLevel level;
    private GameObject levelPrefab;

    public GridUnit[,] GridUnits { set; get; }
    private GridUnit hoveredUnit;
    private GridUnit selectedUnit;    

    [Header("Grid UI")]
    public GridUI gridUI;

    [HideInInspector]
    public bool isPlayerTurn;

    private int selectionX = -1;
    private int selectionY = -1;

    private Coroutine aITurnRoutine;

    [HideInInspector]
    public bool isGameRunning = false;

    private bool isAiMoving = false;
    private bool reloadLevel = false;


    private void Start()
    {
        Instance = this;

        StartGame();
    }

    private void StartGame()
    {
        levelPrefab = Instantiate(levelPrefabs[currentLevel], Vector3.zero, Quaternion.identity) as GameObject;
        levelPrefab.transform.SetParent(transform);

        level = levelPrefab.GetComponent<GridLevel>();

        GridUnits = new GridUnit[level.gridSize, level.gridSize];
        gridUnitCamera.gameObject.SetActive(false);

        isPlayerTurn = false;
        isAiMoving = false;
        isGameRunning = true;

        level.Spawn();
        gridUI.Setup(level.levelName);

        FindObjectOfType<AudioManager>().Play("mainMusic");

        // End Turn 0, give starting actions;
        EndTurn();
    }

    private void Update()
    {
        UpdateDebugGrid();
        
        if (!gridUI.GamePaused())
        {
            UpdateRaycast();
            UpdateHovering();
            UpdateGrid();
            UpdateInputs();
            UpdateProjections();
            UpdateGameRules();
        }        
    }

    private void UpdateGrid()
    {
        if (Input.GetMouseButtonDown(0))
        {
            bool reselect = false;

            // Player clicking on unit occupied tile;
            if (selectionX >= 0 && selectionY >= 0 && isPlayerTurn)
            {
                // Do not select enemy units for now;
                if (selectedUnit == null)
                {                    
                    SelectUnit(selectionX, selectionY);
                }                
                else if (selectedUnit.CurrentX == selectionX && selectedUnit.CurrentY == selectionY)
                {
                    // Clicking on the same selected tile;
                    GridHighlight.Instance.HideHighlights();
                    DeselectUnit();
                }
                else
                {
                    // Moving || Attack selection if possible;
                    if (selectedUnit.moves[selectionX, selectionY] && selectedUnit.CanMove)
                    {
                        MoveUint(selectionX, selectionY);

                        // Select after moving;
                        reselect = selectedUnit.shootAferMove;
                    }

                    // Attacking selection if possible;
                    else if (selectedUnit.actions[selectionX, selectionY] && selectedUnit.CanShoot)
                    {
                        if(!selectedUnit.CanMove)
                        {
                            if (selectedUnit.shootAferMove)
                            {
                                AttackUint(selectionX, selectionY);
                            }
                        }
                        else AttackUint(selectionX, selectionY);
                    }
                    else
                    {
                        // Slecting different unit, while focused on one;
                        reselect = true;
                    }

                    // Deselect and remove the highlights of the used unit;
                    GridHighlight.Instance.HideHighlights();
                    DeselectUnit();
                        
                    // Continues selection;
                    if (reselect)
                    {
                        SelectUnit(selectionX, selectionY);
                    }
                }
            }
        }
    }

    private void UpdateInputs()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            gridUI.PauseGame();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadLevel();
        }
    }

    private void UpdateRaycast()
    {
        selectionX = -1;
        selectionY = -1;

        if (Physics.Raycast
            (
                Camera.main.ScreenPointToRay(Input.mousePosition),
                out RaycastHit hit,
                50.0f,
                LayerMask.GetMask("GameGridPlane"))
            )
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
    }

    private void UpdateHovering()
    {
        GridHighlight.Instance.HighlightTile(selectionX, selectionY, isPlayerTurn);

        if (hoveredUnit && hoveredUnit != selectedUnit)
        {
            hoveredUnit.HideStats();
            hoveredUnit = null;
        }

        if(selectionX != -1 && selectionY != -1 && GridUnits != null)
        {
            if (GridUnits[selectionX, selectionY])
            {
                hoveredUnit = GridUnits[selectionX, selectionY];
                if(hoveredUnit)
                    hoveredUnit.ShowStats();
            }
        }
    }

    private void UpdateProjections()
    {
        GridUnit gUnit;

        if (Input.GetMouseButtonDown(1)) // Show hovered unit actions area;
        {
            if (selectionX != -1 && selectionY != -1)
            {                
                gUnit  = GridUnits[selectionX, selectionY];
                if (gUnit)
                {
                    gUnit.FindPossibleActions();
                    GridHighlight.Instance.HighlightActionArea(gUnit.actionsArea);
                }          
            }
        }
        else if (Input.GetMouseButtonUp(1)) // Hide hovered unit actions area;
        {
            if (selectionX != -1 && selectionY != -1)
            {
                GridHighlight.Instance.HideHighlights(GridHighlight.HIGHLIGHT_AREA);
            }
        }
    }

    private void SelectUnit(int x, int y)
    {
        if (GridUnits[x, y] == null)
        {
            return;
        }

        // Select unit;
        selectedUnit = GridUnits[x, y];
        selectedUnit.FindPossibleMoves();
        selectedUnit.FindPossibleActions();

        GridHighlight.Instance.HighlighUnit(selectedUnit.CurrentX, selectedUnit.CurrentY, selectedUnit.isEnemy);
        gridUnitCamera.target = selectedUnit.transform;
        //gridUnitCamera.gameObject.SetActive(true);
        gridUI.ShowDetails(selectedUnit);

        if (isPlayerTurn && !selectedUnit.isEnemy)
        {           
            if (selectedUnit.CanMove)
            {
                // Hihglight move cells;
                GridHighlight.Instance.HighlightMoves(selectedUnit.moves);
            }
            if (selectedUnit.CanShoot)
            {
                // Hihglight attack cells;
                if (!selectedUnit.CanMove)
                {
                    if (selectedUnit.shootAferMove)
                    {
                        GridHighlight.Instance.HighlightActions(selectedUnit.actions);
                    }
                }
                else GridHighlight.Instance.HighlightActions(selectedUnit.actions);

            }
        }
        else
        {
            GridHighlight.Instance.HighlightMoves(selectedUnit.moves);
        }
    }

    private void DeselectUnit()
    {
        if (selectedUnit)
        {
            GridHighlight.Instance.HideHighlights(GridHighlight.HIGHLIGHT_UNIT);
            selectedUnit.HideStats();
            selectedUnit = null;
        }
        gridUnitCamera.gameObject.SetActive(false);
        gridUI.HideDetails();
    }

    private void MoveUint(int x, int y)
    {
        // Just in case;
        if (selectedUnit.isEnemy && isPlayerTurn) return;
        
        // Detach unit from its current position;
        GridUnits[selectedUnit.CurrentX, selectedUnit.CurrentY] = null;

        // Move unit to desired destination;
        selectedUnit.SetMoveDestination(level.GetTileCenter(x, y));
        selectedUnit.SetPosition(x, y);
        selectedUnit.CanMove = false;

        // Store its new position;
        GridUnits[x, y] = selectedUnit;
    }

    private void AttackUint(int x, int y)
    {
        // Just in case;
        if (selectedUnit.isEnemy && isPlayerTurn) return;

        GridUnit targetedUnit = GridUnits[x, y];

        // Attack sequence;

        if (targetedUnit) //&& !selectedUnit.IsBusy()
        {
            selectedUnit.CanMove = false;
            selectedUnit.CanShoot = false;
            StartCoroutine(Attack(selectedUnit, targetedUnit));
        }
    }

    IEnumerator Attack(GridUnit attacker, GridUnit target)
    {
        while (attacker.IsBusy()) yield return null;

        attacker.LaserTarget(target.gameObject);
        attacker.Attack(target);

        yield return null;
    }

    public void EndTurn()
    {
        // Deny Ending while ai turn is in progress;
        if (isAiMoving || gridUI.ShowingTurn())
        {            
            return;
        }

        // Switch sides;
        isPlayerTurn = !isPlayerTurn;
        gridUI.EndTurn(isPlayerTurn);

        // Hide any selection highlights;
        if (GridHighlight.Instance)
        {
            GridHighlight.Instance.HideHighlights();
        }        

        // Reset attack and move capabilities;
        for (int i = 0; i <= 7; i++)
        {
            for (int j = 0; j <= 7; j++)
            {
                if (GridUnits[i, j])
                {
                    GridUnits[i, j].ResetTurn(isPlayerTurn);
                }                                                             
            }            
        }

        FindObjectOfType<AudioManager>().Play("endTurn");

        if (!isPlayerTurn)
        {
            // Start the AI turn;
            aITurnRoutine = StartCoroutine(AiTurn());            
        }
    }

    IEnumerator AiTurn()
    {
        GridUnit gUnit;

        isAiMoving = true;

        // Simulate A.I. thinking .. ^
        yield return new WaitForSeconds(2);
        if (selectedUnit) DeselectUnit();        

        for (int i = 0; i <= 7; i++)
        {
            for (int j = 0; j <= 7; j++)
            {              
                // Wait for any interaction;
                while (gridUI.GamePaused())
                {
                    yield return null;
                }

                if (GridUnits[i, j])
                {
                    gUnit = GridUnits[i, j];

                    if (gUnit && gUnit.isEnemy && !isPlayerTurn && !gUnit.IsAiMoved)
                    {
                        gUnit.DetermineAiAction();

                        // AI Enemy to trigger it's available actions here;
                        if (gUnit.CanShoot || gUnit.CanMove)
                        {
                            selectedUnit = gUnit;

                            // Attack;
                            if (selectedUnit.CanShoot)
                            {
                                // Wait till unit reaches it's destination;
                                yield return new WaitForSeconds(0.25f);
                                AttackUint(selectedUnit.AITargetX, selectedUnit.AITargetY);
                                yield return new WaitForSeconds(1f);

                                // Wait till unit reaches it's destination;
                                while (selectedUnit.IsBusy()) yield return null;
                            }

                            // Move;
                            if (selectedUnit.CanMove)
                            {                           
                                MoveUint(selectedUnit.AIMoveX, selectedUnit.AIMoveY);
                                yield return new WaitForSeconds(0.75f);

                                while (selectedUnit.IsBusy()) yield return null;

                                // Check if enemy is reaching player 0 line;
                                if (selectedUnit.isEnemy)
                                {
                                    if (selectedUnit.CurrentY == 0)
                                    {
                                        EndGame();
                                        break;
                                    }
                                }  
                                    
                                // Que Attack after move;
                                if (selectedUnit.shootAferMove)
                                {
                                    selectedUnit.DetermineAiAction();

                                    if (selectedUnit.CanShoot)
                                    {
                                        // Wait till unit reaches it's destination;
                                        yield return new WaitForSeconds(0.25f);
                                        AttackUint(selectedUnit.AITargetX, selectedUnit.AITargetY);
                                        yield return new WaitForSeconds(0.75f);

                                        while (selectedUnit.IsBusy()) yield return null;
                                    }
                                }                                                            
                            }
                        }
                        gUnit.IsAiMoved = true;

                    }
                }
            }
        }

        // Leave some time after bot moves;
        yield return new WaitForSeconds(2f);
        isAiMoving = false;

        // Wait for any interaction;
        while (gridUI.GamePaused())
        {
            yield return null;
        }
        EndTurn();        
    }

    private void UpdateDebugGrid()
    {
        Vector3 widthLine = Vector3.right * level.gridSize;
        Vector3 heightLine = Vector3.forward * level.gridSize;

        for(int i = 0; i <= level.gridSize; i++)
        {
            Vector3 start = Vector3.forward * i;
            Debug.DrawLine(start, start+widthLine);
            for (int j = 0; j <= level.gridSize; j++)
            {
                start = Vector3.right * j;
                Debug.DrawLine(start, start + heightLine);
            }
        }

        if(selectionX >= 0 && selectionY >= 0)
        {
            Debug.DrawLine(
                Vector3.forward * selectionY + Vector3.right * selectionX,
                Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1)
                    );
        }
    }

    private void UpdateGameRules()
    {        
        if(isGameRunning)
        {
            if(level.HasWinner())
            {
                EndGame(level.EnemyDefeated());
            }                       
        }
        else
        {
            if(reloadLevel)
            {
                if (levelPrefab)
                {
                    Destroy(levelPrefab);
                }
                reloadLevel = false;
            }
            else if (!level)
            {
                StartGame();
            }
        }
    }

    public void EndGame(bool isVictory = false, bool isCheatTrigger = false)
    {
        isGameRunning = false;

        // Deselect, and stop AI if moving;
        DeselectUnit();

        // Remove any highlighted units;
        GridHighlight.Instance.HideHighlights();

        if (aITurnRoutine != null)
        {
            StopCoroutine(aITurnRoutine);
        }


        // Stop the main theme;
        FindObjectOfType<AudioManager>().Pause("mainMusic");

        if (!isCheatTrigger)
        {          
            // Destroy units on the map if we are not reseting;
            level.EndGame(isVictory);

            // Play some sfx here;
            FindObjectOfType<AudioManager>().Play(isVictory ? "victory" : "defeat");
        }
        
        gridUI.EndGame(isVictory);
    }

    public void SetNextLevel()
    {
        currentLevel++;

        if(currentLevel >= levelPrefabs.Count)
        {
            currentLevel = 0;
        }
        EndGame(false, true);
        reloadLevel = true;
    }

    public void ReloadLevel()
    {
        EndGame(false, true);
        reloadLevel = true;
    }

    public int GetGridLength()
    {
        return level.gridSize;
    }
}
