using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using LibCarcassonne.GameComponents;
using LibCarcassonne.GameStructures;
using LibCarcassonne.GameLogic;
using Photon.Pun;

public class GameManager : MonoBehaviourPun
{
    public GameObject TileRoot;
    public GameObject TilePrefab;
    public GameObject SelectorTilePrefab;
    public GameObject MeeplePrefab;
    public GameObject MeeplePlacePrefab;
    public Image NextTile;

    public GameObject confirmTileButton;
    public GameObject skipMeepleButton;

    // CoreLogic
    ComponentManager componentManager = new ComponentManager();
    GameRunner gameRunner;
    List<TileComponent> tileComponents;
    Tile currentTile;
    Tile currentPlacedTile;
    GameObject currentTileObjectRef;
    (int, int) currentTilePosition;
    int currentTileRotation;
    List<int> currentTilePossibleRotations = new List<int>();
    List<GameObject> selectionTiles = new List<GameObject>();
    List<GameObject> selectionMeeples = new List<GameObject>();

    enum TurnLogicState
    {
        NONE,
        PLACED_TILE,
        CONFIRMED_TILE_POSITION,
        PLACED_MEEPLE
    };
    TurnLogicState currentState;

    void Start()
    {
        tileComponents = componentManager.ParseJson("Assets/Scripts/LibCarcassonne/tiles_map.json");
        if (tileComponents.Count != 72)
        {
            throw new Exception("Incorrect number of tiles");
        }
        gameRunner = new GameRunner(tileComponents);

        StructureManager structureManager = new StructureManager();

        currentState = TurnLogicState.NONE;
        confirmTileButton.SetActive(false);
        skipMeepleButton.SetActive(false);

        //Start tile
        currentTile = gameRunner.GetCurrentRoundTile();
        currentTileRotation = 0;
        currentTilePosition = (0, 0);
        CreateTile(currentTile.GetIndex() - 1, new Vector3(currentTilePosition.Item1, 0, currentTilePosition.Item2), Quaternion.Euler(0.0f, currentTileRotation * 90, 0.0f));
        gameRunner.AddTileInPositionAndRotation(currentTile, (ConvertUnityToLibCarcassonneCoords(currentTilePosition)), currentTileRotation);
        currentTileObjectRef.transform.Find("ArrowPlace").gameObject.SetActive(false);

        //Next tile
        currentTile = gameRunner.GetCurrentRoundTile();
        SetNextTile(currentTile.GetIndex() - 1);
        CreateSelectionTiles();
        currentTileObjectRef = null;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                if (currentState == TurnLogicState.NONE && hitInfo.collider.name.StartsWith("SelectorTile"))
                {
                    var pos = hitInfo.collider.GetComponent<Transform>().localPosition;
                    currentTilePosition = ((int)pos.x, (int)pos.z);
                    var freePositions = gameRunner.GetFreePositionsForTile(currentTile);
                    foreach (var fPos in freePositions)
                    {
                        if (ConvertLibCarcassonneCoordsToUnity(fPos.Item1) == currentTilePosition)
                        {
                            currentTilePossibleRotations = fPos.Item2;
                            currentTileRotation = currentTilePossibleRotations[0];
                            CreateTile(currentTile.GetIndex() - 1, new Vector3(currentTilePosition.Item1, 0, currentTilePosition.Item2), Quaternion.Euler(0.0f, currentTileRotation * 90, 0.0f));
                            photonView.RPC("CreateTile",RpcTarget.Others, currentTile.GetIndex() - 1, new Vector3(currentTilePosition.Item1, 0, currentTilePosition.Item2), Quaternion.Euler(0.0f, currentTileRotation * 90, 0.0f));
                            DestroySelectionTiles();
                            currentState = TurnLogicState.PLACED_TILE;
                            confirmTileButton.SetActive(true);
                            break;
                        }
                    }
                }
                else if (currentState == TurnLogicState.PLACED_TILE && currentTileObjectRef == hitInfo.collider.gameObject.transform.parent.gameObject)
                {
                    RotateTile();
                }
                else if (currentState == TurnLogicState.CONFIRMED_TILE_POSITION && hitInfo.collider.name.StartsWith("MeeplePlace"))
                {
                    AddMeeple(hitInfo.collider.gameObject, int.Parse(hitInfo.collider.name.Substring(11)));
                    currentState = TurnLogicState.PLACED_MEEPLE;
                    ConfirmMove();
                }
            }
        }
    }

    // Object Creation
    [PunRPC]
    public void CreateTile(int tile, Vector3 relativePosition, Quaternion rotation)
    {
        var tileClone = tile >= 0 ? Instantiate(TilePrefab, TileRoot.transform) : Instantiate(SelectorTilePrefab, TileRoot.transform);
        tileClone.transform.localPosition = relativePosition;
        tileClone.transform.rotation = rotation;
        if (tile < 0)
        {
            selectionTiles.Add(tileClone);
            return;
        }
        string spriteName = "tile" + (tile + 1).ToString();
        var sprite = Resources.Load<Sprite>("Images/Tiles/" + spriteName);
        if (sprite)
        {
            tileClone.name = spriteName;
            var rend = tileClone.transform.GetChild(0).GetComponent<SpriteRenderer>();
            rend.sprite = sprite;
            currentTileObjectRef = tileClone;
        }
        else
        {
            Debug.Log("Failed to load" + "Images/Tiles/" + spriteName); //todo: a serializable field with all sprites added would remove loading
        }
    }

    [PunRPC]
    public void CreateMeeple(MeepleColor meepleColor, Vector3 relativePosition)
    {
        const float meepleHeight = 0.115f; //should not be a random constant
        var materialName = meepleColor.ToString() + "_meeple";
        var material = Resources.Load<Material>("Materials/" + materialName);
        if (material)
        {
            var newMeeple = Instantiate(MeeplePrefab, currentTileObjectRef.transform);
            var rend = newMeeple.transform.GetChild(0).GetComponent<Renderer>();
            rend.material = material;
            newMeeple.transform.localPosition = new Vector3(relativePosition.x, relativePosition.y + meepleHeight, relativePosition.z);
        }
        else
        {
            Debug.Log("Failed to load" + "Materials/" + materialName);
        }
    }

    void SetNextTile(int tile)
    {
        string spriteName = "tile" + (tile + 1).ToString();
        var sprite = Resources.Load<Sprite>("Images/Tiles/" + spriteName);
        if (sprite)
        {
            NextTile.sprite = sprite;
        }
    }

    void DestroySelectionTiles()
    {
        foreach (var obj in selectionTiles)
        {
            Destroy(obj);
        }
    }

    void CreateSelectionTiles()
    {
        foreach (var tilePos in gameRunner.GetFreePositionsForTile(currentTile))
        {
            var pos = ConvertLibCarcassonneCoordsToUnity(tilePos.Item1);
            CreateTile(-1, new Vector3(pos.Item1, 0, pos.Item2), Quaternion.Euler(0.0f, 0.0f, 0.0f));
        }
    }

    void SetMeeplePositions(List<int> positions)
    {
        var component = tileComponents[currentTile.GetIndex() - 1];
        for (var i = 0; i < positions.Count; i++)
        {
            var feature = component.Types[positions[i]];
            var featureClone = Instantiate(MeeplePlacePrefab, currentTileObjectRef.transform);
            featureClone.transform.localPosition = new Vector3(feature.Center[0] * 0.5f, featureClone.transform.localPosition.y, feature.Center[1] * 0.5f);
            featureClone.name = "MeeplePlace" + positions[i].ToString();
            selectionMeeples.Add(featureClone);
        }
    }

    void RotateTile()
    {
        for (int i = 0; i < currentTilePossibleRotations.Count; i++)
        {
            if (currentTilePossibleRotations[i] == currentTileRotation)
            {
                currentTileRotation = currentTilePossibleRotations[(i + 1) % currentTilePossibleRotations.Count];
                currentTileObjectRef.transform.rotation = Quaternion.Euler(0.0f, currentTileRotation * 90, 0.0f);
                break;
            }
        }
    }

    void AddMeeple(GameObject place, int featureIndex)
    {
        Vector3 o = place.transform.localPosition;
        CreateMeeple(MeepleColor.Red, new Vector3(o.x - 0.22f, o.y, o.z - 0.22f));
        photonView.RPC("CreateMeeple", RpcTarget.Others,MeepleColor.Blue, new Vector3(o.x - 0.22f, o.y, o.z - 0.22f));
        foreach (var go in selectionMeeples)
        {
            Destroy(go);
        }
        gameRunner.GameBoard.PlaceMeeple(currentPlacedTile, new Meeple(MeepleColor.Red), featureIndex);
    }

    /*
    1. null tile => touch selection tile [Update()]
    2. touch already selected tile = rotate [Update()]
    3. confirm move => [button=ConfirmTilePlacement()]
    4. place meeple or press skip [Update()=AddMeeple()+ConfirmMove()/button+ConfirmMove]
    */
    public void ConfirmTilePlacement()
    {
        currentTileObjectRef.transform.Find("ArrowPlace").gameObject.SetActive(false);
        currentPlacedTile = gameRunner.AddTileInPositionAndRotation(currentTile, (ConvertUnityToLibCarcassonneCoords(currentTilePosition)), currentTileRotation);
        var possiblePositionsForMeeple = currentPlacedTile.GetPossiblePositionsForMeeple();
        currentState = TurnLogicState.CONFIRMED_TILE_POSITION;
        confirmTileButton.SetActive(false);
        skipMeepleButton.SetActive(true);

        if (possiblePositionsForMeeple == null)
        {
            ConfirmMove();
            return;
        }

        SetMeeplePositions(possiblePositionsForMeeple);
    }

    public void ConfirmMove()
    {
        if (currentState != TurnLogicState.CONFIRMED_TILE_POSITION && currentState != TurnLogicState.PLACED_MEEPLE)
        {
            Debug.LogError("Wrong state action");
            return;
        }

        currentTile = gameRunner.GetCurrentRoundTile();
        SetNextTile(currentTile.GetIndex() - 1);
        CreateSelectionTiles();
        currentTileObjectRef = null;
        currentState = TurnLogicState.NONE;
        confirmTileButton.SetActive(false);
        skipMeepleButton.SetActive(false);

        Debug.Log(gameRunner.GameBoard.ToString());
    }

    // Utils
    (int, int) ConvertLibCarcassonneCoordsToUnity((int, int) tuple)
    {
        return (- 72 + tuple.Item2, 72 - tuple.Item1);
    }

    (int, int) ConvertUnityToLibCarcassonneCoords((int, int) tuple)
    {
        return (72 - tuple.Item2, 72 + tuple.Item1);
    }

}