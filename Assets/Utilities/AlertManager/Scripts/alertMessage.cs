//AlertMessage by Daniel Snd (http://snddev.tumblr.com/utilities) is licensed under:
/* The MIT License (MIT)
Copyright (c) 2016 Daniel Snd
Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the �Software�), to deal in the
Software without restriction, including without limitation the rights to use, copy,
modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
and to permit persons to whom the Software is furnished to do so, subject to the
following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED �AS IS�, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;

public class alertMessage : MonoBehaviour {
    public Text txt;
    public AlertType type;
    public Vector3 worldPos;
    public RectTransform mRectTransform;
    public Transform followTransform;
    public RectTransform ownerRectTransform;
    private CanvasGroup cg;
    public float yOffset = 0;
    public bool permanent = false;
    public static Dictionary<Transform,List<alertMessage>> followTransformDictionary = new Dictionary<Transform, List<alertMessage>>();

    // Use this for initialization
    void Awake()
    {
        txt = GetComponentInChildren<Text>();
        mRectTransform = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        Color tempColor = txt.color;
        txt.color = tempColor;
    }

    void OnEnable()
    {
        txt.transform.localPosition = Vector3.zero;
        yOffset = 0;
    }

    public void SetText(string myText, Color alertColor, float disappearIn = 2.5f)
    {
        //txt.DOKill();
        transform.DOKill();
        txt.color = alertColor;
        cg.alpha = 0;
        cg.DOFade(1, 0.3f);
        txt.text = myText;
        txt.transform.parent.DOKill();
        txt.transform.parent.DOShakeScale(0.3f, 0.3f);
        if (disappearIn != 0)
        {
            permanent = false;
            StartCoroutine(Disappear(disappearIn));
        }
        else
        {
            permanent = true;
        }
    }

    public void SetUpWorldFollow(RectTransform _alertMessageCanvasRect, Vector3 _desiredPos)
    {
        ownerRectTransform = _alertMessageCanvasRect;
        followTransform = null;
        worldPos = _desiredPos;
        StartCoroutine(DOFollowWorld());
    }

    public void SetUpWorldFollow(RectTransform _alertMessageCanvasRect, Transform _desiredTransform)
    {
        ownerRectTransform = _alertMessageCanvasRect;
        followTransform = _desiredTransform;
        StartCoroutine(DOFollowTransform());
    }

    public IEnumerator DOFollowWorld()
    {
        while (gameObject.activeInHierarchy)
        {
            Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(worldPos);
            Vector2 WorldObject_ScreenPosition = new Vector2(
            ((ViewportPosition.x * ownerRectTransform.sizeDelta.x) - (ownerRectTransform.sizeDelta.x * 0.5f)),
            ((ViewportPosition.y * ownerRectTransform.sizeDelta.y) - (ownerRectTransform.sizeDelta.y * 0.5f)));

            mRectTransform.anchoredPosition = WorldObject_ScreenPosition;
            yield return null;
        }
        yield return null;
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(Disappear(0.3f));
        }
    }

    public IEnumerator DOFollowTransform()
    {
        NudgeOlderMessagesOnTransform();

        while (gameObject.activeInHierarchy && followTransform != null && followTransform.gameObject.activeInHierarchy)
        {
            Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(followTransform.position+(Vector3.up*0.9f));
            Vector2 WorldObject_ScreenPosition = new Vector2(
            ((ViewportPosition.x * ownerRectTransform.sizeDelta.x) - (ownerRectTransform.sizeDelta.x * 0.5f)),
            (yOffset + (ViewportPosition.y * ownerRectTransform.sizeDelta.y) - (ownerRectTransform.sizeDelta.y * 0.5f)));

            mRectTransform.anchoredPosition = WorldObject_ScreenPosition;
            yield return null;
        }
        yield return null;
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(Disappear(0.3f));
        }
    }

    public IEnumerator Disappear(float timeToDisappear)
    {
        yield return new WaitForSeconds(timeToDisappear);
        cg.DOFade(0, 0.15f);
        yield return new WaitForSeconds(0.15f);
        AlertManager.instance.removeFromList(this, type);
        yield return null;
        AlertManager.RecycleAlertToPool(this);
    }

    public void NudgeOlderMessagesOnTransform()
    {
        if (!followTransform) return;

        AddTransformToDictionary();

        foreach (alertMessage _message in followTransformDictionary[followTransform])
        {
            _message.yOffset += 24;
        }

        followTransformDictionary[followTransform].Add(this);
    }

    void OnDisable()
    {
        if (followTransform && followTransformDictionary.ContainsKey(followTransform) && followTransformDictionary[followTransform].Contains(this))
            followTransformDictionary[followTransform].Remove(this);

        if (followTransform && followTransformDictionary.ContainsKey(followTransform) && followTransformDictionary[followTransform].Count == 0) followTransformDictionary.Remove(followTransform);
    }

    public void AddTransformToDictionary()
    {
        if (followTransform != null && !followTransformDictionary.ContainsKey(followTransform))
        {
            var list = new List<alertMessage>();
            followTransformDictionary.Add(followTransform, list);
        }
    }

    public static bool hasPermanentMessage(Transform obj)
    {
        if (!obj || !followTransformDictionary.ContainsKey(obj)) return false;

        for (int i = followTransformDictionary[obj].Count; i-- > 0;)
            if (followTransformDictionary[obj][i].permanent)
                return true;

        return false;
    }
}
