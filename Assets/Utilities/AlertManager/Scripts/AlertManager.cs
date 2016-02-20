//AlertManager by Daniel Snd (http://snddev.tumblr.com/utilities) is licensed under:
/* The MIT License (MIT)
Copyright (c) 2016 Daniel Snd
Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the “Software”), to deal in the
Software without restriction, including without limitation the rights to use, copy,
modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
and to permit persons to whom the Software is furnished to do so, subject to the
following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. */
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public enum AlertType
{
    Middle,
    Top,
    Bottom,
    Random,
    World
}

public class AlertManager : MonoBehaviour {
    private static AlertManager _instance;
    public alertMessage alertMsgPrefab;
    [HideInInspector]
    public RectTransform mCanvasRect;
    [HideInInspector]
    public List<alertMessage> messagesMiddle = new List<alertMessage>();
    [HideInInspector]
    public List<alertMessage> messagesTop = new List<alertMessage>();
    [HideInInspector]
    public List<alertMessage> messagesBottom = new List<alertMessage>();
    [HideInInspector]
    public List<alertMessage> messagesRandom = new List<alertMessage>();
    [HideInInspector]
    public List<alertMessage> messagesWorld = new List<alertMessage>();
    Dictionary<alertMessage, List<alertMessage>> pooledObjects = new Dictionary<alertMessage, List<alertMessage>>();
    Dictionary<alertMessage, alertMessage> spawnedObjects = new Dictionary<alertMessage, alertMessage>();
    
    public static AlertManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<AlertManager>();

