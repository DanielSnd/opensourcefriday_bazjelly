using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// TurnManager is in charge of anything turn-related.
/// It also keeps track of ticking the actors.
/// </summary>
public class TurnManager : MonoBehaviour {

    /// <summary>
    /// This is where we actually keep the singleton value.
    /// </summary>
    private static TurnManager _instance;

    /// <summary>
    /// TurnManager is a singleton, and this is how we access it.
    /// </summary>
    public static TurnManager get
    {
        get { return _instance; }
        set { _instance = value; }
    }

    /// We store the last turn time here and check for it on Update.
    float lastTurnTime;
    
    /// <summary>
    /// Stores Turn Duration
    /// </summary>
    public float turnDuration = 25;

    /// <summary>
    /// Store the Text reference that shows time left visually
    /// </summary>
    public Text timeLeftText;

    /// <summary>
    /// We use these images to show the turn duration visually.
    /// </summary>
    public Image turnCountdownBar1;
    /// <summary>
    /// We use this second image to also show the turn duration visually.
    /// </summary>
    public Image turnCountdownBar2;

    private int turnsForWindChange = 4;

    /// <summary>
    /// Awake is called first when the object is spawned.
    /// </summary>
    void Awake()
    {
        //Here we set our singleton.
        get = this;

        //And we set our first Turntime reference.
        lastTurnTime = Time.realtimeSinceStartup;
    }

    void Start()
    {
        timeLeftText.transform.DOScale(Vector3.one*1.1f, 0.45f).SetLoops(-1, LoopType.Yoyo);
    }

    /// <summary>
    /// Update is called every frame.
    /// </summary>
    void Update()
    {
        //Here we update the fillamount of the images, so they shrink as the turn time passes.
        //We're using 2 images with opposite directions so they both shrink towards the center.
        turnCountdownBar1.fillAmount = Mathf.Clamp(1-(Time.realtimeSinceStartup - lastTurnTime)/turnDuration, 0, 1);
        turnCountdownBar2.fillAmount = Mathf.Clamp(1-(Time.realtimeSinceStartup - lastTurnTime) / turnDuration, 0, 1);

        int secondsLeft = Mathf.CeilToInt(turnDuration - (Time.realtimeSinceStartup - lastTurnTime));

        timeLeftText.text = secondsLeft.ToString() + " seconds left";

        //Check if we should do a turn tick now
        if (CanDoTick())
        {
            //Time to do a new turn tick.
            TurnTick();
        }
    }

    /// <summary>
    /// If the last turn time saved + the turn duration is smaller than the current time
    /// it means that it's time to do another tick.
    /// </summary>
    /// <returns>True/False if can or can't do tick</returns>
    private bool CanDoTick()
    {
        return lastTurnTime + turnDuration < Time.realtimeSinceStartup;
    }

    /// <summary>
    /// This method actually does the Turn Tick.
    /// </summary>
    void TurnTick()
    {
        //Debug log so we can see the tick happening on the Console.
        Debug.Log("DO TICK");
        turnsForWindChange--;
        if (turnsForWindChange == 0)
        {
            GameManager.get.SetWind(Random.Range(-50,50));
            turnsForWindChange = 3;
        }
        StartCoroutine(DOTick());
    }

    IEnumerator DOTick()
    {
        //We update our last turn time reference with the current time.
        lastTurnTime = Time.realtimeSinceStartup;

        TwitchController.usersVotedThisTurn.Clear();

        List<Actor> tickList = Actor.actorList.OrderByDescending(x => x.priority).ToList();

        //We go over every actor currently in the board and ask them to Tick.
        for (int i = tickList.Count - 1; i >= 0; i--)
        {
            tickList[i].Tick();
            yield return new WaitForSeconds(tickList[i].priority == 0 ? 0.3f : 0.1f);
        }
    }
}
