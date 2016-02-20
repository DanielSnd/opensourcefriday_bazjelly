using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(TwitchIRC))]
public class TwitchController : MonoBehaviour {
    private TwitchIRC IRC;
    
    public static HashSet<string> usersVotedThisTurn = new HashSet<string>();

    // Use this for initialization
    void Start()
    {
        //Get IRC component and register to receive messages from it.
        IRC = this.GetComponent<TwitchIRC>();
        IRC.StartIRC();
        IRC.messageReceivedEvent.AddListener(OnChatMsgReceived);
    }

    /// <summary>
    /// This method will be called by IRC UnityEvent messageRecievedEvent
    /// </summary>
    /// <param name="msg">long ugly message with unnecessary stuff</param>
    void OnChatMsgReceived(string msg)
    {
        //Parse the message received and separate it's pieces.
        int msgIndex = msg.IndexOf("PRIVMSG #");
        string msgString = msg.Substring(msgIndex + IRC.channelName.Length + 11);
        string user = msg.Substring(1, msg.IndexOf('!') - 1);

        string[] param = msgString.Split(' ');

        //If user has a player and message starts with # handle it.
        if (GameManager.playerDictionary.ContainsKey(user) && msgString.ToLower().StartsWith("#"))
        {
            HandlePlayerCommand(msgString, user);
        }
        
        //If there are less than our max number of players in our player dictionary
        //and message is #join or #play spawn player.
        if (GameManager.playerDictionary.Count < GameManager.get.maxPlayers && (msgString.ToLower().StartsWith("#join") || msgString.ToLower().StartsWith("#play")) && GameManager.get.canJoin)
        {
            GameManager.get.SpawnPlayer(user);
        }

        //Show the message in the console, so we can see what it was.
        Debug.Log("["+user+"]: "+msgString);
    }
    
    /// <summary>
    /// This method receives a command and a username and adds a nextcommand
    /// to the username's player actor.
    /// </summary>
    /// <param name="msgString">message</param>
    /// <param name="user">username</param>
    private static void HandlePlayerCommand(string msgString, string user)
    {
        //Get just the command without the # and get a reference to player actor.
        string command = msgString.ToLower().Substring(msgString.LastIndexOf('#') + 1);
        Actor playerActor = GameManager.playerDictionary[user];
        
        string[] parameters = command.Split(' ');
        int commandInt = Actor.None;

        if (parameters.Length > 1)
        {
            if (!int.TryParse(parameters[1], out commandInt))
            {
                return;
            }
        }

        if (command.StartsWith("angle ") || command.StartsWith("a "))
        {
            playerActor.UpdateAngle(Mathf.Clamp(commandInt,0,180));
        }

        if (command.StartsWith("power ") || command.StartsWith("p "))
        {
            playerActor.UpdatePower(Mathf.Clamp(commandInt, 10, 100));
        }

        if (!usersVotedThisTurn.Contains(user))
        {
            usersVotedThisTurn.Add(user);
            AlertManager.Alert(user, Color.white,AlertType.Bottom, 1f, 0.5f);
            playerActor.CommandReceived();
        }
    }
    
}
