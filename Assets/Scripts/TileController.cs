using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour {

    public Vector2Int coordinates;

    public Sprite originalSpr, hoverSpr, disableSpr;

    SpriteRenderer spriteRenderer;
    public PieceController pieceController;
    public int tileIndex;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Initializes variables on the instance
    /// </summary>
    /// <param name="myPC">Reference to the piece controller this tile is associated with</param>
    /// <param name="index">Index of the tile on the piece</param>
    public void InitializeTile(PieceController myPC, int index)
    {
        originalSpr = spriteRenderer.sprite;
        pieceController = myPC;
        tileIndex = index;
    }

    /// <summary>
    /// Called when the mouse hovers above the tile. Used to change color when above the tile during the sacificing phase.
    /// </summary>
    private void OnMouseEnter()
    {
        if (!BoardController.instance.isSacrificing || pieceController.isDisabledFromSacrifice)
        {
            return;
        }
        PiecesController.instance.pieceToDestroy = pieceController.gameObject;
        ShowHighlightedSprite();
    }

    /// <summary>
    /// Called when the mouse is no longer hovering the tile.
    /// </summary>
    private void OnMouseExit()
    {
        if (!BoardController.instance.isSacrificing || pieceController.isDisabledFromSacrifice)
        {
            return;
        }
        PiecesController.instance.pieceToDestroy = null;
        ShowOriginalSprite();
    }

    /// <summary>
    /// Changes the the sprite to the disabled sprite
    /// </summary>
    public void ShowDisabledSprite()
    {
        pieceController.SetTileSprites(disableSpr);
    }

    /// <summary>
    /// Changes the sprite to the highlighted sprite
    /// </summary>
    public void ShowHighlightedSprite()
    {
        pieceController.SetTileSprites(hoverSpr);
    }

    /// <summary>
    /// Changes the sprite to the original sprite
    /// </summary>
    public void ShowOriginalSprite()
    {
        pieceController.SetTileSprites(originalSpr);
    }

    /// <summary>
    /// Checks to see if the tile can be moved to the specified positon.
    /// </summary>
    /// <param name="endPos">Coordinates of the position you are trying to move the tile to</param>
    /// <returns>True if the tile can be moved there. False if the tile cannot be moved there</returns>
    public bool CanTileMove(Vector2Int endPos)
    {
        if (!BoardController.instance.IsInBounds(endPos))
        {
            return false;
        }
        if (!BoardController.instance.IsPosEmpty(endPos))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Moves the tile by the specified amount
    /// </summary>
    /// <param name="movement">X,Y amount the tile will be moved by</param>
    public void MoveTile(Vector2Int movement)
    {
        Vector2Int endPos = coordinates + movement;
        UpdatePosition(endPos);
    }

    /// <summary>
    /// Sets some new variables at the new position
    /// </summary>
    /// <param name="newPos">New position the tile will reside at</param>
    public void UpdatePosition(Vector2Int newPos)
    {
        coordinates = newPos;
        Vector3 newV3Pos = new Vector3(newPos.x, newPos.y);
        gameObject.transform.position = newV3Pos;
    }

    /// <summary>
    /// Sets the tile in it's current position
    /// </summary>
    /// <returns>True if the tile is on the board. False if tile is above playing field, GAME OVER.</returns>
    public bool SetTile()
    {
        if (coordinates.y >= 20)
        {
            return false;
        }

        BoardController.instance.OccupyPos(coordinates, gameObject);
        return true;
    }

    /// <summary>
    /// Rotates the tile by 90 degrees about the origin tile.
    /// </summary>
    /// <param name="originPos">Coordinates this tile will be rotating about.</param>
    /// <param name="clockwise">True if rotating clockwise. False if rotatitng CCW</param>
    public void RotateTile(Vector2Int originPos, bool clockwise)
    {

        Vector2Int relativePos = coordinates - originPos;
        Vector2Int[] rotMatrix = clockwise ? new Vector2Int[2] { new Vector2Int(0, -1), new Vector2Int(1, 0) }
                                           : new Vector2Int[2] { new Vector2Int(0, 1), new Vector2Int(-1, 0) };
        int newXPos = (rotMatrix[0].x * relativePos.x) + (rotMatrix[1].x * relativePos.y);
        int newYPos = (rotMatrix[0].y * relativePos.x) + (rotMatrix[1].y * relativePos.y);
        Vector2Int newPos = new Vector2Int(newXPos, newYPos);

        newPos += originPos;
        UpdatePosition(newPos);
    }
}
