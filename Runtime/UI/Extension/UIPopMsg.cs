using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using ZGame.Sound;

namespace ZGame.UI
{
    public class UIPopMsg : MonoBehaviour
    {
        public GameObject textMsgPopPanle;
        public TMP_Text textPanle;
        public GameObject audioMsgPopPanle;
        private Coroutine coroutine;
        private AudioClip clip;

        public UnityEvent<object> onCompletion;
        public UnityEvent<object> onShowPopMsg;
        public bool isPlaying => coroutine != null;

        private void Awake()
        {
            onCompletion = new UnityEvent<object>();
            onShowPopMsg = new UnityEvent<object>();
        }


        public void Stop()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            if (clip != null)
            {
                SoundManager.instance.StopSound(clip.name);
            }

            coroutine = null;
            textMsgPopPanle.SetActive(false);
            audioMsgPopPanle.SetActive(false);
        }


        public void ShowNow(string text, float time = 2f, object userData = null)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            coroutine = StartCoroutine(PopMsgCoroutine(text, null, time, userData));
        }

        public void ShowNow(string text, AudioClip clip, object userData)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            coroutine = StartCoroutine(PopMsgCoroutine(text, clip, 0, userData));
        }

        public void ShowNow(AudioClip clip, object userData = null)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            coroutine = StartCoroutine(PopMsgCoroutine(String.Empty, clip, 0, userData));
        }

        private IEnumerator PopMsgCoroutine(string text, AudioClip clip, float time, object userData)
        {
            textPanle.text = text;
            textMsgPopPanle.SetActive(text.IsNullOrEmpty() is false);
            audioMsgPopPanle.SetActive(clip != null && text.IsNullOrEmpty());
            onShowPopMsg?.Invoke(userData);
            if (clip != null)
            {
                bool isCompletion = false;
                this.clip = clip;
                SoundManager.instance.PlayEffectSound(clip, state => isCompletion = state == PlayState.Complete);
                yield return new WaitUntil(() => isCompletion);
            }
            else
            {
                yield return new WaitForSeconds(time);
            }

            textMsgPopPanle.SetActive(false);
            audioMsgPopPanle.SetActive(false);
            coroutine = null;
            this.clip = null;
            onCompletion?.Invoke(userData);
        }
    }
}