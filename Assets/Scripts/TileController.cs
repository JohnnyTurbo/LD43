using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour {

    public Vector2Int coordinates;

    public Sprite originalSpr, hoverSpr, disableSpr;

    SpriteRenderer spriteRenderer;
    public PieceController pieceController;
    public int tileID;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void InitializeTile(PieceController myPC, int ID)
    {
        originalSpr = spriteRenderer.sprite;
        pieceController = myPC;
        tileID = ID;
    }

    private void OnMouseEnter()
    {
        if (!BoardController.instance.isSacrificing || pieceController.isDisabledFromSacrifice)
        {
            return;
        }
        PiecesController.instance.pieceToDestroy = pieceController.gameObject;
        ShowHighlightedSprite();
    }

    private void OnMouseExit()
    {
        if (!BoardController.instance.isSacrificing || pieceController.isDisabledFromSacrifice)
        {
            return;
        }
        PiecesController.instance.pieceToDestroy = null;
        ShowOriginalSprite();
    }

    public void ShowDisabledSprite()
    {
        //spriteRenderer.sprite = disableSpr;
        pieceController.SetTileSprites(disableSpr);
    }

    public void ShowHighlightedSprite()
    {
        //spriteRenderer.sprite = hoverSpr;
        pieceController.SetTileSprites(hoverSpr);
    }

    public void ShowOriginalSprite()
    {
        //spriteRenderer.sprite = originalSpr;
        pieceController.SetTileSprites(originalSpr);
    }

    public bool CanTileMove(Vector2Int endPos)
    {
        //Vector2Int endPos = coordinates + movement;
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

    public void MoveTile(Vector2Int movement)
    {
        Vector2Int endPos = coordinates + movement;
        UpdatePosition(endPos);
    }

    public void UpdatePosition(Vector2Int newPos)
    {
        //Debug.Log("updating pos from " + coordinates.ToString() + " to: " + newPos.ToString(), gameObject);
        coordinates = newPos;
        Vector3 newV3Pos = new Vector3(newPos.x, newPos.y);
        gameObject.transform.position = newV3Pos;
    }

    public bool SetTile()
    {
        if (coordinates.y >= 20)
        {
            return false;
        }

        BoardController.instance.OccupyPos(coordinates, gameObject);
        return true;
    }

    public void RotateTile(Vector2Int originTile, bool clockwise)
    {

        Vector2Int relativePos = coordinates - originTile;
        Vector2Int[] rotMatrix = clockwise ? new Vector2Int[2] { new Vector2Int(0, -1), new Vector2Int(1, 0) }
                                            : new Vector2Int[2] { new Vector2Int(0, 1), new Vector2Int(-1, 0) };
        int newXPos = (rotMatrix[0].x * relativePos.x) + (rotMatrix[1].x * relativePos.y);
        int newYPos = (rotMatrix[0].y * relativePos.x) + (rotMatrix[1].y * relativePos.y);
        Vector2Int newPos = new Vector2Int(newXPos, newYPos);

        /*
        Debug.Log("Rotating " + gameObject.name + " about " + originTile.ToString() + ", the rel pos is: " +
                    relativePos.ToString() + " and the new rel pos is: " + newPos.ToString());
        */

        newPos += originTile;
        UpdatePosition(newPos);
    }
}
