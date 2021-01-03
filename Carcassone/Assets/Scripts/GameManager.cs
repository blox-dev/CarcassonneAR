using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.UI;
using LibCarcassonne.GameComponents;
using LibCarcassonne.GameStructures;
using LibCarcassonne.GameLogic;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPun
{
    // Board objects
    public GameObject TileRoot;
    public GameObject TilePrefab;
    public GameObject SelectorTilePrefab;
    public GameObject MeeplePrefab;
    public GameObject MeeplePlacePrefab;
    public Image NextTile;

    // UI references
    public GameObject confirmTileButton;
    public GameObject skipMeepleButton;
    public GameObject ScoresUIContent;
    public GameObject PlayerScoreUIPrefab;
    public GameObject CurrentTurnUI;

    // CoreLogic
    /* shared (pseudo-shared) variables */
    GameRunner gameRunner;
    Dictionary<int, string> playerNamesIndexes = new Dictionary<int, string>();
    List<TileComponent> tileComponents;
    Tile currentTile;
    int currentTurn;
    int totalNumberOfPlayers;
    
    Dictionary<int, GameObject> placedMeeples = new Dictionary<int, GameObject>();

    /* local-only variables */
    GameObject currentTileObjectRef;
    (int, int) currentTilePosition;
    int currentTileRotation;
    List<int> currentTilePossibleRotations = new List<int>();
    
    List<GameObject> selectionTiles = new List<GameObject>();
    List<GameObject> selectionMeeples = new List<GameObject>();
    
    int chosenMeepleIndexPosition;
    Vector3 chosenMeeplePosition;

    enum TurnLogicState
    {
        NONE,
        PLACED_TILE,
        CONFIRMED_TILE_POSITION,
        PLACED_MEEPLE
    };
    TurnLogicState currentState;

    // Network event
    const byte EventPlayerExecutedMove = 1;

    // Main functions
    void Start()
    {
        var componentManager = new ComponentManager();
        //tileComponents = componentManager.ParseJson("Assets/Scripts/LibCarcassonne/tiles_map.json");
        //tileComponents = componentManager.ParseJson(Path.Combine(Application.streamingAssetsPath, "tiles_map.json"));
        tileComponents = componentManager.ParseJson(tilesJsonRaw);
        if (tileComponents.Count != 72)
        {
            throw new Exception("Incorrect number of tiles");
        }
        currentTurn = 0;
        totalNumberOfPlayers = PhotonNetwork.PlayerList.Length;
        gameRunner = new GameRunner(tileComponents, totalNumberOfPlayers);
        
        // Making name indexes map - assigning player ids in case the player list changes (one of the player exits)
        var playerNames = new List<string>();
        foreach (var player in PhotonNetwork.PlayerList)
        {
            playerNames.Add(player.NickName);
        }
        playerNames.Sort();
        foreach (var pName in playerNames)
        {
            playerNamesIndexes.Add(playerNamesIndexes.Count, pName);
        }

        currentState = TurnLogicState.NONE;
        Init();
    }

    void Update()
    {
        if (/*is this player's turn && */Input.GetMouseButtonDown(0))
        {
            CheckInteractionWithBoard();
        }
    }

    /*
    1. null tile => touch selection tile [Update()]
    2. touch already selected tile = rotate [Update()]
    3. confirm move => [button=ConfirmTilePlacement()]
    4. place meeple or press skip [Update()=AddMeeple()+ConfirmMove()/button+ConfirmMove]
    */

    // STATE NONE:
    void Init()
    {
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
        UpdatePlayerScores();
        CurrentTurnUI.GetComponent<Text>().text = "It's " + (MeepleColor)(currentTurn%totalNumberOfPlayers) + " player's turn";
        if (playerNamesIndexes[currentTurn % totalNumberOfPlayers] == PhotonNetwork.NickName)
        {
            CreateSelectionTiles();
        }
        currentTileObjectRef = null;
    }

    void CheckInteractionWithBoard()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            if (currentState == TurnLogicState.NONE && hitInfo.collider.name.StartsWith("SelectorTile"))
            {
                var pos = hitInfo.collider.GetComponent<Transform>().localPosition;
                var newPos = ((int)pos.x, (int)pos.z);
                foreach (var fPos in gameRunner.GetFreePositionsForTile(currentTile))
                {
                    if (ConvertLibCarcassonneCoordsToUnity(fPos.Item1) == newPos)
                    {
                        currentState = TurnLogicState.PLACED_TILE;
                        PrepareHandlingTilePlacement_PLACED_TILE(newPos, fPos.Item2);
                        break;
                    }
                }
            }
            else if (currentState == TurnLogicState.PLACED_TILE && currentTileObjectRef == hitInfo.collider.gameObject.transform.parent.gameObject)
            {
                RotateTile_PLACED_TILE();
            }
            else if (currentState == TurnLogicState.CONFIRMED_TILE_POSITION && hitInfo.collider.name.StartsWith("MeeplePlace"))
            {
                currentState = TurnLogicState.PLACED_MEEPLE;
                AddMeeple_PLACED_MEEPLE(hitInfo.collider.gameObject, int.Parse(hitInfo.collider.name.Substring(11)));

                ConfirmMove();
            }
        }
    }

    // STATE PLACED_TILE
    void PrepareHandlingTilePlacement_PLACED_TILE((int,int) pos, List<int> possibleRotations)
    {
        if (currentState != TurnLogicState.PLACED_TILE)
        {
            Debug.LogError("Wrong state action");
            return;
        }

        currentTilePosition = pos;
        currentTilePossibleRotations = possibleRotations;
        currentTileRotation = currentTilePossibleRotations[0];
        CreateTile(currentTile.GetIndex() - 1, new Vector3(currentTilePosition.Item1, 0, currentTilePosition.Item2), Quaternion.Euler(0.0f, currentTileRotation * 90, 0.0f));
        DestroySelectionTiles();
        confirmTileButton.SetActive(true);
    }

    void RotateTile_PLACED_TILE()
    {
        if (currentState != TurnLogicState.PLACED_TILE)
        {
            Debug.LogError("Wrong state action");
            return;
        }

        for (var i = 0; i < currentTilePossibleRotations.Count; i++)
        {
            if (currentTilePossibleRotations[i] == currentTileRotation)
            {
                currentTileRotation = currentTilePossibleRotations[(i + 1) % currentTilePossibleRotations.Count];
                currentTileObjectRef.transform.rotation = Quaternion.Euler(0.0f, currentTileRotation * 90, 0.0f);
                break;
            }
        }
    }

    // goes to CONFIRMED_TILE_POSITION
    // called by button
    public void ConfirmTilePlacement_PLACED_TILE()
    {
        if (currentState != TurnLogicState.PLACED_TILE)
        {
            Debug.LogError("Wrong state action");
            return;
        }

        currentTileObjectRef.transform.Find("ArrowPlace").gameObject.SetActive(false);
        var possiblePositionsForMeeple = gameRunner.AddTileInPositionAndRotation(currentTile, (ConvertUnityToLibCarcassonneCoords(currentTilePosition)), currentTileRotation);
        currentState = TurnLogicState.CONFIRMED_TILE_POSITION;
        confirmTileButton.SetActive(false);

        var meepleToPlace = gameRunner.PlayerManager.GetPlayer(currentTurn % totalNumberOfPlayers).GetFreeMeeple();
        if (possiblePositionsForMeeple == null || meepleToPlace == null)
        {
            CommitMove();
            return;
        }

        skipMeepleButton.SetActive(true);

        CreateMeeplePositions(possiblePositionsForMeeple);
    }

    // STATE CONFIRMED_TILE_POSITION
    // called by button
    public void ConfirmMove()
    {
        if (currentState != TurnLogicState.PLACED_MEEPLE && currentState != TurnLogicState.CONFIRMED_TILE_POSITION)
        {
            Debug.LogError("Wrong state action");
            return;
        }

        skipMeepleButton.SetActive(false);

        CommitMove();
    }

    // STATE PLACED_MEEPLE
    void AddMeeple_PLACED_MEEPLE(GameObject place, int featureIndex)
    {
        if (currentState != TurnLogicState.PLACED_MEEPLE)
        {
            Debug.LogError("Wrong state action");
            return;
        }

        chosenMeepleIndexPosition = featureIndex;
        Vector3 o = place.transform.localPosition;
        chosenMeeplePosition = new Vector3(o.x - 0.22f, o.y, o.z - 0.22f);
    }

    // STATE CONFIRMED_TILE_POSITION + PLACED_MEEPLE
    void CommitMove()
    {
        if (currentState != TurnLogicState.PLACED_MEEPLE && currentState != TurnLogicState.CONFIRMED_TILE_POSITION)
        {
            Debug.LogError("Wrong state action");
            return;
        }

        Debug.Log(gameRunner.GameBoard.ToString());

        currentState = TurnLogicState.NONE;

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        var content = SerializeEventPlayerExecutedMoveData();

        // Revert local changes and wait for changes from event
        Destroy(currentTileObjectRef);
        currentTileObjectRef = null;
        DestroyMeeplePositions();
        chosenMeepleIndexPosition = -1;

        PhotonNetwork.RaiseEvent(EventPlayerExecutedMove, content, raiseEventOptions, SendOptions.SendReliable);
    }

    Dictionary<string, object> SerializeEventPlayerExecutedMoveData()
    {
        var data = new Dictionary<string, object>() {
            {"currentTilePosition", new []{currentTilePosition.Item1, currentTilePosition.Item2}},
            {"currentTileRotation", currentTileRotation},
            {"CreateTile/tile", currentTile.GetIndex() - 1},
            {"CreateTile/relativePosition", new Vector3(currentTilePosition.Item1, 0, currentTilePosition.Item2)},
            {"CreateTile/rotation", Quaternion.Euler(0.0f, currentTileRotation * 90, 0.0f)},
            {"chosenMeepleIndexPosition", chosenMeepleIndexPosition},
            {"chosenMeeplePosition", chosenMeeplePosition}
        };

        return data;
    }

    Dictionary<string, object> DeserializeEventPlayerExecutedMoveData(object data)
    {
        var serDict = (Dictionary<string, object>)data;
        var content = new Dictionary<string, object>() {
            {"currentTilePosition", (((int[])serDict["currentTilePosition"])[0], ((int[])serDict["currentTilePosition"])[1])},
            {"currentTileRotation", serDict["currentTileRotation"]},
            {"CreateTile/tile", serDict["CreateTile/tile"]},
            {"CreateTile/relativePosition", serDict["CreateTile/relativePosition"]},
            {"CreateTile/rotation", serDict["CreateTile/rotation"]},
            {"chosenMeepleIndexPosition", serDict["chosenMeepleIndexPosition"]},
            {"chosenMeeplePosition", serDict["chosenMeeplePosition"]}
        };

        return content;
    }

    void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != EventPlayerExecutedMove)
        { 
            return;
        }
        var data = DeserializeEventPlayerExecutedMoveData(photonEvent.CustomData);

        // 1. Create the tile
        if (playerNamesIndexes[currentTurn % totalNumberOfPlayers] != PhotonNetwork.NickName) // if i am this player, dont redo what has already been done
        {
            gameRunner.AddTileInPositionAndRotation(
                currentTile, 
                ConvertUnityToLibCarcassonneCoords(((int, int))data["currentTilePosition"]),
                (int)data["currentTileRotation"]
            );
        }
        CreateTile((int)data["CreateTile/tile"], (Vector3)data["CreateTile/relativePosition"], (Quaternion)data["CreateTile/rotation"]);
        currentTileObjectRef.transform.Find("ArrowPlace").gameObject.SetActive(false);
        
        // 2. Create the meeple
        if ((int)data["chosenMeepleIndexPosition"] != -1)
        {
            var meepleToPlace = gameRunner.PlayerManager.GetPlayer(currentTurn % totalNumberOfPlayers).GetFreeMeeple();
            gameRunner.PlaceMeeple( meepleToPlace, (int)data["chosenMeepleIndexPosition"]);
            CreateMeeple(meepleToPlace.MeepleColor, meepleToPlace.MeepleId, (Vector3)data["chosenMeeplePosition"]);
        }
        
        // 3. Commit and raise meeple
        var meeplesToRaise = gameRunner.CommitChanges();
        if (meeplesToRaise != null)
        {
            foreach (Meeple meeple in meeplesToRaise)
            {
                if (!placedMeeples.TryGetValue(meeple.MeepleId, out GameObject meepleGameObject))
                {
                    Debug.LogError("Could not find meeple in map");
                }
                Destroy(meepleGameObject);
                meeple.RaiseMeeple();
                placedMeeples.Remove(meeple.MeepleId);
            }
        }

        // 4. Advance the game
        currentTurn++;
        currentTile = gameRunner.GetCurrentRoundTile();
        if (currentTile == null)
        {
            Debug.Log("Game ended");
            return;
        }
        SetNextTile(currentTile.GetIndex() - 1);
        UpdatePlayerScores();
        CurrentTurnUI.GetComponent<Text>().text = "It's " + (MeepleColor)(currentTurn % totalNumberOfPlayers) + " player's turn";

        // and everyone should check first if the game is over
        // 5. One of the players prepares the next move
        if (playerNamesIndexes[currentTurn % totalNumberOfPlayers] == PhotonNetwork.NickName)
        {
            CreateSelectionTiles();
        }
    }

    // Networking
    public void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
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

    public void CreateMeeple(MeepleColor meepleColor, int meepleId, Vector3 relativePosition)
    {
        const float meepleHeight = 0.115f; //should not be a random constant
        var materialName = meepleColor + "_meeple";
        var material = Resources.Load<Material>("Materials/" + materialName);
        if (material)
        {
            var newMeeple = Instantiate(MeeplePrefab, currentTileObjectRef.transform);
            var rend = newMeeple.transform.GetChild(0).GetComponent<Renderer>();
            rend.material = material;
            newMeeple.transform.localPosition = new Vector3(relativePosition.x, relativePosition.y + meepleHeight, relativePosition.z);
            placedMeeples.Add(meepleId, newMeeple);
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
        selectionTiles.Clear();
    }

    void CreateSelectionTiles()
    {
        foreach (var tilePos in gameRunner.GetFreePositionsForTile(currentTile))
        {
            var pos = ConvertLibCarcassonneCoordsToUnity(tilePos.Item1);
            CreateTile(-1, new Vector3(pos.Item1, 0, pos.Item2), Quaternion.Euler(0.0f, 0.0f, 0.0f));
        }
    }

    void CreateMeeplePositions(List<int> positions)
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

    void DestroyMeeplePositions()
    {
        foreach (var go in selectionMeeples)
        {
            Destroy(go);
        }
        selectionMeeples.Clear();
    }

    void UpdatePlayerScores()
    {
        foreach (Transform child in ScoresUIContent.transform)
        {
            Destroy(child.gameObject);
        }

        for (var id = 0; id < playerNamesIndexes.Count; ++id)
        {
            var sgo = Instantiate(PlayerScoreUIPrefab, ScoresUIContent.transform);
            var player = gameRunner.PlayerManager.GetPlayer(id);
            sgo.GetComponent<Text>().text = playerNamesIndexes[id] + "(" + (MeepleColor)id + ")" + " -- " + player.PlayerPoints;
        }
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

    string tilesJsonRaw = "[{ \"name\": \"tile1\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [-1, 1, 2, 3, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.2], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [-0.5, -0.7], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [0, -0.5], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0.5, -0.7], \"neighbour\": [0] }], \"note\": \"\" }, { \"name\": \"tile2\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [-1, 1, 2, 3, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.2], \"shield\": true, \"neighbour\": [] }, { \"type\": \"field\", \"center\": [-0.5, -0.7], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [0, -0.7], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0.5, -0.7], \"neighbour\": [0] }], \"note\": \"\" }, { \"name\": \"tile3\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [-1, 1, 2, 3, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.2], \"shield\": true, \"neighbour\": [] }, { \"type\": \"field\", \"center\": [-0.5, -0.7], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [0, -0.7], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0.5, -0.7], \"neighbour\": [0] }], \"note\": \"\" }, { \"name\": \"tile4\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 1], [0, 0, 0, 1, 1], [0, 0, 1, 1, 1], [-1, 1, 1, 1, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [-0.5, 0.5], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0.5, -0.5], \"neighbour\": [0] }], \"note\": \"\" }, { \"name\": \"tile5\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 1], [0, 0, 0, 1, 1], [0, 0, 1, 1, 1], [-1, 1, 1, 1, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [-0.5, 0.5], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0.5, -0.5], \"neighbour\": [0] }], \"note\": \"\" }, { \"name\": \"tile6\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 1], [0, 0, 0, 1, 1], [0, 0, 1, 1, 1], [-1, 1, 1, 1, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [-0.5, 0.5], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0.5, -0.5], \"neighbour\": [0] }], \"note\": \"\" }, { \"name\": \"tile7\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 1], [0, 0, 0, 1, 1], [0, 0, 1, 1, 1], [-1, 1, 1, 1, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [-0.5, 0.5], \"shield\": true, \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0.5, -0.5], \"neighbour\": [0] }], \"note\": \"\" }, { \"name\": \"tile8\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 1], [0, 0, 0, 1, 1], [0, 0, 1, 1, 1], [-1, 1, 1, 1, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [-0.5, 0.5], \"shield\": true, \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0.5, -0.5], \"neighbour\": [0] }], \"note\": \"\" }, { \"name\": \"tile9\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 1, 1], [0, 0, 1, 2, 2], [0, 1, 2, 3, 3], [-1, 1, 2, 3, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [-0.5, 0.5], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [-0.5, -0.8], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [0.3, -0.3], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0.7, -0.7], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile10\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 1, 1], [0, 0, 1, 2, 2], [0, 1, 2, 3, 3], [-1, 1, 2, 3, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [-0.5, 0.5], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [-0.5, -0.8], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [0.3, -0.3], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0.7, -0.7], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile11\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 1, 1], [0, 0, 1, 2, 2], [0, 1, 2, 3, 3], [-1, 1, 2, 3, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [-0.5, 0.5], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [-0.5, -0.8], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [0.3, -0.3], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0.7, -0.7], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile12\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 1, 1], [0, 0, 1, 2, 2], [0, 1, 2, 3, 3], [-1, 1, 2, 3, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [-0.5, 0.5], \"shield\": true, \"neighbour\": [] }, { \"type\": \"field\", \"center\": [-0.5, -0.8], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [0.3, -0.3], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0.7, -0.7], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile13\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 1, 1], [0, 0, 1, 2, 2], [0, 1, 2, 3, 3], [-1, 1, 2, 3, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [-0.5, 0.5], \"shield\": true, \"neighbour\": [] }, { \"type\": \"field\", \"center\": [-0.5, -0.8], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [0.3, -0.3], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0.7, -0.7], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile14\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [-1, 2, 2, 2, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0, 0.9], \"neighbour\": [1] }, { \"type\": \"city\", \"center\": [0, 0], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0, -0.9], \"neighbour\": [1] }], \"note\": \"\" }, { \"name\": \"tile15\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [-1, 2, 2, 2, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0, 0.9], \"neighbour\": [1] }, { \"type\": \"city\", \"center\": [0, 0], \"shield\": true, \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0, -0.9], \"neighbour\": [1] }], \"note\": \"\" }, { \"name\": \"tile16\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [-1, 2, 2, 2, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0, 0.9], \"neighbour\": [1] }, { \"type\": \"city\", \"center\": [0, 0], \"shield\": true, \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0, -0.9], \"neighbour\": [1] }], \"note\": \"\" }, { \"name\": \"tile17\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 2, 2, 2, 2], [1, 2, 2, 2, 2], [1, 2, 2, 2, 2], [-1, 2, 2, 2, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9], \"neighbour\": [] }, { \"type\": \"city\", \"center\": [-0.9, 0], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0.3, -0.3], \"neighbour\": [0, 1] }], \"note\": \"\" }, { \"name\": \"tile18\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 2, 2, 2, 2], [1, 2, 2, 2, 2], [1, 2, 2, 2, 2], [-1, 2, 2, 2, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9], \"neighbour\": [] }, { \"type\": \"city\", \"center\": [-0.9, 0], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0.3, -0.3], \"neighbour\": [0, 1] }], \"note\": \"\" }, { \"name\": \"tile19\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [-1, 2, 2, 2, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0, 0], \"neighbour\": [0, 2] }, { \"type\": \"city\", \"center\": [0, -0.9], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile20\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [-1, 2, 2, 2, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0, 0], \"neighbour\": [0, 2] }, { \"type\": \"city\", \"center\": [0, -0.9], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile21\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [-1, 2, 2, 2, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0, 0], \"neighbour\": [0, 2] }, { \"type\": \"city\", \"center\": [0, -0.9], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile22\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 0, 1, 1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [-1, 1, 1, 1, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0, -0.1], \"neighbour\": [0] }], \"note\": \"\" }, { \"name\": \"tile23\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 0, 1, 1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [-1, 1, 1, 1, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0, -0.1], \"neighbour\": [0] }], \"note\": \"\" }, { \"name\": \"tile24\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 0, 1, 1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [-1, 1, 1, 1, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0, -0.1], \"neighbour\": [0] }], \"note\": \"\" }, { \"name\": \"tile25\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 0, 1, 1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [-1, 1, 1, 1, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0, -0.1], \"neighbour\": [0] }], \"note\": \"\" }, { \"name\": \"tile26\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 0, 1, 1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [-1, 1, 1, 1, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9], \"neighbour\": [] }, { \"type\": \"field\", \"center\": [0, -0.1], \"neighbour\": [0] }], \"note\": \"\" }, { \"name\": \"tile27\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 0, 1, 1], [2, 2, 2, 1, 1], [3, 3, 2, 1, 1], [-1, 3, 2, 1, 1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9] }, { \"type\": \"field\", \"center\": [0.6, -0.1], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [-0.1, 0] }, { \"type\": \"field\", \"center\": [-0.6, -0.6], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile28\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 0, 1, 1], [2, 2, 2, 1, 1], [3, 3, 2, 1, 1], [-1, 3, 2, 1, 1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9] }, { \"type\": \"field\", \"center\": [0.6, -0.1], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [-0.1, 0] }, { \"type\": \"field\", \"center\": [-0.6, -0.6], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile29\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 0, 1, 1], [2, 2, 2, 1, 1], [3, 3, 2, 1, 1], [-1, 3, 2, 1, 1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9] }, { \"type\": \"field\", \"center\": [0.6, -0.1], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [-0.1, 0] }, { \"type\": \"field\", \"center\": [-0.6, -0.6], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile30\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 0, 1, 1], [1, 1, 2, 2, 2], [1, 1, 2, 3, 3], [-1, 1, 2, 3, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9] }, { \"type\": \"field\", \"center\": [-0.6, 0], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [0.2, 0] }, { \"type\": \"field\", \"center\": [0.6, 0.6], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile31\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 0, 1, 1], [1, 1, 2, 2, 2], [1, 1, 2, 3, 3], [-1, 1, 2, 3, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9] }, { \"type\": \"field\", \"center\": [-0.6, 0], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [0.2, 0] }, { \"type\": \"field\", \"center\": [0.6, 0.6], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile32\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 0, 1, 1], [1, 1, 2, 2, 2], [1, 1, 2, 3, 3], [-1, 1, 2, 3, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9] }, { \"type\": \"field\", \"center\": [-0.6, 0], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [-0.1,-0.2] }, { \"type\": \"field\", \"center\": [0.6, 0.6], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile33\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 1, 1, 1], [2, 2, -1, 3, 3], [4, 4, 5, 6, 6], [-1, 4, 5, 6, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9] }, { \"type\": \"field\", \"center\": [-0.7, 0.3], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [-0.5, 0] }, { \"type\": \"road\", \"center\": [0.5, 0] }, { \"type\": \"field\", \"center\": [-0.5, -0.5], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, -0.5] }, { \"type\": \"field\", \"center\": [0.5, -0.5], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile34\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 1, 1, 1], [2, 2, -1, 3, 3], [4, 4, 5, 6, 6], [-1, 4, 5, 6, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9] }, { \"type\": \"field\", \"center\": [-0.7, 0.3], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [-0.5, 0] }, { \"type\": \"road\", \"center\": [0.5, 0] }, { \"type\": \"field\", \"center\": [-0.5, -0.5], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, -0.5] }, { \"type\": \"field\", \"center\": [0.5, -0.5], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile35\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 1, 1, 1], [2, 2, -1, 3, 3], [4, 4, 5, 6, 6], [-1, 4, 5, 6, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9] }, { \"type\": \"field\", \"center\": [-0.7, 0.3], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [-0.5, 0] }, { \"type\": \"road\", \"center\": [0.5, 0] }, { \"type\": \"field\", \"center\": [-0.5, -0.5], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, -0.5] }, { \"type\": \"field\", \"center\": [0.5, -0.5], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile36\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 1, 1, 1], [2, 2, 2, 2, 2], [3, 3, 3, 3, 3], [-1, 3, 3, 3, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9] }, { \"type\": \"field\", \"center\": [0, 0.2], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [0, -0.1] }, { \"type\": \"field\", \"center\": [0, -0.5], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile37\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 1, 1, 1], [2, 2, 2, 2, 2], [3, 3, 3, 3, 3], [-1, 3, 3, 3, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9] }, { \"type\": \"field\", \"center\": [0, 0.2], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [0, -0.1] }, { \"type\": \"field\", \"center\": [0, -0.5], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile38\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 1, 1, 1], [2, 2, 2, 2, 2], [3, 3, 3, 3, 3], [-1, 3, 3, 3, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9] }, { \"type\": \"field\", \"center\": [0, 0.2], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [0, -0.1] }, { \"type\": \"field\", \"center\": [0, -0.5], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile39\", \"matrix\": [ [-1, 0, 0, 0, -1], [1, 1, 1, 1, 1], [2, 2, 2, 2, 2], [3, 3, 3, 3, 3], [-1, 3, 3, 3, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0.9] }, { \"type\": \"field\", \"center\": [0, 0.2], \"neighbour\": [0] }, { \"type\": \"road\", \"center\": [0, -0.1] }, { \"type\": \"field\", \"center\": [0, -0.5], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile40\", \"matrix\": [ [-1, 0, 1, 2, -1], [0, 0, 1, 2, 2], [0, 0, 1, 2, 2], [0, 0, 1, 2, 2], [-1, 0, 1, 2, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [-0.6, 0], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0] }, { \"type\": \"field\", \"center\": [0.5, 0], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile41\", \"matrix\": [ [-1, 0, 1, 2, -1], [0, 0, 1, 2, 2], [0, 0, 1, 2, 2], [0, 0, 1, 2, 2], [-1, 0, 1, 2, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [-0.6, 0], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0] }, { \"type\": \"field\", \"center\": [0.5, 0], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile42\", \"matrix\": [ [-1, 0, 1, 2, -1], [0, 0, 1, 2, 2], [0, 0, 1, 2, 2], [0, 0, 1, 2, 2], [-1, 0, 1, 2, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [-0.6, 0], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0] }, { \"type\": \"field\", \"center\": [0.5, 0], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile43\", \"matrix\": [ [-1, 0, 1, 2, -1], [0, 0, 1, 2, 2], [0, 0, 1, 2, 2], [0, 0, 1, 2, 2], [-1, 0, 1, 2, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [-0.6, 0], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0] }, { \"type\": \"field\", \"center\": [0.5, 0], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile44\", \"matrix\": [ [-1, 0, 1, 2, -1], [0, 0, 1, 2, 2], [0, 0, 1, 2, 2], [0, 0, 1, 2, 2], [-1, 0, 1, 2, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [-0.6, 0], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0] }, { \"type\": \"field\", \"center\": [0.5, 0], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile45\", \"matrix\": [ [-1, 0, 1, 2, -1], [0, 0, 1, 2, 2], [0, 0, 1, 2, 2], [0, 0, 1, 2, 2], [-1, 0, 1, 2, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [-0.6, 0], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0] }, { \"type\": \"field\", \"center\": [0.5, 0], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile46\", \"matrix\": [ [-1, 0, 1, 2, -1], [0, 0, 1, 2, 2], [0, 0, 1, 2, 2], [0, 0, 1, 2, 2], [-1, 0, 1, 2, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [-0.6, 0], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0] }, { \"type\": \"field\", \"center\": [0.5, 0], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile47\", \"matrix\": [ [-1, 0, 1, 2, -1], [0, 0, 1, 2, 2], [0, 0, 1, 2, 2], [0, 0, 1, 2, 2], [-1, 0, 1, 2, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [-0.6, 0], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0] }, { \"type\": \"field\", \"center\": [0.5, 0], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile48\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [1, 1, 1, 0, 0], [2, 2, 1, 0, 0], [-1, 2, 1, 0, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0.6, 0.6], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0] }, { \"type\": \"field\", \"center\": [-0.5, 0.5], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile49\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [1, 1, 1, 0, 0], [2, 2, 1, 0, 0], [-1, 2, 1, 0, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0.6, 0.6], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [-0.2,-0.2] }, { \"type\": \"field\", \"center\": [-0.6,-0.6], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile50\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [1, 1, 1, 0, 0], [2, 2, 1, 0, 0], [-1, 2, 1, 0, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0.6, 0.6], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0] }, { \"type\": \"field\", \"center\": [-0.5, 0.5], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile51\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [1, 1, 1, 0, 0], [2, 2, 1, 0, 0], [-1, 2, 1, 0, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0.6, 0.6], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0] }, { \"type\": \"field\", \"center\": [-0.5, 0.5], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile52\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [1, 1, 1, 0, 0], [2, 2, 1, 0, 0], [-1, 2, 1, 0, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0.6, 0.6], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0] }, { \"type\": \"field\", \"center\": [-0.5, 0.5], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile53\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [1, 1, 1, 0, 0], [2, 2, 1, 0, 0], [-1, 2, 1, 0, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0.6, 0.6], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0] }, { \"type\": \"field\", \"center\": [-0.5, 0.5], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile54\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [1, 1, 1, 0, 0], [2, 2, 1, 0, 0], [-1, 2, 1, 0, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0.6, 0.6], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [-0.2,-0.2] }, { \"type\": \"field\", \"center\": [-0.6,-0.6], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile55\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [1, 1, 1, 0, 0], [2, 2, 1, 0, 0], [-1, 2, 1, 0, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0.6, 0.6], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [-0.2,-0.2] }, { \"type\": \"field\", \"center\": [-0.6,-0.6], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile56\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [1, 1, 1, 0, 0], [2, 2, 1, 0, 0], [-1, 2, 1, 0, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0.6, 0.6], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [-0.2,-0.2] }, { \"type\": \"field\", \"center\": [-0.6,-0.6], \"neighbour\": [] }], \"note\": \"\" }, { \"name\": \"tile57\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [1, 1, -1, 2, 2], [3, 3, 4, 5, 5], [-1, 3, 4, 5, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0, 0.5], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [-0.7, 0] }, { \"type\": \"road\", \"center\": [0.7, 0] }, { \"type\": \"field\", \"center\": [-0.7, -0.7], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0.8] }, { \"type\": \"field\", \"center\": [0.7, -0.7], \"neighbour\": [] }], \"note\": \"centrele poate\" }, { \"name\": \"tile58\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [1, 1, -1, 2, 2], [3, 3, 4, 5, 5], [-1, 3, 4, 5, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0, 0.5], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [-0.7, 0] }, { \"type\": \"road\", \"center\": [0.7, 0] }, { \"type\": \"field\", \"center\": [-0.7, -0.7], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0.8] }, { \"type\": \"field\", \"center\": [0.7, -0.7], \"neighbour\": [] }], \"note\": \"centrele poate\" }, { \"name\": \"tile59\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [1, 1, -1, 2, 2], [3, 3, 4, 5, 5], [-1, 3, 4, 5, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0, 0.5], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [-0.7, 0] }, { \"type\": \"road\", \"center\": [0.7, 0] }, { \"type\": \"field\", \"center\": [-0.7, -0.7], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0.8] }, { \"type\": \"field\", \"center\": [0.7, -0.7], \"neighbour\": [] }], \"note\": \"centrele poate\" }, { \"name\": \"tile60\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [1, 1, -1, 2, 2], [3, 3, 4, 5, 5], [-1, 3, 4, 5, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0, 0.5], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [-0.7, 0] }, { \"type\": \"road\", \"center\": [0.7, 0] }, { \"type\": \"field\", \"center\": [-0.7, -0.7], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0.8] }, { \"type\": \"field\", \"center\": [0.7, -0.7], \"neighbour\": [] }], \"note\": \"centrele poate\" }, { \"name\": \"tile61\", \"matrix\": [ [-1, 0, 1, 2, -1], [0, 0, 1, 2, 2], [3, 3, -1, 4, 4], [5, 5, 6, 7, 7], [-1, 5, 6, 7, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [-0.8, 0.8], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0.7] }, { \"type\": \"field\", \"center\": [0.8, 0.8], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [-0.7, 0] }, { \"type\": \"road\", \"center\": [0.7, 0] }, { \"type\": \"field\", \"center\": [-0.8, -0.8], \"neighbour\": [] }, { \"type\": \"road\", \"center\": [0, 0.7] }, { \"type\": \"field\", \"center\": [0.8, -0.8], \"neighbour\": [] }], \"note\": \"centrele poate\" }, { \"name\": \"tile62\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [0, 0, 1, 0, 0], [0, 0, 0, 0, 0], [-1, 0, 0, 0, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0.8, 0.8], \"neighbour\": [] }, { \"type\": \"monastery\", \"center\": [0, 0] }], \"note\": \"-\" }, { \"name\": \"tile63\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [0, 0, 1, 0, 0], [0, 0, 0, 0, 0], [-1, 0, 0, 0, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0.8, 0.8], \"neighbour\": [] }, { \"type\": \"monastery\", \"center\": [0, 0] }], \"note\": \"-\" }, { \"name\": \"tile64\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [0, 0, 1, 0, 0], [0, 0, 0, 0, 0], [-1, 0, 0, 0, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0.8, 0.8], \"neighbour\": [] }, { \"type\": \"monastery\", \"center\": [0, 0] }], \"note\": \"-\" }, { \"name\": \"tile65\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [0, 0, 1, 0, 0], [0, 0, 0, 0, 0], [-1, 0, 0, 0, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0.8, 0.8], \"neighbour\": [] }, { \"type\": \"monastery\", \"center\": [0, 0] }], \"note\": \"-\" }, { \"name\": \"tile66\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [0, 0, 1, 0, 0], [0, 0, 2, 0, 0], [-1, 0, 2, 0, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0.8, 0.8], \"neighbour\": [] }, { \"type\": \"monastery\", \"center\": [0, 0] }, { \"type\": \"road\", \"center\": [0.3, -0.6] }], \"note\": \"-\" }, { \"name\": \"tile67\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [0, 0, 1, 0, 0], [0, 0, 2, 0, 0], [-1, 0, 2, 0, -1] ], \"types\": [{ \"type\": \"field\", \"center\": [0.8, 0.8], \"neighbour\": [] }, { \"type\": \"monastery\", \"center\": [0, 0] }, { \"type\": \"road\", \"center\": [0.3, -0.6] }], \"note\": \"-\" }, { \"name\": \"tile68\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [-1, 0, 0, 0, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0], \"shield\": true }], \"note\": \"-\" }, { \"name\": \"tile69\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [-1, 1, 1, 1, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0] }, { \"type\": \"field\", \"center\": [0, 0.7], \"neighbour\": [0] }], \"note\": \"-\" }, { \"name\": \"tile70\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [-1, 1, 1, 1, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0] }, { \"type\": \"field\", \"center\": [0, 0.7], \"neighbour\": [0] }], \"note\": \"-\" }, { \"name\": \"tile71\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [-1, 1, 1, 1, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0, 0] }, { \"type\": \"field\", \"center\": [0, 0.7], \"neighbour\": [0] }], \"note\": \"-\" }, { \"name\": \"tile72\", \"matrix\": [ [-1, 0, 0, 0, -1], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [-1, 1, 1, 1, -1] ], \"types\": [{ \"type\": \"city\", \"center\": [0.4, 0.4], \"shield\": true }, { \"type\": \"field\", \"center\": [0, -0.7], \"neighbour\": [0] }], \"note\": \"centru oras\" }]";
}