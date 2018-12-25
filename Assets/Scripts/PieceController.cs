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
    public bool isDisabledFromSacrifice;

    public TileController[] tiles;
    //Vector2Int spawnLocation = new Vector2Int(4, 21);
    Vector2Int spawnLocation;

    /// <summary>
    /// Called as soon as the piece is initialized. Initialiezes some necessary values.
    /// </summary>
    private void Awake()
    {
        spawnLocation = PiecesController.instance.spawnPos;
        rotationIndex = 0;

        tiles = new TileController[4];
        for(int i = 1; i <= tiles.Length; i++)
        {
            string tileName = "Tile" + i;
            TileController newTile = transform.Find("Tiles").Find(tileName).GetComponent<TileController>();
            tiles[i - 1] = newTile;
        } 
    }

    /// <summary>
    /// Moves the attached tiles to form the Tetris piece specified. Also sets the correct color of tile sprite.
    /// </summary>
    /// <param name="newType">Type of tetris piece to be spawned.</param>
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

    /// <summary>
    /// Called when the piece is disabled from being sacrificed. Sets tile sprites gray.
    /// </summary>
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

    /// <summary>
    /// Called after sacrifice operation is complete, sets tile sprite color back to original values.
    /// </summary>
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

    /// <summary>
    /// Gets the coordinates of all active tiles attached to this piece.
    /// </summary>
    /// <returns>Returns array of coordinates for currently active tiles on the piece.</returns>
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

    /// <summary>
    /// Sets the sprites of all tiles on this piece
    /// </summary>
    /// <param name="newSpr">New sprite to set for this tile</param>
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

    /// <summary>
    /// Checks if the piece is able to be moved by the specified amount. A piece cannot be moved if there
    /// is already another piece there or the piece would end up out of bounds.
    /// </summary>
    /// <param name="movement">X,Y amount to move the piece</param>
    /// <returns></returns>
    public bool CanMovePiece(Vector2Int movement)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (!tiles[i].CanTileMove(movement + tiles[i].coordinates))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Checks to see if there are any tiles left for the given piece.
    /// </summary>
    /// <returns>True if there are still tiles left. False if the piece has no remaining tiles.</returns>
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
        PiecesController.instance.RemovePiece(gameObject);
        return false;
    }

    /// <summary>
    /// Moves the piece by the specified amount.
    /// </summary>
    /// <param name="movement">X,Y amount to move the piece</param>
    /// <returns>True if the piece was able to be moved. False if the move couln't be completed.</returns>
    public bool MovePiece(Vector2Int movement)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (!tiles[i].CanTileMove(movement + tiles[i].coordinates))
            {
                Debug.Log("Cant Go there!");
                if(movement == Vector2Int.down)
                {
                    SetPiece();
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

    /// <summary>
    /// Rotates the piece by 90 degrees in specified direction. Offest operations should almost always be attempted,
    /// unless you are rotating the piece back to its original position.
    /// </summary>
    /// <param name="clockwise">Set to true if rotating clockwise. Set to False if rotating CCW</param>
    /// <param name="shouldOffset">Set to true if offset operations should be attempted.</param>
    public void RotatePiece(bool clockwise, bool shouldOffset)
    {
        int oldRotationIndex = rotationIndex;
        rotationIndex += clockwise ? 1 : -1;
        rotationIndex = Mod(rotationIndex, 4);

        for(int i = 0; i < tiles.Length; i++)
        {
            tiles[i].RotateTile(tiles[0].coordinates, clockwise);

        }

        if (!shouldOffset)
        {
            return;
        }

        bool canOffset = Offset(oldRotationIndex, rotationIndex);

        if (!canOffset)
        {
            RotatePiece(!clockwise, false);
        }
    }

    /// <summary>
    /// True modulo operation that works for positive and negative values.
    /// </summary>
    /// <param name="x">The dividend</param>
    /// <param name="m">The divisor</param>
    /// <returns>Returns the remainder of x divided by m</returns>
    int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    /// <summary>
    /// Performs 5 tests on the piece to find a valid final location for the piece.
    /// </summary>
    /// <param name="oldRotIndex">Original rotation index of the piece</param>
    /// <param name="newRotIndex">Rotation index the piece will be rotating to</param>
    /// <returns>True if one of the tests passed and a final location was found. False if all test failed.</returns>
    bool Offset(int oldRotIndex, int newRotIndex)
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

        for (int testIndex = 0; testIndex < 5; testIndex++)
        {
            offsetVal1 = curOffsetData[testIndex, oldRotIndex];
            offsetVal2 = curOffsetData[testIndex, newRotIndex];
            endOffset = offsetVal1 - offsetVal2;
            if (CanMovePiece(endOffset))
            {
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

    /// <summary>
    /// Sets the piece in its permanent location.
    /// </summary>
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

    /// <summary>
    /// Drops piece down as far as it can go.
    /// </summary>
    public void SendPieceToFloor()
    {
        while (MovePiece(Vector2Int.down)) {}
    }
}
