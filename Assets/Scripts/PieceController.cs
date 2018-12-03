using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PieceType { O, I, S, Z, L, J, T }

public class PieceController : MonoBehaviour {

    public PieceType curType;
    public Sprite[] tileSprites;

    public int rotationIndex { get; private set; }
    public int setDelayTime;
    public int tileNumber;
    public bool isDisabledFromSacrifice;

    public TileController[] tiles;
    Vector2Int spawnLocation = new Vector2Int(4, 21);

    private void Awake()
    {
        rotationIndex = 0;

        tiles = new TileController[4];
        for(int i = 1; i <= tiles.Length; i++)
        {
            //Debug.Log(i);
            string tileName = "Tile" + i;
            //GameObject tileGO = transform.Find("Tiles").Find(tileName).gameObject;
            //TileController newTile = new TileController(tileGO, spawnLocation);
            TileController newTile = transform.Find("Tiles").Find(tileName).GetComponent<TileController>();
            tiles[i - 1] = newTile;
        } 
    }

    public void SpawnPiece(PieceType newType)
    {
        isDisabledFromSacrifice = false;
        curType = newType;
        tiles[0].UpdatePosition(spawnLocation);

        switch (curType)
        {
            case PieceType.I:
                tiles[1].UpdatePosition(spawnLocation + Vector2Int.left);
                tiles[2].UpdatePosition(spawnLocation + (Vector2Int.right * 2));
                tiles[3].UpdatePosition(spawnLocation + Vector2Int.right);
                SetTileSprites(tileSprites[0]);
                break;

            case PieceType.J:
                tiles[1].UpdatePosition(spawnLocation + Vector2Int.left);
                tiles[2].UpdatePosition(spawnLocation + new Vector2Int(-1, 1));
                tiles[3].UpdatePosition(spawnLocation + Vector2Int.right);
                SetTileSprites(tileSprites[1]);
                break;

            case PieceType.L:
                tiles[1].UpdatePosition(spawnLocation + Vector2Int.left);
                tiles[2].UpdatePosition(spawnLocation + new Vector2Int(1, 1));
                tiles[3].UpdatePosition(spawnLocation + Vector2Int.right);
                SetTileSprites(tileSprites[2]);
                break;

            case PieceType.O:
                tiles[1].UpdatePosition(spawnLocation + Vector2Int.right);
                tiles[2].UpdatePosition(spawnLocation + new Vector2Int(1, 1));
                tiles[3].UpdatePosition(spawnLocation + Vector2Int.up);
                SetTileSprites(tileSprites[3]);
                break;

            case PieceType.S:
                tiles[1].UpdatePosition(spawnLocation + Vector2Int.left);
                tiles[2].UpdatePosition(spawnLocation + new Vector2Int(1, 1));
                tiles[3].UpdatePosition(spawnLocation + Vector2Int.up);
                SetTileSprites(tileSprites[4]);
                break;

            case PieceType.T:
                tiles[1].UpdatePosition(spawnLocation + Vector2Int.left);
                tiles[2].UpdatePosition(spawnLocation + Vector2Int.up);
                tiles[3].UpdatePosition(spawnLocation + Vector2Int.right);
                SetTileSprites(tileSprites[5]);
                break;

            case PieceType.Z:
                tiles[1].UpdatePosition(spawnLocation + Vector2Int.up);
                tiles[2].UpdatePosition(spawnLocation + new Vector2Int(-1, 1));
                tiles[3].UpdatePosition(spawnLocation + Vector2Int.right);
                SetTileSprites(tileSprites[6]);
                break;

            default:

                break;
        }
        int index = 0;
        foreach(TileController ti in tiles)
        {
            ti.InitializeTile(this, index);
            index++;
        }
    }

