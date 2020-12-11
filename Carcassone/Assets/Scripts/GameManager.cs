using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using LibCarcassonne.GameComponents;
using LibCarcassonne.GameStructures;
using LibCarcassonne.GameLogic;

public class GameManager : MonoBehaviour
{
    public GameObject TileRoot;
    public GameObject TilePrefab;
    public GameObject SelectorTilePrefab;
    public GameObject MeeplePrefab;
    public GameObject MeeplePlacePrefab;
    public Image NextTile;

    // TileDeck
    private System.Random rand = new System.Random();
    private uint deckIndex = 0;

    // CoreLogic
    ComponentManager componentManager = new ComponentManager();
    GameRunner gameRunner;
    List<TileComponent> tileComponents;
    Tile currentTile;
    int currentTileRotation;
    (int, int) currentTilePosition;
    GameObject currentTileObjectRef;
    List<GameObject> selectionTiles = new List<GameObject>();

    public enum MeepleColor
    {
        black,
        red,
        blue,
        breen,
        yellow
    }

    void Start()
    {
        tileComponents = componentManager.ParseJson("Assets/Scripts/LibCarcassonne/tiles_map.json");
        if (tileComponents.Count != 72)
        {
            throw new Exception("Incorrect number of tiles");
        }
        gameRunner = new GameRunner(tileComponents);

        StructureManager structureManager = new StructureManager();

        //Start tile
        currentTile = gameRunner.GetCurrentRoundTile();
        currentTileRotation = 0;
        currentTilePosition = (0, 0);
        CreateTile(currentTile.GetIndex() - 1, new Vector3(currentTilePosition.Item1, 0, currentTilePosition.Item2), Quaternion.Euler(0.0f, currentTileRotation * 90, 0.0f));
        gameRunner.AddTileInPositionAndRotation(currentTile, ConvertUnityToLibCarcassonneCoords(currentTilePosition), currentTileRotation);

        //Next tile
        currentTile = gameRunner.GetCurrentRoundTile();
        SetNextTile(currentTile.GetIndex() - 1);
        RefreshSelectionTiles();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                var pos = hitInfo.collider.GetComponent<Transform>().localPosition;
                if (hitInfo.collider.name.StartsWith("SelectorTile"))
                {
                    currentTilePosition = ((int)pos.x, (int)pos.z);
                    Debug.Log(currentTilePosition);
                    var freePositions = gameRunner.GetFreePositionsForTile(currentTile);
                    bool found = false;
                    foreach(var fPos in freePositions)
                    {
                        if (ConvertLibCarcassonneCoordsToUnity(fPos.Item1) == currentTilePosition)
                        {
                            found = true;
                            currentTileRotation = fPos.Item2[0];
                            CreateTile(currentTile.GetIndex() - 1, new Vector3(currentTilePosition.Item1, 0, currentTilePosition.Item2), Quaternion.Euler(0.0f, currentTileRotation * 90, 0.0f));
                            MakeTileChoice();
                            //SetMeeplePositions();
                            break;
                        }
                    }
                    if (!found)
                    {
                        Debug.LogError("Could not find tile in freePos");
                    }
                }
                else if (hitInfo.collider.name.StartsWith("MeeplePlace"))
                {
                    AddMeeple(hitInfo.collider.gameObject);
                }
            }
        }
    }

    // Object Creation
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

    public void CreateMeeple(MeepleColor meepleColor, Transform parent, Vector3 relativePosition)
    {
        const float meepleHeight = 0.115f; //should not be a random constant
        var materialName = meepleColor.ToString() + "_meeple";
        var material = Resources.Load<Material>("Materials/" + materialName);
        if (material)
        {
            var newMeeple = Instantiate(MeeplePrefab, parent);
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

    void RefreshSelectionTiles()
    {
        foreach (var obj in selectionTiles)
        {
            Destroy(obj);
        }
        foreach (var tilePos in gameRunner.GetFreePositionsForTile(currentTile))
        {
            var pos = ConvertLibCarcassonneCoordsToUnity(tilePos.Item1);
            CreateTile(-1, new Vector3(pos.Item1, 0, pos.Item2), Quaternion.Euler(0.0f, 0.0f, 0.0f));
        }
    }

    void AddMeeple(GameObject place)
    {
        Vector3 o = place.transform.localPosition;
        CreateMeeple(MeepleColor.red, place.transform.parent.transform, new Vector3(o.x - 0.22f, o.y, o.z - 0.22f));
        Destroy(place);
    }

    void SetMeeplePositions()
    {
        for (var i = 0; i < tileComponents[currentTile.GetIndex() - 1].Types.Count; i++)
        {
            var feature = tileComponents[currentTile.GetIndex() - 1].Types[i];
            var featureClone = Instantiate(MeeplePlacePrefab, currentTileObjectRef.transform);
            featureClone.transform.localPosition = new Vector3(feature.Center[0] * 0.5f, featureClone.transform.localPosition.y, feature.Center[1] * 0.5f);
        }
    }

    // Meta functions
    void MakeTileChoice()
    {
        var freePositions = gameRunner.GetFreePositionsForTile(currentTile);
        bool found = false;
        foreach(var fPos in freePositions)
        {
            if (ConvertLibCarcassonneCoordsToUnity(fPos.Item1) == currentTilePosition)
            {
                found = true;
                break;
            }
        }
        if (!found)
        {
            Debug.LogError("Location could not be found even though selection tile existed");
        }

        gameRunner.AddTileInPositionAndRotation(currentTile, ConvertUnityToLibCarcassonneCoords(currentTilePosition), currentTileRotation);

        currentTile = gameRunner.GetCurrentRoundTile();
        SetNextTile(currentTile.GetIndex() - 1);
        RefreshSelectionTiles();
        Debug.Log(gameRunner.GameBoard.ToString());
    }

    void RotateTile()
    {

    }

    // Utils
    (int, int) ConvertLibCarcassonneCoordsToUnity((int, int) tuple)
    {
        return (tuple.Item2 - 72, tuple.Item1 - 72);
    }

    (int, int) ConvertUnityToLibCarcassonneCoords((int, int) tuple)
    {
        return (72 - tuple.Item2, 72 + tuple.Item1);
    }
}
