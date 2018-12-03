using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PiecesController : MonoBehaviour {

    public static PiecesController instance;

    //public GameObject[] piecePrefabs;
    public GameObject piecePrefab;
    public Vector2Int spawnPos;
    public float dropTime;
    public int turnsToSac;
    public Coroutine dropTileNorm;
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
    int curTileNumber = 0;

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

    private void Start()
    {
        piecesInGame = new List<GameObject>();
        availablePieces = new List<GameObject>();
        sacText.SetActive(false);
        gameOverText.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //MovePieceOneUnit(Vector2Int.up);
            curPieceController.SendPieceToFloor();
            //SpawnPiece();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MovePieceOneUnit(Vector2Int.down);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MovePieceOneUnit(Vector2Int.left);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MovePieceOneUnit(Vector2Int.right);
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if(curPieceController != null)
            {
                //curPieceController.SetPiece();
                return;
            }
            turnCounter = 0;
            SpawnPiece();
            //InvokeRepeating("DropPiece", 0f, dropTime);

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
        if(Input.GetButtonDown("Fire1") && pieceToDestroy != null)
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
    }

    IEnumerator DropThePiece()
    {
        
        while (!BoardController.instance.isSacrificing) {
            //Debug.Log("dropthepiece");
            MovePieceOneUnit(Vector2Int.down);
            yield return new WaitForSeconds(dropTime);
        }
    }

    public void PieceSet()
    {
        StopCoroutine(dropTileNorm);
    }

    public void GameOver()
    {
        PieceSet();
        gameOverText.SetActive(true);
    }

    void DestroyPiece()
    {
        PieceController curPC = pieceToDestroy.GetComponent<PieceController>();
        Debug.Log("removing piece");
        piecesInGame.Remove(curPC.gameObject);
        Vector2Int[] tileCoords = curPC.GetTileCoords();
        Destroy(pieceToDestroy);
        BoardController.instance.PieceRemoved(tileCoords);
    }

    public void SpawnPiece()
    {
        
        turnCounter++;
        curTileNumber++;

        PieceController localPC;
        // localGO;

        if(turnCounter >= turnsToSac && AttemptToSacrifice())
        {
            Debug.Log("tryna sacc?");
            sacText.SetActive(true);
            BoardController.instance.isSacrificing = true;
            return;
        }

        GameObject localGO = GameObject.Instantiate(piecePrefab, transform);
        curPiece = localGO;
        PieceType randPiece = (PieceType)Random.Range(0, 7);        
        localPC = curPieceController = curPiece.GetComponent<PieceController>();
        curPieceController.SpawnPiece(randPiece);
        curPieceController.tileNumber = curTileNumber;
        
        piecesInGame.Add(localGO);

        dropTileNorm = StartCoroutine(DropThePiece());
        Debug.Log("Spawning tole " + curTileNumber + " turn " + turnCounter);
    }

    bool AttemptToSacrifice()
    {
        //availablePieces = piecesInGame;
        availablePieces.Clear();
        availablePieces.AddRange(piecesInGame);
        Debug.Log("there are " + piecesInGame.Count + " toatl pieces");
        List<GameObject> unavailablePieces = BoardController.instance.GetUnavailablePieces();
        foreach (GameObject pc in unavailablePieces)
        {
            availablePieces.Remove(pc);
            pc.GetComponent<PieceController>().DisablePiece();
        }
        
        Debug.Log("there are " + availablePieces.Count + " available pieces");
        if (availablePieces.Count > 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void RemovePiece(GameObject pieceToREm)
    {
        Debug.Log("removing piece");
        piecesInGame.Remove(pieceToREm);
    }

    void DropPiece()
    {
        MovePieceOneUnit(Vector2Int.down);
    }

    public void MovePieceOneUnit(Vector2Int moveDirection)
    {
        if(curPiece == null)
        {
            return;
        }
        curPieceController.MovePiece(moveDirection);
    }
}
