using System;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class AIBundle : ScriptableObject
	{
		public int MapId = 1;

		[NonSerialized]
		public TextAsset[] Track0;

		[NonSerialized]
		public TextAsset[] Track1;

		[NonSerialized]
		public TextAsset[] Track2;

		[NonSerialized]
		public TextAsset[] Track3;

		[NonSerialized]
		public TextAsset[] Track4;

		[NonSerialized]
		public TextAsset[] Track5;

		public byte[] GetAI(int track, int aiId, bool cycleIndex)
		{
			if (track < 6)
			{
				TextAsset[] array = null;
				switch (track)
				{
					case 0:
						array = Track0;
						break;
					case 1:
						array = Track1;
						break;
					case 2:
						array = Track2;
						break;
					case 3:
						array = Track3;
						break;
					case 4:
						array = Track4;
						break;
					case 5:
						array = Track5;
						break;
				}
				if (array != null && array.Length != 0)
				{
					if (cycleIndex)
					{
						TextAsset textAsset = array[aiId % array.Length];
						if (textAsset != null)
						{
							return textAsset.bytes;
						}
					}
					else if (aiId >= 0 && aiId < array.Length)
					{
						TextAsset textAsset2 = array[aiId];
						if (textAsset2 != null)
						{
							return textAsset2.bytes;
						}
					}
				}
			}
			return null;
		}
	}
}
