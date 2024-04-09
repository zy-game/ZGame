using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	internal class PathController : ControllerBase, IResetChecker
	{
		private CarView carView;

		private CarState carState;

		private ResetController reseter;

		private RacePathManager pathManager;

		private LinkedList<RacePathNode> forwardList = new LinkedList<RacePathNode>();

		private LinkedList<RacePathNode> backList = new LinkedList<RacePathNode>();

		private float pathUpdateInterval = 0.5f;

		private float pathStartTime;

		private float pastLengthUpdateInterval = 0.35f;

		private float lengthStartTime;

		private float checkDirTime;

		[SerializeField]
		private float lapPastDistance;

		internal const float MIN_FORWARD_DOT = 0f;

		internal const int MAX_CLOSEST_COUNTER = 3;

		public static float ResetDelayTime = 6f;

		private Vector3 vt1;

		private Vector3 vt2;

		private Vector3 vt3;

		private bool _checkLapCountCorrect;

		private LinkedList<RacePathNode> stack = new LinkedList<RacePathNode>();

		public float LapPastDistance
		{
			get
			{
				return lapPastDistance;
			}
			set
			{
				lapPastDistance = value;
			}
		}

		public bool CheckLapCountCorrect
		{
			get
			{
				return _checkLapCountCorrect;
			}
			set
			{
				_checkLapCountCorrect = value;
			}
		}

		public RacePathManager PathManager => pathManager;

		public object ResetUserData => carState.CurNode;

		public float ResetDelay => ResetDelayTime;

		public void Init(CarView view, CarState state)
		{
			carView = view;
			carState = state;
			reseter = carView.Reseter;
			pathManager = UnityEngine.Object.FindObjectOfType(typeof(RacePathManager)) as RacePathManager;
		}

		public override void OnActiveChange(bool active)
		{
			if ((bool)this.carView && !(pathManager == null))
			{
				CarView carView = this.carView;
				carView.OnFixedupdate = (Action)Delegate.Remove(carView.OnFixedupdate, new Action(OnFixedUpdate));
				CarView carView2 = this.carView;
				carView2.OnUpdate = (Action)Delegate.Remove(carView2.OnUpdate, new Action(OnUpdate));
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnPathNodeArrived = (Action<RacePathNode, bool>)Delegate.Remove(callBacks.OnPathNodeArrived, new Action<RacePathNode, bool>(OnReachNode));
				if (active)
				{
					CarView carView3 = this.carView;
					carView3.OnFixedupdate = (Action)Delegate.Combine(carView3.OnFixedupdate, new Action(OnFixedUpdate));
					CarView carView4 = this.carView;
					carView4.OnUpdate = (Action)Delegate.Combine(carView4.OnUpdate, new Action(OnUpdate));
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnPathNodeArrived = (Action<RacePathNode, bool>)Delegate.Combine(callBacks2.OnPathNodeArrived, new Action<RacePathNode, bool>(OnReachNode));
				}
			}
		}

		internal void OnUpdate()
		{
			if (base.Active && Time.time - checkDirTime > 0.5f)
			{
				checkDirection();
				checkDirTime = Time.time;
			}
		}

		internal void Reset()
		{
			carState.PastDistance = 0;
			carState.PastLapCount = 0;
			lapPastDistance = 0f;
		}

		internal void OnReachNode(RacePathNode node, bool isEnter)
		{
			if (base.Active && isEnter)
			{
				pathStartTime = 0f;
				carState.ClosestNode = node;
				UpdateNodes();
			}
		}

		internal void OnFixedUpdate()
		{
			if (base.Active)
			{
				if (Time.time - pathStartTime > pathUpdateInterval)
				{
					UpdateNodes();
				}
				if (Time.time - lengthStartTime > pastLengthUpdateInterval)
				{
					lengthStartTime = Time.time;
					updatePastLength();
				}
			}
		}

		private void UpdateNodes()
		{
			pathStartTime = Time.time;
			RacePathNode curNode = carState.CurNode;
			UpdateCurNode();
			if (!(curNode != carState.CurNode))
			{
				return;
			}
			OnCurNodeChange(curNode);
			try
			{
				if (carView.carState.CallBacks.OnCurNodeChanged != null)
				{
					carView.carState.CallBacks.OnCurNodeChanged(pathManager);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"{ex.Message} {ex.StackTrace}");
			}
		}

		internal void UpdateCurNode()
		{
			if (base.Active && !(pathManager == null) && !(carState.ClosestNode == null))
			{
				vt1 = carState.transform.position - carState.ClosestNode.transform.position;
				vt2 = carState.ClosestNode.transform.forward;
				vt2.y = vt1.y;
				if (Vector3.Dot(vt1, vt2) < 0f && carState.ClosestNode.ParentNode != null)
				{
					carState.CurNode = carState.ClosestNode.ParentNode;
				}
				else
				{
					carState.CurNode = carState.ClosestNode;
				}
			}
		}

		internal void CalculateClosestNode()
		{
			if (pathManager == null)
			{
				LogWarning("No path manager found.");
				return;
			}
			RacePathNode racePathNode = carState.ClosestNode;
			if (racePathNode == null)
			{
				racePathNode = pathManager.StartNode;
			}
			LinkedList<RacePathNode> linkedList = new LinkedList<RacePathNode>();
			RacePathNode racePathNode2 = racePathNode;
			for (int i = 0; i < 3; i++)
			{
				linkedList.AddLast(racePathNode2);
				if (racePathNode2.ParentNode == null)
				{
					break;
				}
				racePathNode2 = racePathNode2.ParentNode;
			}
			int count = 0;
			getChildren(racePathNode, linkedList, ref count);
			vt1 = carState.transform.position;
			vt2 = racePathNode.transform.position;
			float num = (vt2 - vt1).sqrMagnitude;
			foreach (RacePathNode item in linkedList)
			{
				vt3 = item.transform.position;
				float sqrMagnitude = (vt3 - vt1).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					racePathNode = item;
				}
			}
			carState.ClosestNode = racePathNode;
		}

		internal void OnCurNodeChange(RacePathNode old)
		{
			updateLastCorrectNode(old);
			CheckLapCount();
		}

		internal void updatePastLength()
		{
			RacePathNode lastCorrectNode = carState.LastCorrectNode;
			if (lastCorrectNode == null)
			{
				return;
			}
			Transform transform = lastCorrectNode.transform;
			vt2 = transform.forward;
			vt1 = Vector3.Project(carState.transform.position - transform.position, vt2);
			if (Vector3.Dot(vt1, vt2) < 0f)
			{
				lapPastDistance = lastCorrectNode.Distance - vt1.magnitude;
			}
			else
			{
				lapPastDistance = lastCorrectNode.Distance + vt1.magnitude;
			}
			if (_checkLapCountCorrect)
			{
				float num = (float)(int)carState.PastLapCount * pathManager.TotalLength + lapPastDistance;
				if (num - (float)carState.PastDistance >= 0.7f * pathManager.TotalLength && (int)carState.PastLapCount > 0)
				{
					--carState.PastLapCount;
				}
			}
			carState.PastDistance = (float)(int)carState.PastLapCount * pathManager.TotalLength + lapPastDistance;
		}

		private void updateLastCorrectNode(RacePathNode old)
		{
			if (isBack(old, carState.CurNode))
			{
				if (carState.CurNode.LeftNode == null)
				{
					return;
				}
				if (carState.CurNode.LeftNode == pathManager.EndNode)
				{
					if (!(carState.PastLapCount == 0))
					{
						--carState.PastLapCount;
						carState.LastCorrectNode = ((pathManager.StartNode == pathManager.EndNode) ? pathManager.EndNode.ParentNode : pathManager.EndNode);
					}
					return;
				}
			}
			if (old == null || carState.LastCorrectNode == null || Mathf.Abs(carState.LastCorrectNode.Distance - carState.CurNode.Distance) < pathManager.TotalLength * 0.3f)
			{
				carState.LastCorrectNode = carState.CurNode;
			}
		}

		public void CheckLapCount()
		{
			if (carState.CurNode == pathManager.EndNode && lapPastDistance > pathManager.TotalLength * 0.8f)
			{
				lapPastDistance = 0f;
				++carState.PastLapCount;
				if (pathManager.StartNode == pathManager.EndNode)
				{
					carState.LastCorrectNode = pathManager.StartNode;
				}
				if (carState.CallBacks.OnLapFinish != null)
				{
					carState.CallBacks.OnLapFinish(carState);
				}
			}
		}

		private void pushToNode(RacePathNode node, LinkedList<RacePathNode> list, bool back)
		{
			if (node == null || list == null)
			{
				return;
			}
			RacePathNode racePathNode = ((list.Last == null) ? pathManager.StartNode : list.Last.Value);
			if (!(racePathNode == node))
			{
				racePathNode = (back ? racePathNode.ParentNode : racePathNode.LeftNode);
				int num = 0;
				while (racePathNode != null && racePathNode != node && num < 100)
				{
					list.AddLast(racePathNode);
					Log("Add last " + racePathNode);
					racePathNode = (back ? racePathNode.ParentNode : racePathNode.LeftNode);
					num++;
				}
				list.AddLast(node);
				Log("Push to node back=" + back + " node=" + node);
				if (num >= 50)
				{
					LogWarning("Infinite cycle");
				}
			}
		}

		private void popToNode(RacePathNode node, LinkedList<RacePathNode> list)
		{
			Log("Pop to node node=" + node);
			if (!(node == null) && list != null)
			{
				LinkedListNode<RacePathNode> last = list.Last;
				int num = 0;
				while (last != null && last.Value != node && num < 100)
				{
					list.RemoveLast();
					last = list.Last;
					num++;
				}
				if (num >= 50)
				{
					LogWarning("Infinite cycle");
				}
			}
		}

		private bool isBack(RacePathNode old, RacePathNode cur)
		{
			if (cur == null)
			{
				return false;
			}
			if (old == null)
			{
				return Vector3.Dot(carState.transform.position - cur.transform.position, cur.transform.forward) < 0f;
			}
			if (old == pathManager.StartNode)
			{
				return Vector3.Dot(carState.transform.position - old.transform.position, old.transform.forward) < 0f;
			}
			if (old.Distance > cur.Distance)
			{
				return old.Id - cur.Id < 10;
			}
			return false;
		}

		private void getChildren(RacePathNode node, LinkedList<RacePathNode> list, ref int count)
		{
			if (!(node == null))
			{
				list.AddLast(node);
				count++;
				if (count <= 3)
				{
					getChildren(node.LeftNode, list, ref count);
				}
			}
		}

		private void checkDirection()
		{
			bool flag = true;
			if (carState.CurNode == null)
			{
				flag = false;
			}
			else
			{
				float num = Vector3.Dot(carState.transform.forward, carState.CurNode.transform.forward);
				flag = num < 0f;
			}
			if (flag != carState.DirectionWrong)
			{
				carState.DirectionWrong = flag;
			}
			if (!carState.DirectionWrong)
			{
				carState.LastRightDirTime = Time.time;
			}
		}

		internal bool IsCorrectNode(RacePathNode node)
		{
			if (carState.CurNode == null || carState.LastCorrectNode == null)
			{
				return true;
			}
			bool flag = node == carState.CurNode.LeftNode || node == carState.CurNode.RightNode;
			bool flag2 = true;
			return flag && flag2;
		}

		public bool StartToWait()
		{
			bool r = base.Active && carState.DirectionWrong;
			if (r && carState.CallBacks.InverseChecker != null)
			{
				carState.CallBacks.InverseChecker(ref r);
			}
			return r;
		}

		public bool NeedReset(object userdata)
		{
			if (!base.Active)
			{
				return false;
			}
			bool r = true;
			if (carState.CallBacks.ResetChecker != null)
			{
				carState.CallBacks.ResetChecker(ref r);
			}
			if (!r)
			{
				return false;
			}
			return StartToWait();
		}

		public bool Cancelable(object userdata)
		{
			checkDirection();
			return !StartToWait();
		}

		public void OnReset()
		{
			RacePathNode curNode = carState.CurNode;
			if (curNode == null)
			{
				LogWarning("No path node reached.");
				return;
			}
			Vector3 vector = curNode.transform.position;
			if (Physics.Raycast(vector + curNode.Up * 2f, -curNode.Up, out var hitInfo, 10f, 256))
			{
				vector = hitInfo.point;
			}
			else
			{
				Debug.LogWarning("No RoadPoint found at " + curNode.name);
			}
			vector += RaceConfig.ResetOffsetY * curNode.Up;
			carState.transform.position = vector;
			carState.transform.rotation = curNode.transform.rotation;
			if (carState.view.DriftEnable)
			{
				carState.view.DriftEnable = false;
				carState.view.DriftEnable = true;
			}
		}
	}
}
