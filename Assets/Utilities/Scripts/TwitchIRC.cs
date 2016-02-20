/* https://github.com/Grahnz/TwitchIRC-Unity
The MIT License (MIT)
Copyright (c) 2015 Grahnz https://github.com/Grahnz/TwitchIRC-Unity/blob/master/LICENSE
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

public class TwitchIRC : MonoBehaviour
{
    [HideInInspector]
    public string oauth;
    public string nickName;
    public string channelName;
    private string server = "irc.twitch.tv";
    private int port = 6667;

    public bool canSendMessages = false;

    //event(buffer).
    public class MsgEvent : UnityEngine.Events.UnityEvent<string> { }
    public MsgEvent messageReceivedEvent = new MsgEvent();
    private TcpClient IRC;
    private Queue<string> commandQueue = new Queue<string>();

    private NetworkStream ircStream;
    private string buffer = string.Empty;
    private StreamReader input;
    private StreamWriter output;
    private float lastTimeSentCommand;
    private bool connected = false;

    public void StartIRC()
    {
        lastTimeSentCommand = Time.realtimeSinceStartup - 1;

        if (string.IsNullOrEmpty(oauth))
        {
            oauth = "213123141234123";
        }
        if (string.IsNullOrEmpty(nickName))
        {
            nickName = "justinfan12345";
        }
        canSendMessages = false;

        IRC = new TcpClient(server, port);

        ircStream = IRC.GetStream();
        input = new StreamReader(ircStream);
        output = new StreamWriter(ircStream);

        output.WriteLine("USER " + nickName.ToLower() + "tmi twitch :" + nickName.ToLower());
        output.Flush();
        output.WriteLine("PASS " + oauth);
        output.Flush();
        output.WriteLine("NICK " + nickName.ToLower());
        output.Flush();
    }

    public void SendCommand(string cmd)
    {
        commandQueue.Enqueue(cmd);
    }

    public void SendMsg(string msg)
    {
        if (!canSendMessages) return;

        commandQueue.Enqueue("PRIVMSG #" + channelName + " :" + msg);
    }

    void Update()
    {
        // Receive Messages
        if (ircStream != null && ircStream.DataAvailable)
        {
            buffer = input.ReadLine();
            Debug.Log(buffer);
            if (buffer != null)
            {
                if (buffer.Contains("PRIVMSG #"))
                {
                    messageReceivedEvent.Invoke(buffer);
                }

                if (buffer.StartsWith("PING "))
                {
                    SendCommand(buffer.Replace("PING", "PONG"));
                }

                if (buffer.Split(' ')[1] == "001")
                {
                    SendCommand("MODE " + nickName + " +B");
                    SendCommand("JOIN #" + channelName);
                }
            }
        }

        //Send messages
        if (commandQueue.Count > 0) //do we have any commands to send?
        {
            // https://github.com/justintv/Twitch-API/blob/master/IRC.md#command--message-limit 
            //have enough time passed since we last sent a message/command?
            if (lastTimeSentCommand + 0.2f < Time.realtimeSinceStartup)
            {
                //send msg.
                output.WriteLine(commandQueue.Peek());
                output.Flush();
                //remove msg from queue.
                commandQueue.Dequeue();

                lastTimeSentCommand = Time.realtimeSinceStartup;
            }
        }
    }
}
