using UnityEngine;

public class EffectToggleFx : EffectToggleBase
{
	private GameObject[] children;

	public bool _isAcitive;

	public override bool Active
	{
		get
		{
			return _isAcitive;
		}
		set
		{
			_isAcitive = value;
			for (int i = 0; i < children.Length; i++)
			{
				children[i].SetActive(value);
			}
		}
	}

	public override void Init()
	{
		children = new GameObject[base.transform.childCount];
		for (int i = 0; i < base.transform.childCount; i++)
		{
			children[i] = base.transform.GetChild(i).gameObject;
		}
	}
}