                if (!_instance)
                {
                    GameObject singleton = (GameObject)Instantiate(Resources.Load("UI/AlertManager"));
                    singleton.name = "AlertManager";
                    _instance = singleton.GetComponent<AlertManager>();
                }
                //Tell unity not to destroy this object when loading a new scene!
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }
    
    public static bool isShuttingDown;
    
    //Unity calls this function when it's about to quit
    void OnApplicationQuit()
    {
        isShuttingDown = true;
    }


    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != _instance)
                Destroy(this.gameObject);
        }
        mCanvasRect = GetComponent<RectTransform>();
    }


    public static void Alert(string text, Color alertColor, AlertType type = AlertType.Top, float duration = 1.4f, float scale=1 )
    {
        alertMessage msg = SpawnAlert(instance.alertMsgPrefab);
        msg.transform.SetParent(instance.transform);
        Vector3 desiredLocalPosition = Vector3.zero;
        switch (type)
        {
            case AlertType.Middle:
                if (instance.messagesMiddle.Count > 0)
                    for (int i = 0; i < instance.messagesMiddle.Count; i++)
                        instance.messagesMiddle[i].transform.DOLocalMoveY(instance.messagesMiddle[i].transform.localPosition.y + 90, 0.2f);
                instance.messagesMiddle.Add(msg);
                desiredLocalPosition.y += 20f;
                msg.transform.localPosition = desiredLocalPosition;

                msg.transform.localScale = Vector3.one * scale;
                break;
            case AlertType.Random:
                instance.messagesRandom.Add(msg);
                desiredLocalPosition.y += Random.value<0.5f ? Random.Range(-150,-60) : Random.Range(60,150);
                desiredLocalPosition.x += Random.value < 0.5f ? Random.Range(-220, -80) : Random.Range(80, 220);
                msg.transform.localPosition = desiredLocalPosition;

                msg.transform.localScale = Vector3.one * 0.85f * scale;
                break;
            case AlertType.Top:
                if (instance.messagesTop.Count > 0)
                    for (int i = 0; i < instance.messagesTop.Count; i++)
                        instance.messagesTop[i].transform.DOLocalMoveY(instance.messagesTop[i].transform.localPosition.y + 75, 0.2f);
                instance.messagesTop.Add(msg);
                
                desiredLocalPosition.y = 260;
                msg.transform.localPosition = desiredLocalPosition;
                msg.transform.localScale = Vector3.one * 0.85f * scale;
                break;
            case AlertType.Bottom:
                if (instance.messagesBottom.Count > 0)
                    for (int i = 0; i < instance.messagesBottom.Count; i++)
                        instance.messagesBottom[i].transform.DOLocalMoveY(instance.messagesBottom[i].transform.localPosition.y + 75, 0.2f);
                instance.messagesBottom.Add(msg);
                
                desiredLocalPosition.y = -230;
                msg.transform.localPosition = desiredLocalPosition;
                msg.transform.localScale = Vector3.one * 0.85f * scale;
                break;
        }
        msg.type = type;
        msg.SetText(text, alertColor, duration);
    }

    public static void AlertWorld(string text,Vector3 worldPos,Color messageColor, float duration = 1.4f, float scale=1)
    {
        alertMessage msg = SpawnAlert(instance.alertMsgPrefab);
        msg.transform.SetParent(instance.transform);
        msg.transform.localScale = Vector3.one * 0.45f * scale;

        msg.SetUpWorldFollow(instance.mCanvasRect,worldPos);

        instance.messagesWorld.Add(msg);
        msg.type = AlertType.World;
        msg.SetText(text, messageColor, duration);
    }

    public static void AlertWorld(string text, Transform posFollower, Color messageColor, float duration = 1.4f, float scale = 1)
    {
        alertMessage msg = SpawnAlert(instance.alertMsgPrefab);
        msg.transform.SetParent(instance.transform);

        msg.transform.localScale = Vector3.one * 0.3f * scale;

        msg.SetText(text, messageColor, duration);
        msg.SetUpWorldFollow(instance.mCanvasRect, posFollower);
        instance.messagesWorld.Add(msg);
        msg.type = AlertType.World;
    }
    
    /// <summary>
    /// Recycle and hide all of the messages on the screen.
    /// </summary>
    public static void ClearAll()
    {
        instance.doClearAll();
    }

    public void doClearAll()
    {
        foreach (var _alertMessage in messagesMiddle)
        {
            RecycleAlertToPool(_alertMessage);
        }
        foreach (alertMessage _alertMessage in messagesTop)
        {
            RecycleAlertToPool(_alertMessage);
        }
        foreach (alertMessage _alertMessage in messagesWorld)
        {
            RecycleAlertToPool(_alertMessage);
        }
        foreach (alertMessage _alertMessage in messagesBottom)
        {
            RecycleAlertToPool(_alertMessage);
        }
        foreach (alertMessage _alertMessage in messagesRandom)
        {
            RecycleAlertToPool(_alertMessage);
        }
        messagesWorld.Clear();
        messagesMiddle.Clear();
        messagesTop.Clear();
        messagesBottom.Clear();
        messagesRandom.Clear();
    }

    public void removeFromList(alertMessage theMessage, AlertType type)
    {
        switch (type)
        {
            case AlertType.Middle:
                messagesMiddle.Remove(theMessage);
                break;
            case AlertType.Top:
                messagesTop.Remove(theMessage);
                break;
            case AlertType.Bottom:
                messagesBottom.Remove(theMessage);
                break;
            case AlertType.Random:
                messagesRandom.Remove(theMessage);
                break;
            case AlertType.World:
                messagesWorld.Remove(theMessage);
                break;
        }
    }
    
    #region PoolingSystem
    /// <summary>
    /// Adds this Alert prefab to our pool of spawnable prefabs
    /// </summary>
    /// <param name="prefab">Soundgroup prefab to add</param>
    public static void AddToPool(alertMessage prefab)
    {
        if (isShuttingDown) return;
        // If the prefab isn't null and it doesn't exist on our dictionary yet, add it.
        if (prefab != null && !instance.pooledObjects.ContainsKey(prefab))
        {
            var list = new List<alertMessage>();
            instance.pooledObjects.Add(prefab, list);
        }
    }

    /// <summary>
    /// Spawn Alert from pool
    /// </summary>
    /// <param name="prefab">Desired Prefab to spawn</param>
    /// <param name="position">Desired spawn position</param>
    /// <returns></returns>
    public static alertMessage SpawnAlert(alertMessage prefab)
    {
        if (isShuttingDown) return null;
        List<alertMessage> list;
        alertMessage obj;
        obj = null;
        //Get a list from the dictionary with the current objects of this prefab we have on the pool
        if (instance.pooledObjects.TryGetValue(prefab, out list))
        {
            //While we don't have an object to spawn and our list still has objects in there.
            while (obj == null && list.Count > 0)
            {
                //While we haven't picked one, check if the one at 0 is one we can use.
                if (list[0] != null)
                    obj = list[0];
                //Remove the one at 0.
                list.RemoveAt(0);
            }
        }
        else
        {
            //This prefab is definitely not in the list D: let's add it there for later.
            AddToPool(prefab);
        }

        //If I still don't have an object to spawn, means my pool doesn't have that object, let's instantiate one old-style.
        if (obj == null) obj = (alertMessage)Instantiate(prefab);

        //Set the object's name to be my prefab name.
        obj.transform.name = prefab.name;

        //Parent it to the AlertManager Transform
        obj.transform.SetParent(instance.transform);
        
        //Set it's gameobject to active (if it's coming from the pool it was deactivated)
        obj.gameObject.SetActive(true);

        //Add it to the list of currently spawned objects
        instance.spawnedObjects.Add(obj, prefab);

        //Return the spawned object.
        return obj;
    }

    /// <summary>
    /// Send the soundgroup object back to the pool to be reused later.
    /// </summary>
    /// <param name="obj">Soundgroup object to recycle</param>
    public static void RecycleAlertToPool(alertMessage obj)
    {
        if (isShuttingDown || obj == null) return;

        //Try and get the prefab reference from the pool dictionary
        alertMessage groupPrefab = null;
        instance.spawnedObjects.TryGetValue(obj, out groupPrefab);

        //If the prefab couldn't be found
        if (groupPrefab == null)
        {
            //Destroy the object oldschool way, it wasn't pooled :(
            Destroy(obj.gameObject);
            return;
        }

        //If the object isn't null
        if (obj != null)
        {
            //Add it back to the pool list
            instance.pooledObjects[groupPrefab].Add(obj);
            //Remove it from the currently spawned objects list
            instance.spawnedObjects.Remove(obj);
        }

        //Parent the object back to our pool container and hide it
        obj.transform.SetParent(instance.transform);
        obj.gameObject.SetActive(false);
    }
    #endregion
}
