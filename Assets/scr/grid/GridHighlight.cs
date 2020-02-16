using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridHighlight : MonoBehaviour
{
    public static GridHighlight Instance { set; get; }

    public GameObject highlightTilePrefab;
    public GameObject highlightUnitPrefab;
    public GameObject highlightMovePrefab; 
    public GameObject highlightActionPrefab;
    public GameObject highlightEnemyPrefab;
    public GameObject highlightAreaActionPrefab;

    public static string HIGHLIGHT_TILE = "GridTileHighlight";
    public static string HIGHLIGHT_UNIT = "GridUnitHightlight";
    public static string HIGHLIGHT_ENEMY = "GridEnemyHighlight";
    public static string HIGHLIGHT_AREA = "GridActionAreaHighlight";
    public static string HIGHLIGHT_ACTION = "GridActionHighlight";
    public static string HIGHLIGHT_MOVE = "GridMoveHighlight";

    private List<GameObject> highlights;
    private GameObject highlightTile;


    private void Start()
    {
        Instance = this;
        highlights = new List<GameObject>();
        highlightTile = GetHighlightObject(highlightTilePrefab, HIGHLIGHT_TILE);
        highlights.Add(highlightTile);
    }

    public void HighlightTile(int x, int y, bool isPlayer)
    {
        if (x != -1 && y != -1 && isPlayer)
        {
            highlightTile.SetActive(true);
            highlightTile.transform.position = new Vector3(x + 0.5f, 0, y + 0.5f);
        }
        else HideHighlights(HIGHLIGHT_TILE);
    }

    public void HighlighUnit(int x, int y, bool isEnemy)
    {
        GameObject go = GetHighlightObject(isEnemy ? highlightEnemyPrefab : highlightUnitPrefab, isEnemy ? HIGHLIGHT_ENEMY :  HIGHLIGHT_UNIT);
        go.SetActive(true);
        go.transform.position = new Vector3(x + 0.5f, 0, y + 0.5f);
    }

    public void HighlightActionArea(bool[,] actions)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (actions[i, j])
                {
                    GameObject go = GetHighlightObject(highlightAreaActionPrefab, HIGHLIGHT_AREA);
                    go.SetActive(true);
                    go.transform.position = new Vector3(i + 0.5f, 0, j + 0.5f);
                }
            }
        }
    }

    public void HighlightMoves(bool[,] moves)
    {
        for(int i=0; i<8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if(moves[i,j])
                {
                    GameObject go = GetHighlightObject(highlightMovePrefab, HIGHLIGHT_MOVE);
                    go.SetActive(true);
                    go.transform.position = new Vector3(i+0.5f, 0, j + 0.5f);
                }
            }
        }
    }

    public void HighlightActions(bool[,] actions)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (actions[i, j])
                {
                    GameObject go = GetHighlightObject(highlightActionPrefab, HIGHLIGHT_ACTION);  
                    go.SetActive(true);
                    go.transform.position = new Vector3(i + 0.5f, 0, j + 0.5f);
                }
            }
        }
    }

    private GameObject GetHighlightObject(GameObject prefab, string highlightTag)
    {
        GameObject go = highlights.Find(g => !g.activeSelf && g.tag == highlightTag);

        if (go == null) // || go.gameObject"
        {
            go = Instantiate(prefab);
            go.tag = highlightTag;
            go.transform.SetParent(GridManager.Instance.transform);
            highlights.Add(go);
        }

        return go;
    }

    public void HideHighlights(string tag)
    {
        foreach (GameObject go in highlights)
        {
            if(go.tag == tag)
            {
                go.SetActive(false);
            }            
        }
    }

    public void HideHighlights()
    {
        foreach(GameObject go in highlights )
        {
            if (go)
                go.SetActive(false);
        }
    }
}