    public void DisablePiece()
    {
        isDisabledFromSacrifice = true;
        for(int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == null)
            {
                continue;
            }
            tiles[i].ShowDisabledSprite();
        }
    }

    public void EnablePiece()
    {
        isDisabledFromSacrifice = false;
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == null)
            {
                continue;
            }
            tiles[i].ShowOriginalSprite();
        }
    }

    public Vector2Int[] GetTileCoords()
    {
        List<Vector2Int> curTileCoords = new List<Vector2Int>();

        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == null)
            {
                continue;
            }
            curTileCoords.Add(tiles[i].coordinates);
        }
        curTileCoords = curTileCoords.OrderBy(x => x.x).ThenByDescending(x => x.y).ToList();
        foreach(Vector2Int v2i in curTileCoords)
        {
            Debug.Log("CurtIle is " + v2i.ToString());
        }
        Vector2Int[] curCoords = curTileCoords.ToArray();
        return curCoords;
    }

    public void SetTileSprites(Sprite newSpr)
    {
        for(int i = 0; i < tiles.Length; i++)
        {
            if(tiles[i] == null)
            {
                continue;
            }
            tiles[i].gameObject.GetComponent<SpriteRenderer>().sprite = newSpr;
        }
    }

    public bool CanMovePiece(Vector2Int movement)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (!tiles[i].CanTileMove(movement + tiles[i].coordinates))
            {
                //Debug.Log("Cant Go there!");
                return false;
            }
        }
        return true;
    }

    public bool AnyTilesLeft()
    {
        for(int i = 0; i < tiles.Length; i++)
        {
            if(tiles[i] != null)
            {
                return true;
            }
        }
        Debug.Log("no tiles left");
        //PiecesController.instance.piecesInGame.Remove(gameObject);
        PiecesController.instance.RemovePiece(gameObject);
        return false;
    }

    public bool MovePiece(Vector2Int movement)
    {
        //if (setDelay != null){ StopCoroutine(setDelay); }
        for (int i = 0; i < tiles.Length; i++)
        {
            if (!tiles[i].CanTileMove(movement + tiles[i].coordinates))
            {
                Debug.Log("Cant Go there!");
                if(movement == Vector2Int.down)
                {
                    SetPiece();
                    //setDelay = StartCoroutine(BeginSetDelay());
                    //PiecesController.instance.StopCoroutine(PiecesController.instance.dropTileNorm);
                }
                return false;
            }
        }

        for(int i = 0; i< tiles.Length; i++)
        {
            tiles[i].MoveTile(movement);
        }

        return true;
    }

    public void RotatePiece(bool clockwise, bool shouldOffset)
    {
        int oldRotationIndex = rotationIndex;
        rotationIndex += clockwise ? 1 : -1;
        rotationIndex = Mod(rotationIndex, 4);

        Debug.Log("Rot from index: " + oldRotationIndex + " to " + rotationIndex);

        for(int i = 0; i < tiles.Length; i++)
        {
            tiles[i].RotateTile(tiles[0].coordinates, clockwise);

        }

        if (!shouldOffset)
        {
            return;
        }

        if(!Offset(oldRotationIndex, rotationIndex))
        {
            RotatePiece(!clockwise, false);
        }
    }

    int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    bool Offset(int oldRot, int newRot)
    {
        Vector2Int offsetVal1, offsetVal2, endOffset;
        Vector2Int[,] curOffsetData;
        
        if(curType == PieceType.O)
        {
            curOffsetData = PiecesController.instance.O_OFFSET_DATA;
        }
        else if(curType == PieceType.I)
        {
            curOffsetData = PiecesController.instance.I_OFFSET_DATA;
        }
        else
        {
            curOffsetData = PiecesController.instance.JLSTZ_OFFSET_DATA;
        }

        endOffset = Vector2Int.zero;

        bool movePossible = false;

        for (int i = 0; i < 5; i++)
        {
            offsetVal1 = curOffsetData[i, oldRot];
            offsetVal2 = curOffsetData[i, newRot];
            endOffset = offsetVal1 - offsetVal2;
            if (CanMovePiece(endOffset))
            {
                Debug.Log("Moving on condition: " + i + ", endOffset is: " + endOffset.ToString());
                movePossible = true;
                break;
            }
        }

        if (movePossible)
        {
            MovePiece(endOffset);
        }
        else
        {
            Debug.Log("Move impossible");
        }
        return movePossible;
    }

    public void SetPiece()
    {
        for(int i = 0; i < tiles.Length; i++)
        {
            if (!tiles[i].SetTile())
            {
                Debug.Log("GAME OVER!");
                PiecesController.instance.GameOver();
                return;
            }
        }
        BoardController.instance.CheckLineClears();
        PiecesController.instance.PieceSet();
        PiecesController.instance.SpawnPiece();
    }

    public void SendPieceToFloor()
    {
        while (MovePiece(Vector2Int.down))
        {
            //Debug.Log("Dropping piece");
        }
        //Debug.Log("piece at floor");
    }
}
