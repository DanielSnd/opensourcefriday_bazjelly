using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// This is our base Actor class, will be extended
/// later to handle players, enemies, and other actors.
/// </summary>
public class Actor : CacheBehaviour
{
    public Image healthBar;
    public Transform angleIndicator;
    public Transform shotSpawnPoint;
    public Image hasCommandIcon;
    public Image powerBarImage;

    public int maxHealth = 10;
    public int currentHealth;
    public int baseDamage = 1;
    public int priority = 0;

    public Weapon weaponPrefab;

    //Actor's name reference
    private string _name;
    
    /// <summary>
    /// Static actorlist so we can access actors from anywhere.
    /// </summary>
    public static List<Actor> actorList = new List<Actor>();

    /// <summary>
    /// Property to get and set the Text reference for the Canvas Text above Actor's head.
    /// </summary>
    public new Text nameText { get { return _nameText ? _nameText : (_nameText = GetComponentInChildren<Text>()); } }
    [HideInInspector, NonSerialized]
    private Text _nameText;

    //Next command to execute on Tick.
    public int angle = None;

    public int power = 30;

    public const int None = -1;

    /// <summary>
    /// Property to get the name and set. Whenever the name is set it'll update the text above actor's head.
    /// </summary>
    public string actorName
    {
        get { return _name;}
        set
        {
            _name = value;
            nameText.text = value;
        }
    }
    
    /// <summary>
    /// On Enable we'll add it to the static list.
    /// On Enable is called when the object becomes visible.
    /// </summary>
    public virtual void OnEnable()
    {
        if (!actorList.Contains(this)) actorList.Add(this);
        angle = -1;
        UpdatePower(30);
        transform.localScale = Vector3.one;
        currentHealth = maxHealth;
        angleIndicator.gameObject.SetActive(false);
        UpdateHealthBar();
    }

    /// <summary>
    /// On Disable we'll remove it from the static list.
    /// On Disable is called when the object becomes invisible.
    /// </summary>
    public virtual void OnDisable()
    {
        if (actorList.Contains(this)) actorList.Remove(this);
    }

    /// <summary>
    /// Here's where the player is going to do it's stuff on the turn tick.
    /// </summary>
    public virtual void Tick()
    {
        //DO STUFF
        Debug.Log(actorName + " TICK! ");
        if (angle != None && currentHealth>0)
        {
            Shoot();
        }
        
        if (hasCommandIcon)
        {
            hasCommandIcon.enabled = false;
        }
        UpdateHealthBar();
    }
    
    public void Shoot()
    {
        Weapon shotWeapon = weaponPrefab.Spawn(shotSpawnPoint.transform.position);
        Vector2 shootForce = GetVForce() * (power*0.15f);
        shotWeapon.rigidbody2D.AddForce(shootForce, ForceMode2D.Impulse);
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        spriteRenderer.transform.DOKill();
        ResetScale();
        spriteRenderer.transform.DOPunchPosition(Vector3.one*1.5f, 0.5f);

        if (currentHealth <= 0)
        {
            Die();
        }
        UpdateHealthBar();
    }

    public void ResetScale()
    {
        spriteRenderer.transform.localScale = Vector3.one;
    }

    public void Die()
    {
        if (GameManager.enemyDictionary.ContainsKey(actorName)) GameManager.enemyDictionary.Remove(actorName);
        if (GameManager.playerDictionary.ContainsKey(actorName)) GameManager.playerDictionary.Remove(actorName);
        if (actorList.Contains(this)) actorList.Remove(this);

        StartCoroutine(DODie());
    }
    
    public IEnumerator DODie()
    {
        transform.DOKill();
        Tween dieTween = transform.DOScale(Vector3.zero, 0.5f);
        yield return dieTween.WaitForCompletion();
        GameManager.get.UpdateInstructionsText();
        this.Recycle();
    }

    public void UpdateAngle(int newAngle)
    {
        angle = newAngle;
        angleIndicator.transform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    public void UpdatePower(int _power)
    {
        powerBarImage.fillAmount = ((float)_power / (float)100);
        power = _power;
    }

    public void CommandReceived()
    {
        transform.DOKill();
        transform.DOShakeScale(0.2f, 0.3f).OnComplete(ResetScale);

        if (!angleIndicator.gameObject.activeInHierarchy) angleIndicator.gameObject.SetActive(true);

        hasCommandIcon.enabled = true;
    }

    public Vector2 GetVForce()
    {
        return new Vector2(angleIndicator.transform.right.x, angleIndicator.transform.right.y);
    }

    public void UpdateHealthBar()
    {
        if (!healthBar) return;
        healthBar.fillAmount = 1-((float)currentHealth/ (float)maxHealth);
    }

    public static float GetNewUnpopulatedPos()
    {
        for (int i = 0; i < 15; i++)
        {
            float tryPos = Random.Range(-11, 11);
            if (!AnyActorsInRange(tryPos)) return tryPos;
        }
        return Random.Range(-11, 11);
    }

    public static bool AnyActorsInRange(float x)
    {
        foreach (var _Actor in GameManager.playerDictionary.Values)
        {
            if (Mathf.Abs(_Actor.transform.position.x - x) < 0.2f) return true;
        }
        return false;
    }
}
