using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour {

    public static BoardController instance;

    public GameObject gridUnitPrefab;
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

    /// <summary>
    /// Creates a grid of sized based off of gridSizeX and gridSizeY public variables
    /// </summary>
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

    /// <summary>
    /// Checks to see if the coorinate is a valid coordinate on the current tetris board.
    /// </summary>
    /// <param name="coordToTest">The x,y coordinate to test</param>
    /// <returns>Returns true if the coordinate to test is a vaild coordinate on the tetris board</returns>
    public bool IsInBounds(Vector2Int coordToTest)
    {
        if (coordToTest.x < 0 || coordToTest.x >= gridSizeX || coordToTest.y < 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Checks to see if a given coordinate is occupied by a tetris piece
    /// </summary>
    /// <param name="coordToTest">The x,y coordinate to test</param>
    /// <returns>Returns true if the coordinate is not occupied by a tetris piece</returns>
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

    /// <summary>
    /// Called when a piece is set in place. Sets the grid location to an occupied state.
    /// </summary>
    /// <param name="coords">The x,y coordinates to be occupied.</param>
    /// <param name="tileGO">GameObject of the specific tile on this grid location.</param>
    public void OccupyPos(Vector2Int coords, GameObject tileGO)
    {
        fullGrid[coords.x, coords.y].isOccupied = true;
        fullGrid[coords.x, coords.y].tileOnGridUnit = tileGO;
    }

    /// <summary>
    /// Checks line by line from bottom to top to see if that line is full and should be cleared.
    /// </summary>
    public void CheckLineClears()
    {
        //List of indexes for the lines that need to be cleared.
        List<int> linesToClear = new List<int>();

        //Counts how many lines next to each other will be cleared.
        //If this count get  to four lines, that is a Tetris line clear.
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

        //Once the lines have been cleared, the lines above those will drop to fill in the empty space
        if (linesToClear.Count > 0)
        {
            for(int i = 0; i < linesToClear.Count; i++)
            {
                /* The initial index of lineToDrop is calculated by taking the index of the first line
                 * that was cleared then adding 1 to indicate the index of the line above the cleared line,
                 * then the value i is subtracted to compensate for any lines already cleared.
                 */
                for (int lineToDrop = linesToClear[i] + 1 - i; lineToDrop < gridSizeY; lineToDrop++)
                {
                    for (int x = 0; x < gridSizeX; x++)
                    {
                        GridUnit curGridUnit = fullGrid[x, lineToDrop];
                        if (curGridUnit.isOccupied)
                        {
                            MoveTileDown(curGridUnit);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Displays the Tetris text when a Tetris line clear is achieved.
    /// </summary>
    void ShowTetrisText()
    {
        tetrisText.SetActive(true);
        Invoke("HideTetrisText", 4f);
    }

    /// <summary>
    /// Hides the Tetris line clear text.
    /// </summary>
    void HideTetrisText()
    {
        tetrisText.SetActive(false);
    }

    /// <summary>
    /// Moves an individual tile down one unit.
    /// </summary>
    /// <param name="curGridUnit">The grid unit that contains the tile to be moved down</param>
    void MoveTileDown(GridUnit curGridUnit)
    {
        TileController curTile = curGridUnit.tileOnGridUnit.GetComponent<TileController>();
        curTile.MoveTile(Vector2Int.down);
        curTile.SetTile();
        curGridUnit.tileOnGridUnit = null;
        curGridUnit.isOccupied = false;
    }

    /// <summary>
    /// Clears all tiles from a specified line
    /// </summary>
    /// <param name="lineToClear">Index of the line to be cleared</param>
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
            curPC.tiles[fullGrid[x, lineToClear].tileOnGridUnit.GetComponent<TileController>().tileIndex] = null;
            Destroy(fullGrid[x, lineToClear].tileOnGridUnit);
            if (!curPC.AnyTilesLeft()) { Destroy(curPC.gameObject); }
            fullGrid[x, lineToClear].tileOnGridUnit = null;
            fullGrid[x, lineToClear].isOccupied = false;
        }
    }

    /// <summary>
    /// Clears out the references to the piece being occupied on the grid unit,
    /// then drops all pieces above them by one unit.
    /// </summary>
    /// <param name="pieceCoords">Array of coordinates where where the pieces were occupying</param>
    public void PieceRemoved(Vector2Int[] pieceCoords)
    {
        foreach(Vector2Int coords in pieceCoords)
        {
            GridUnit curGridUnit = fullGrid[coords.x, coords.y];
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

    /// <summary>
    /// Determines which pieces are unavailable to be 'sacrificed.' Any piece where one tile is at the top of a given
    /// column is unable to be sacrificed.
    /// </summary>
    /// <returns>Returns a list of tiles unable to be sacrificed.</returns>
    public List<GameObject> GetUnavailablePieces()
    {
        List<GameObject> unavaiablePieces = new List<GameObject>();

        for (int x = 0; x < gridSizeX; x++) {
            for(int y = gridSizeY - 1; y >= 0; y--)
            {
                if (fullGrid[x, y].isOccupied)
                {
                    GameObject curPC = fullGrid[x, y].tileOnGridUnit.GetComponent<TileController>().pieceController.gameObject;
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