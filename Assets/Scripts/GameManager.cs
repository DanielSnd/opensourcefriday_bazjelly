using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    /// <summary>
    /// Reference to store the singleton instance.
    /// </summary>
    private static GameManager _instance;

    /// <summary>
    /// This is how we get the singleton reference.
    /// </summary>
    public static GameManager get
    {
        get { return _instance; }
        set { _instance = value; }
    }

    /// <summary>
    /// Maximum amount of players to spawn at a time.
    /// </summary>
    public int maxPlayers = 4;

    /// <summary>
    /// Player prefab to spawn.
    /// </summary>
    public ActorPlayer playerPrefab;

    public ActorEnemy enemyPrefab;

    public Text instructionsText;
    public Text windText;

    /// <summary>
    /// Dictionary of usernames versus player actor objects.
    /// </summary>
    public static Dictionary<string, ActorPlayer> playerDictionary = new Dictionary<string, ActorPlayer>();

    /// <summary>
    /// Dictionary of usernames versus enemy actor objects.
    /// </summary>
    public static Dictionary<string, ActorEnemy> enemyDictionary = new Dictionary<string, ActorEnemy>();

    public bool canJoin = true;

    public const int alphabettotal = 25;

    public float wind;

    /// <summary>
    /// Awake gets called when the object is spawned into the scene.
    /// </summary>
    void Awake()
    {
        //Set our singleton to be this object.
        get = this;
    }

    public void UpdateInstructionsText()
    {
        string instructionsString = "";
        if (playerDictionary.Count < maxPlayers)
        {
            instructionsString += "<color=#FF0>#PLAY</color> TO JOIN!          ";
        }
        instructionsString += "Commands:    <color=#FF0>#a</color> angle (between 0-180) /   <color=#0FF>#p</color> power (between 10-100)";

        instructionsText.text = instructionsString;
    }
    
    /// <summary>
    /// Spawn player for the desired username using ObjectPool pooling system.
    /// Find a random empty spot on the board and move the newly spawned player there.
    /// </summary>
    /// <param name="username">desired username for player</param>
    /// <returns>Spawned Player Prefab.</returns>
    public Actor SpawnPlayer(string username)
    {
        //If there is already a player with that username don't spawn it.
        if (playerDictionary.ContainsKey(username))
        {
            return null;
        }
        
        //Spawn Player Object.
        ActorPlayer spawnedPlayer = playerPrefab.Spawn();

        //Set position.
        //spawnedPlayer.currentPosition = BoardMethod.GetRandomTile(TType.Floor, true);
        //Tile emptyTile = BoardMethod.GetTile(spawnedPlayer.currentPosition);
        //spawnedPlayer.transform.position = emptyTile.transform.position;
        //TODO: Position cannon on spawn
        spawnedPlayer.transform.position = new Vector3(Actor.GetNewUnpopulatedPos(), -0.1f, 0);

        spawnedPlayer.spriteRenderer.color = new Color(Random.value, Random.value, Random.value, 1);

        //Set username.
        spawnedPlayer.actorName = username;
        
        //Add to dictionary so we can reference it later by username.
        playerDictionary.Add(username,spawnedPlayer);
        UpdateInstructionsText();
        
        return spawnedPlayer;
    }

    public void SetWind(float _wind)
    {
        wind = _wind;
        windText.text = "Wind: <color=#F00>" + wind + "</color>";
        windText.transform.DOShakePosition(1f);
    }

    public static void ResetAll()
    {
        if (Actor.actorList.Count > 0)
        {
            foreach (Actor _actor in Actor.actorList)
            {
                if (_actor != null)
                {
                    _actor.Recycle();
                }
            }
        }
        Actor.actorList.Clear();
        enemyDictionary.Clear();
        playerDictionary.Clear();
    }
}
