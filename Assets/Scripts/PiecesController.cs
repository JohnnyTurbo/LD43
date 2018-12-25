using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PiecesController : MonoBehaviour {

    public static PiecesController instance;

    public GameObject piecePrefab;
    public Vector2Int spawnPos;
    public float dropTime;
    public int turnsToSac;
    public Coroutine dropCurPiece;
    public Vector2Int[,] JLSTZ_OFFSET_DATA { get; private set; }
    public Vector2Int[,] I_OFFSET_DATA { get; private set; }
    public Vector2Int[,] O_OFFSET_DATA { get; private set; }
    public List<GameObject> piecesInGame;
    public GameObject pieceToDestroy = null;
    public GameObject sacText, gameOverText;

    GameObject curPiece = null;
    PieceController curPieceController = null;
    List<GameObject> availablePieces;

    int turnCounter;

    /// <summary>
    /// Called as soon as the instance is enabled. Sets the singleton and offset data arrays.
    /// </summary>
    private void Awake()
    {
        instance = this;
        

        JLSTZ_OFFSET_DATA = new Vector2Int[5, 4];
        JLSTZ_OFFSET_DATA[0, 0] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[0, 1] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[0, 2] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[0, 3] = Vector2Int.zero;

        JLSTZ_OFFSET_DATA[1, 0] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[1, 1] = new Vector2Int(1,0);
        JLSTZ_OFFSET_DATA[1, 2] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[1, 3] = new Vector2Int(-1, 0);

        JLSTZ_OFFSET_DATA[2, 0] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[2, 1] = new Vector2Int(1, -1);
        JLSTZ_OFFSET_DATA[2, 2] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[2, 3] = new Vector2Int(-1, -1);

        JLSTZ_OFFSET_DATA[3, 0] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[3, 1] = new Vector2Int(0, 2);
        JLSTZ_OFFSET_DATA[3, 2] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[3, 3] = new Vector2Int(0, 2);

        JLSTZ_OFFSET_DATA[4, 0] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[4, 1] = new Vector2Int(1, 2);
        JLSTZ_OFFSET_DATA[4, 2] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[4, 3] = new Vector2Int(-1, 2);

        I_OFFSET_DATA = new Vector2Int[5, 4];
        I_OFFSET_DATA[0, 0] = Vector2Int.zero;
        I_OFFSET_DATA[0, 1] = new Vector2Int(-1, 0);
        I_OFFSET_DATA[0, 2] = new Vector2Int(-1, 1);
        I_OFFSET_DATA[0, 3] = new Vector2Int(0, 1);

        I_OFFSET_DATA[1, 0] = new Vector2Int(-1, 0);
        I_OFFSET_DATA[1, 1] = Vector2Int.zero;
        I_OFFSET_DATA[1, 2] = new Vector2Int(1, 1);
        I_OFFSET_DATA[1, 3] = new Vector2Int(0, 1);

        I_OFFSET_DATA[2, 0] = new Vector2Int(2, 0);
        I_OFFSET_DATA[2, 1] = Vector2Int.zero;
        I_OFFSET_DATA[2, 2] = new Vector2Int(-2, 1);
        I_OFFSET_DATA[2, 3] = new Vector2Int(0, 1);

        I_OFFSET_DATA[3, 0] = new Vector2Int(-1, 0);
        I_OFFSET_DATA[3, 1] = new Vector2Int(0, 1);
        I_OFFSET_DATA[3, 2] = new Vector2Int(1, 0);
        I_OFFSET_DATA[3, 3] = new Vector2Int(0, -1);

        I_OFFSET_DATA[4, 0] = new Vector2Int(2, 0);
        I_OFFSET_DATA[4, 1] = new Vector2Int(0, -2);
        I_OFFSET_DATA[4, 2] = new Vector2Int(-2, 0);
        I_OFFSET_DATA[4, 3] = new Vector2Int(0, 2);

        O_OFFSET_DATA = new Vector2Int[1, 4];
        O_OFFSET_DATA[0, 0] = Vector2Int.zero;
        O_OFFSET_DATA[0, 1] = Vector2Int.down;
        O_OFFSET_DATA[0, 2] = new Vector2Int(-1, -1);
        O_OFFSET_DATA[0, 3] = Vector2Int.left;
    }

    /// <summary>
    /// Called at the first frame instance is enabled. Sets some variables.
    /// </summary>
    private void Start()
    {
        piecesInGame = new List<GameObject>();
        availablePieces = new List<GameObject>();
        sacText.SetActive(false);
        gameOverText.SetActive(false);
    }

    /// <summary>
    /// Called once every frame. Checks for player input.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            curPieceController.SendPieceToFloor();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveCurPiece(Vector2Int.down);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveCurPiece(Vector2Int.left);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveCurPiece(Vector2Int.right);
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if(curPieceController != null)
            {
                return;
            }
            turnCounter = 0;
            SpawnPiece();
        }
        if (Input.GetKeyDown(KeyCode.R)){
            SceneManager.LoadScene(0);
        }

        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Space))
        {
            curPieceController.RotatePiece(true, true);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            curPieceController.RotatePiece(false, true);
        }

        if (Input.GetButtonDown("Fire1") && pieceToDestroy != null)
        {
            turnCounter = 0;
            sacText.SetActive(false);
            foreach(GameObject pc in piecesInGame)
            {
                pc.GetComponent<PieceController>().EnablePiece();
            }
            DestroyPiece();
            BoardController.instance.isSacrificing = false;
            SpawnPiece();
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SpawnDebug(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SpawnDebug(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnDebug(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SpawnDebug(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SpawnDebug(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SpawnDebug(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SpawnDebug(6);
        }

    }

    /// <summary>
    /// Drops the piece the current piece the player is controlling by one unit.
    /// </summary>
    /// <returns>Function is called on a loop based on the 'dropTime' variable.</returns>
    IEnumerator DropCurPiece()
    {     
        while (!BoardController.instance.isSacrificing) {
            MoveCurPiece(Vector2Int.down);
            yield return new WaitForSeconds(dropTime);
        }
    }

    /// <summary>
    /// Once the piece is set in it's final location, the coroutine called to repeatedly drop the piece is stopped.
    /// </summary>
    public void PieceSet()
    {
        //if(dropCurPiece == null) { return; }
        StopCoroutine(dropCurPiece);
    }

    /// <summary>
    /// Makes any necessary changes once the game has ended.
    /// </summary>
    public void GameOver()
    {
        PieceSet();
        gameOverText.SetActive(true);
    }

    /// <summary>
    /// Removes the specified piece from the list of current pieces in the game.
    /// </summary>
    /// <param name="pieceToRem">Game Object of the Tetris piece to be removed.</param>
    public void RemovePiece(GameObject pieceToRem)
    {
        piecesInGame.Remove(pieceToRem);
    }

    /// <summary>
    /// Makes any necessary changes when destroying a piece.
    /// </summary>
    void DestroyPiece()
    {
        PieceController curPC = pieceToDestroy.GetComponent<PieceController>();
        Vector2Int[] tileCoords = curPC.GetTileCoords();
        RemovePiece(pieceToDestroy);
        Destroy(pieceToDestroy);
        BoardController.instance.PieceRemoved(tileCoords);
    }

    /// <summary>
    /// Spawns a new Tetris piece.
    /// </summary>
    public void SpawnPiece()
    {     
        turnCounter++;

        if(turnCounter >= turnsToSac && CanSacrifice())
        {
            Debug.Log("Attempting Sacrifice");
            sacText.SetActive(true);
            BoardController.instance.isSacrificing = true;
            return;
        }

        GameObject localGO = GameObject.Instantiate(piecePrefab, transform);
        curPiece = localGO;
        PieceType randPiece = (PieceType)Random.Range(0, 7);        
        curPieceController = curPiece.GetComponent<PieceController>();
        curPieceController.SpawnPiece(randPiece);
        
        piecesInGame.Add(localGO);

        dropCurPiece = StartCoroutine(DropCurPiece());
    }

    public void SpawnDebug(int id)
    {
        GameObject localGO = GameObject.Instantiate(piecePrefab, transform);
        curPiece = localGO;
        PieceType randPiece = (PieceType)id;
        curPieceController = curPiece.GetComponent<PieceController>();
        curPieceController.SpawnPiece(randPiece);

        piecesInGame.Add(localGO);
    }

    /// <summary>
    /// Checks to see if the sacrificing operation can be made.
    /// </summary>
    /// <returns>True if operation can be made. False if it can't</returns>
    bool CanSacrifice()
    {
        availablePieces.Clear();
        availablePieces.AddRange(piecesInGame);
        Debug.Log("there are " + piecesInGame.Count + " total pieces");
        List<GameObject> unavailablePieces = BoardController.instance.GetUnavailablePieces();
        int numAvailablePieces = piecesInGame.Count - unavailablePieces.Count;
        if(numAvailablePieces < 2) { return false; }
        foreach (GameObject pc in unavailablePieces)
        {
            availablePieces.Remove(pc);
            pc.GetComponent<PieceController>().DisablePiece();
        }
        
        Debug.Log("there are " + availablePieces.Count + " available pieces");
        return true;
    }

    /// <summary>
    /// Moves the current piece controlled by the player.
    /// </summary>
    /// <param name="movement">X,Y amount the piece should be moved by</param>
    public void MoveCurPiece(Vector2Int movement)
    {
        if(curPiece == null)
        {
            return;
        }
        curPieceController.MovePiece(movement);
    }
}
