using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AbstractView : MonoBehaviour
{
	public Action OnAwake = empty;

	public Action OnStart = empty;

	public Action OnUpdate = empty;

	public Action OnFixedupdate = empty;

	public Action OnRelease = empty;

	public Action OnLateUpdate = empty;

	public Action OnQuit = empty;

	public Action OnLevelLoaded = empty;

	public Action<Collision> OnCollisionBegin = empty;

	public Action<Collision> OnCollisionEnd = empty;

	public Action<Collider> OnTriggerBegin = delegate
	{
	};

	public Action<Collider> OnTriggerEnd = delegate
	{
	};

	public Action<Collider> OnTriggerStaying = delegate
	{
	};

	public Action<bool> OnPause;

	public Action<bool> OnFocus;

	public Action AcOnEnable;

	public Action AcOnDisable;

	private List<Action> attachList = new List<Action>(4);

	protected LinkedList<TimeCallback> cdic;

	protected virtual void OnSceneWasLoaded(Scene scene, LoadSceneMode mode)
	{
		if (OnLevelLoaded != null)
		{
			OnLevelLoaded();
		}
	}

	public virtual void Awake()
	{
		SceneManager.sceneLoaded += OnSceneWasLoaded;
		if (OnAwake != null)
		{
			OnAwake();
		}
	}

	public virtual void Start()
	{
		if (OnStart != null)
		{
			OnStart();
		}
	}

	public virtual void Update()
	{
		if (OnUpdate != null)
		{
			OnUpdate();
		}
		if (attachList.Count == 0)
		{
			return;
		}
		Action[] array = null;
		lock (attachList)
		{
			array = attachList.ToArray();
			attachList.Clear();
		}
		if (array == null)
		{
			return;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				array[i]();
			}
		}
	}

	public virtual void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneWasLoaded;
		if (OnRelease != null)
		{
			OnRelease();
		}
		OnAwake = null;
		OnStart = null;
		OnUpdate = null;
		OnFixedupdate = null;
		OnRelease = null;
		OnLateUpdate = null;
		OnQuit = null;
		OnLevelLoaded = null;
		OnCollisionBegin = null;
		OnCollisionEnd = null;
		OnTriggerBegin = null;
		OnTriggerEnd = null;
		OnPause = null;
		OnFocus = null;
		AcOnEnable = null;
		AcOnDisable = null;
		attachList.Clear();
		if (cdic != null)
		{
			cdic.Clear();
		}
	}

	public virtual void FixedUpdate()
	{
		if (OnFixedupdate != null)
		{
			OnFixedupdate();
		}
	}

	public virtual void LateUpdate()
	{
		if (OnLateUpdate != null)
		{
			OnLateUpdate();
		}
	}

	public virtual void OnEnable()
	{
		if (AcOnEnable != null)
		{
			AcOnEnable();
		}
	}

	public virtual void OnDisable()
	{
		if (AcOnDisable != null)
		{
			AcOnDisable();
		}
	}

	public virtual void OnCollisionEnter(Collision collision)
	{
		if (OnCollisionBegin != null)
		{
			OnCollisionBegin(collision);
		}
	}

	public virtual void OnCollisionExit(Collision collision)
	{
		if (OnCollisionEnd != null)
		{
			OnCollisionEnd(collision);
		}
	}

	public virtual void OnTriggerEnter(Collider other)
	{
		OnTriggerBegin(other);
	}

	public virtual void OnTriggerExit(Collider other)
	{
		OnTriggerEnd(other);
	}

	public virtual void OnTriggerStay(Collider other)
	{
		OnTriggerStaying(other);
	}

	public virtual void OnApplicationPause(bool paused)
	{
		if (OnPause != null)
		{
			OnPause(paused);
		}
	}

	public virtual void OnApplicationFocus(bool focused)
	{
		if (OnFocus != null)
		{
			OnFocus(focused);
		}
	}

	public virtual void OnApplicationQuit()
	{
		OnQuit();
	}

	private static void empty()
	{
	}

	private static void empty(Collision c)
	{
	}

	public void CallDelay<T>(Action<T> callback, float delay, T obj)
	{
		StartCoroutine(delayFunc(callback, delay, obj));
	}

	private IEnumerator delayFunc<T>(Action<T> callback, float delay, T obj)
	{
		yield return new WaitForSeconds(delay);
		callback(obj);
	}

	public void CallDelay(Action callback, float delay)
	{
		if (this != null && callback != null)
		{
			TimeCallback timeCallback = new TimeCallback();
			timeCallback.ac = callback;
			timeCallback.time = Time.realtimeSinceStartup + delay;
			timeCallback.deleted = false;
			if (cdic == null)
			{
				cdic = new LinkedList<TimeCallback>();
			}
			cdic.AddLast(timeCallback);
			StartCoroutine(delayFunc2(timeCallback));
		}
	}

	public void CallDelay_TimeScale(Action callback, float delay)
	{
		if (callback != null)
		{
			TimeCallback timeCallback = new TimeCallback();
			timeCallback.ac = callback;
			timeCallback.time = Time.time + delay;
			timeCallback.deleted = false;
			if (cdic == null)
			{
				cdic = new LinkedList<TimeCallback>();
			}
			cdic.AddLast(timeCallback);
			StartCoroutine(delayFunc3(timeCallback));
		}
	}

	public void StopCallFunc(Action callback)
	{
		if (callback != null && cdic != null)
		{
			TimeCallback tc = getTc(callback);
			if (tc != null)
			{
				tc.deleted = true;
			}
		}
	}

	public void StopAllCallFunc()
	{
		if (cdic == null || cdic.Count <= 0)
		{
			return;
		}
		foreach (TimeCallback item in cdic)
		{
			item.deleted = true;
		}
	}

	private IEnumerator delayFunc(Action callback, float delay)
	{
		yield return new WaitForSeconds(delay);
		callback();
	}

	private IEnumerator delayFunc2(TimeCallback tc)
	{
		if (tc.deleted)
		{
			tc.time = -1f;
		}
		while (Time.realtimeSinceStartup < tc.time)
		{
			yield return 1;
		}
		if (!tc.deleted)
		{
			tc.ac();
		}
		if (cdic != null)
		{
			cdic.Remove(tc);
		}
	}

	private IEnumerator delayFunc3(TimeCallback tc)
	{
		if (tc.deleted)
		{
			tc.time = -1f;
		}
		while (Time.time < tc.time)
		{
			yield return 1;
		}
		if (!tc.deleted)
		{
			tc.ac();
		}
		if (cdic != null)
		{
			cdic.Remove(tc);
		}
	}

	public void Attach(Action callback)
	{
		lock (attachList)
		{
			attachList.Add(callback);
		}
	}

	private TimeCallback getTc(Action ac)
	{
		if (cdic == null || cdic.Count <= 0)
		{
			return null;
		}
		foreach (TimeCallback item in cdic)
		{
			if (ac == item.ac)
			{
				return item;
			}
		}
		return null;
	}
}
