using UnityEngine;

public class CrashState : MonoBehaviour
{
	public int Id;

	public string HitTargetTag = "Wall";

	private int hitCount;

	public int HitFlag => hitCount;

	public void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.tag.Equals(HitTargetTag))
		{
			hitCount++;
		}
		Debug.Log(" ON Hit " + collision.collider.tag + "  at " + Time.time);
	}

	public void OnCollisionExit(Collision collision)
	{
		if (collision.collider.tag.Equals(HitTargetTag))
		{
			hitCount--;
		}
	}
}
