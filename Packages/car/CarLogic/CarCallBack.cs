using System;

namespace CarLogic
{
	public class CarCallBack
	{
		public Action<bool> OnReachEnd;

		public Action<CarState, ItemToggle> OnAiPassItemPoint;

		public Action<byte> OnAiQtePoint;

		public Action<float> OnAiGasPoint;

		public Action<CarEventState> OnDrift = delegate
		{
		};

		public Action<CarEventState> OnGas = delegate
		{
		};

		public Action<CarEventState> OnAirGroundGas = delegate
		{
		};

		public Action<CarState, CarEventState> OnAIDrift;

		public Action OnApplyForce = delegate
		{
		};

		public Action<CarState> OnSetState = delegate
		{
		};

		public Func<CarState, float> OnSteer = (CarState obj) => obj.Steer;

		public Action<RacePathNode, bool> OnPathNodeArrived;

		public Action<CarState> OnLapFinish;

		public Action<RaceItemBase> OnGetItem;

		public Action<RaceItemBase> OnUseItem;

		public Action<RaceItemBase> OnNotifyUseItem;

		public Action<RaceItemBase, bool> OnNotifyUseToGroup;

		public Action<RaceItemId, float> OnFreezeMoveEffect;

		public Action<RaceItemId> OnCrashItemBox;

		public ModifyAction<CarState, RaceItemId> OnToggleItemBox;

		public Action<bool> PauseOnThrow;

		public Action<RaceItemId, CarState, ItemCallbackType, object> OnAffectedByItem;

		public ModifyAction<RaceItemId, bool> AffectChecker;

		public ModifyAction<bool> InverseChecker;

		public ModifyAction<bool> ResetChecker;

		public ModifyAction<RaceItemId, bool> DropDownGasChecker;

		public ModifyAction<RaceItemId, bool> ItemPauseChecker;

		public Action<CarState, SpecialTriggerBase, SpecialCallback> OnSpecialCallback;

		public Action<CarState, bool> OnDropAfterThrow;

		public Action<CarState, TranslateTrigger> OnTransQTEAffect;

		public Action<CarState> OnGoodDrift;

		public Action<CarState> OnDragDriftStart;

		public Action<CarState> OnDragDriftStop;

		public Action<CarState, CarSkillId> OnSkillToggle;

		public Action<CarState, CarChipTechnologySkillBase> OnChipTechnologySkillToggle;

		public Action<CarState, CarSkillId> OnSkillStop;

		public Action<CarState> OnItemsChange;

		public Action<CarState> OnHitWall;

		public Action<CarState, CarState> OnHitCar;

		public ModifyAction<float, float> OnDriftScrapeValue;

		public ModifyAction<float, float> OnScrapeValueRecover;

		public Action<CarState> OnUpdateScrapeValue;

		public Action OnPostCountdown;

		public Action OnGasFull;

		public Action OnSpeedUpToggle;

		public Action<bool> OnEnableMovement;

		public Action<bool> OnWaitSecondSmallGas;

		public ModifyAction<float> OnSetDeltaAcceleration;

		public Action<RacePathManager> OnCurNodeChanged;

		public Action<ItemCallbackType> OnFallDownGround;

		public Action OnPreCountdown;
	}
}
