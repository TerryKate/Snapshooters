using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLevel : MonoBehaviour
{
    [Header("Level Settings (Defaults)")]
    public string levelName;
    public int gridSize = 8;
    public int gridTileSize = 1;

    [Header("Level Units")]
    public List<GameObject> playerUnitPrefabs;
    public List<GameObject> enemyUnitPrefabs;

    private int playerUnitsLeft = -1;
    private int enemyUnitsLeft = -1;

    [HideInInspector]
    public List<GameObject> activePrefabs;

    private void Start()
    {
        // TODO y = -0.01f;
    }
    public void Spawn()
    {
        playerUnitsLeft = 0;
        enemyUnitsLeft = 0;

        for (int i = playerUnitPrefabs.Count - 1; i >= 0; i--)
        {
            SpawUnit(playerUnitPrefabs, i, 1 + i, 0);
        }

        for (int i = enemyUnitPrefabs.Count - 1; i >= 0; i--)
        {
            SpawUnit(enemyUnitPrefabs, i, 1, 6, true);
        }



        Vector3 levelPos = transform.position;
        levelPos.y = -0.02f;
        transform.position = levelPos;
    }

    private void SpawUnit(List<GameObject> prefabs, int index, int x, int y, bool isEnemy = false)
    {
        Quaternion unitOrientation = isEnemy ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
        Vector3 spawnPosition = GetSpawnPosition(isEnemy);

        GameObject unit = Instantiate(prefabs[index], spawnPosition, unitOrientation) as GameObject;
        unit.transform.SetParent(GridManager.Instance.transform);

        GridUnit gUnit = unit.GetComponent<GridUnit>();
        gUnit.animator = unit.GetComponent<Animator>();

        GridManager.Instance.GridUnits[(int)spawnPosition.x, (int)spawnPosition.z] = gUnit;
        gUnit.Setup((int)spawnPosition.x, (int)spawnPosition.z, isEnemy);

        // Simple tracking of units on the grid;
        if (isEnemy)  enemyUnitsLeft++;
        else playerUnitsLeft++;
        
        activePrefabs.Add(unit);
    }

    private Vector3 GetSpawnPosition(bool isEnemy)
    {
        Vector3 vec3;

        int xMinCap = isEnemy ? 1 : 0;
        int xMaxCap = gridSize; // isEnemy ? 8 : 8;

        int yMinCap = isEnemy ? gridSize-4 : 0;
        int yMaxCap = isEnemy ? gridSize   : 2;

        int x = Random.Range(xMinCap, xMaxCap);
        int y = Random.Range(yMinCap, yMaxCap);

        vec3 = GetTileCenter(x,y);

        while (GridManager.Instance.GridUnits[x, y])
        {
            x = Random.Range(xMinCap, xMaxCap);
            y = Random.Range(yMinCap, yMaxCap);
            vec3 = GetTileCenter(x, y);
        }                    

        return vec3;
    }

    public Vector3 GetTileCenter(int x, int y)
    {
        Vector3 pos = Vector3.zero;

        pos.x += (gridTileSize * x) + (float)gridTileSize / 2;
        pos.z += (gridTileSize * y) + (float)gridTileSize / 2;
        pos.y = 0;

        return pos;
    }

    public void OnDestroy()
    {
        GameObject prefab;
        GridUnit gUnit;

        // Destroy current level units;
        for (int i = activePrefabs.Count - 1; i >= 0; i--)
        {
            prefab = activePrefabs[i];
            gUnit = prefab.GetComponent<GridUnit>();
            GridManager.Instance.GridUnits[gUnit.CurrentX, gUnit.CurrentY] = null;                    
            activePrefabs.Remove(prefab.gameObject);
            Destroy(prefab.gameObject);
        }
    }

    public void DestroyUnit(GridUnit gUnit)
    {
        activePrefabs.Remove(gUnit.gameObject);
        Destroy(gUnit.gameObject);
        GridManager.Instance.GridUnits[gUnit.CurrentX, gUnit.CurrentY] = null;

        // Simple tracking of units on the grid;
        if (gUnit.isEnemy)
        {
            enemyUnitsLeft--;
        }
        else playerUnitsLeft--;
    }

    public void EndGame(bool isPlayerWin)
    {
        GameObject prefab;
        GridUnit gUnit;

        for (int i = activePrefabs.Count - 1; i >= 0; i--)
        {
            prefab = activePrefabs[i];
            gUnit = prefab.GetComponent<GridUnit>();

            if(gUnit.isEnemy == isPlayerWin)
            {
                gUnit.TakeDamage(gUnit.CurrentHealth);
            }
        }
    }

    public bool HasWinner()
    {
        return enemyUnitsLeft <= 0 || playerUnitsLeft <= 0;
    }

    public bool EnemyDefeated()
    {
        return enemyUnitsLeft <= 0;
    }
}
