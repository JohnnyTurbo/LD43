using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour {

    public static BoardController instance;

    public GameObject gridUnitPrefab;
    //public Transform gridParent;
    public int gridSizeX, gridSizeY;
    public bool isSacrificing = false;

    public GameObject tetrisText;

    GridUnit[,] fullGrid;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        CreateGrid();
        tetrisText.SetActive(false);
    }

    private void CreateGrid()
    {
        fullGrid = new GridUnit[gridSizeX, gridSizeY];

        for(int y = 0; y < gridSizeY; y++)
        {
            for(int x = 0; x < gridSizeX; x++)
            {
                GridUnit newGridUnit = new GridUnit(gridUnitPrefab, transform, x, y);
                fullGrid[x, y] = newGridUnit;
            }
        }
    }

    public bool IsInBounds(Vector2Int coordToTest)
    {
        //Debug.Log("testing pos " + coordToTest.ToString());
        if (coordToTest.x < 0 || coordToTest.x >= gridSizeX || coordToTest.y < 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool IsPosEmpty(Vector2Int coordToTest)
    {
        if(coordToTest.y >= 20)
        {
            return true;
        }

        if(fullGrid[coordToTest.x, coordToTest.y].isOccupied)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void OccupyPos(Vector2Int coords, GameObject tileGO)
    {
        //Debug.Log("occupy: " + coords.ToString());
        fullGrid[coords.x, coords.y].isOccupied = true;
        fullGrid[coords.x, coords.y].tileOnGridUnit = tileGO;
    }

    public void CheckLineClears()
    {
        List<int> linesToClear = new List<int>();
        int consecutiveLineClears = 0;

        for(int y = 0; y < gridSizeY; y++)
        {
            bool lineClear = true;
            for(int x = 0; x < gridSizeX; x++)
            {
                if (!fullGrid[x, y].isOccupied){
                    lineClear = false;
                    consecutiveLineClears = 0;
                }
            }
            if (lineClear)
            {
                linesToClear.Add(y);
                consecutiveLineClears++;
                Debug.Log("consecutive clears: " + consecutiveLineClears);
                if (consecutiveLineClears == 4)
                {
                    ShowTetrisText();
                    Debug.Log("<color=red>T</color>" +
                              "<color=orange>E</color>" +
                              "<color=yellow>T</color>" +
                              "<color=lime>R</color>" +
                              "<color=aqua>I</color>" +
                              "<color=purple>S</color>" +
                              "<color=blue>!</color>");
                }
                ClearLine(y);
            }
        }

        if (linesToClear.Count > 0)
        {
            for(int i = 0; i < linesToClear.Count; i++)
            {
                for (int lineToDrop = linesToClear[i] - i + 1; lineToDrop < gridSizeY; lineToDrop++) {
                    //Debug.Log("line to drop: " + lineToDrop);
                    for (int j = 0; j < gridSizeX; j++)
                    {
                        GridUnit curGridUnit = fullGrid[j, lineToDrop];
                        if (curGridUnit.isOccupied)
                        {
                            MoveTileDown(curGridUnit);
                        }
                    }
                }
            }
        }
    }

    void ShowTetrisText()
    {
        tetrisText.SetActive(true);
        Invoke("HideTetrisText", 4f);
    }

    void HideTetrisText()
    {
        tetrisText.SetActive(false);
    }

    void MoveTileDown(GridUnit curGridUnit)
    {
        TileController curTile = curGridUnit.tileOnGridUnit.GetComponent<TileController>();
        curTile.MoveTile(Vector2Int.down);
        curTile.SetTile();
        curGridUnit.tileOnGridUnit = null;
        curGridUnit.isOccupied = false;
    }

    void ClearLine(int lineToClear)
    {
        if(lineToClear < 0 || lineToClear > gridSizeY)
        {
            Debug.LogError("Error: Cannot Clear Line: " + lineToClear);
            return;
        }
        for(int x = 0; x < gridSizeX; x++)
        {
            PieceController curPC = fullGrid[x, lineToClear].tileOnGridUnit.GetComponent<TileController>().pieceController;
            curPC.tiles[fullGrid[x, lineToClear].tileOnGridUnit.GetComponent<TileController>().tileID] = null;
            Destroy(fullGrid[x, lineToClear].tileOnGridUnit);
            if (!curPC.AnyTilesLeft()) { Destroy(curPC.gameObject); }
            fullGrid[x, lineToClear].tileOnGridUnit = null;
            fullGrid[x, lineToClear].isOccupied = false;
        }
    }

    public void PieceRemoved(Vector2Int[] pieceCoords)
    {
        foreach(Vector2Int coords in pieceCoords)
        {
            GridUnit curGridUnit = fullGrid[coords.x, coords.y];
            //MoveTileDown(curGridUnit);
            curGridUnit.tileOnGridUnit = null;
            curGridUnit.isOccupied = false;
        }

        for(int i = 0; i < pieceCoords.Length; i++)
        {
            for(int y = pieceCoords[i].y + 1; y < gridSizeY; y++)
            {
                GridUnit curGridUnit = fullGrid[pieceCoords[i].x, y];
                if (curGridUnit.isOccupied)
                {
                    MoveTileDown(curGridUnit);
                }
            }
        }
        CheckLineClears();
    }

    public List<GameObject> GetUnavailablePieces()
    {
        List<GameObject> unavaiablePieces = new List<GameObject>();

        for (int x = 0; x < gridSizeX; x++) {
            for(int y = gridSizeY - 1; y >= 0; y--)
            {
                if (fullGrid[x, y].isOccupied)
                {
                    GameObject curPC = fullGrid[x, y].tileOnGridUnit.GetComponent<TileController>().pieceController.gameObject;
                    //Debug.Log("unavailable piece at " + x + ", " + y);
                    //int curNum = curPC.tileNumber;
                    if (!unavaiablePieces.Any(test => test.GetInstanceID() == curPC.GetInstanceID()))
                    {
                        unavaiablePieces.Add(curPC);
                    }
                    y = -1;
                }
            }
        }
        Debug.Log("there are " + unavaiablePieces.Count + " Unavailable pieces");
        return unavaiablePieces;
    }

}

public class GridUnit
{
    public GameObject gameObject { get; private set; }
    public GameObject tileOnGridUnit;
    public Vector2Int location { get; private set; }
    public bool isOccupied;

    public GridUnit(GameObject newGameObject, Transform boardParent, int x, int y)
    {
        gameObject = GameObject.Instantiate(newGameObject, boardParent);
        location = new Vector2Int(x, y);
        isOccupied = false;

        gameObject.transform.position = new Vector3(location.x, location.y);
    }
}