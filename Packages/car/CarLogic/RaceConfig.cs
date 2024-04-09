using UnityEngine;

namespace CarLogic
{
	public static class RaceConfig
	{
		public static string UIRootPath = "Public/UI/Prefabs/UIRoot";

		public static string[] RACE_HSPEED_ARRAY = new string[0];

		public static int CarCount = 22;

		public static string CarModelPrefix = "Custom/Race/Prefabs/car/car{0}";

		public static string EffectModelPrefix = "Effects/Sence/effect{0:D3}";

		public static string ExhaustEffect = "Effects/Sence/weiyan";

		public static string SimpleHSEffect2 = "Other/Skidmark/jiasuxian";

		public static string TranslateEffect = "Other/Skidmark/gaosuxian";

		public static string SkidSparkEffect = "Effects/Sence/chelunhuohua0001";

		public static string AirShipDriftEffect_Back = "Effects/Sence/feichuanpiaoyi_back{0:D2}";

		public static string AirShipDriftEffect_Down = "Effects/Sence/feichuanpiaoyi_down{0:D2}";

		public static string AirShipNormalEffect_Back = "Effects/Sence/feichuanpingchang_back{0:D2}";

		public static string AirShipNormalEffect_Down = "Effects/Sence/feichuanpingchang_down{0:D2}";

		public static string HSEffect = "Other/Skidmark/gaosuxian";

		public static string SimpleN2Effect = "Other/Skidmark/gaosuxian";

		public static string CrashSpark = "Effects/Sence/huohua";

		public static string DropDownEffect = "Effects/Sence/yanwu";

		public static string TranslateQteEffect = "Effects/Sence/xuanzhuan0002";

		public static string ItemCheatBox = "Effects/Sence/ItemBox";

		public static string ItemWaterBubble = "Effects/Sence/suipao";

		public static string ItemWaterSplash = "Effects/Sence/shuihua";

		public static string ItemboxSpark = "Effects/Sence/caitiao";

		public static string ItemSingleAngel = "Effects/Sence/dunpai02";

		public static string ItemTeamAngel = "Effects/Sence/angel02";

		public static string ItemSingleDefence = "Effects/Sence/dunpai01";

		public static string ItemTeamDefence = "Effects/Sence/baohuzhao";

		public static string ItemAntiUFO = "Effects/Sence/UFO02";

		public static string ItemBanana = "Effects/Sence/dici";

		public static string ItemFog = "Effects/Sence/yun";

		public static string ItemInkBottle = "Effects/Sence/moshui01";

		public static string ItemInkEffect = "Effects/Sence/moshui";

		public static string ItemInkEffectSmallView = "Effects/Sence/moshui_smallview";

		public static string ItemMine = "Effects/Sence/dilei";

		public static string ItemEvil = "Effects/Sence/evil";

		public static string ItemExplosion = "Effects/Sence/baozha02";

		public static string ItemRocket = "Effects/Sence/rocket";

		public static string ItemUFO = "Effects/Sence/UFO01";

		public static string ItemWaterBomb = "Effects/Sence/shuidan01";

		public static string ItemWaterFly = "Effects/Sence/waterfly";

		public static string ItemBananaEffect = "Effects/Sence/banana 1";

		public static string ItemBlock = "Effects/Sence/block";

		public static string ItemBlockAttach = "Effects/Sence/luzhang_beiji";

		public static string ItemStorm = "Effects/Sence/storm";

		public static string ItemStormAttach = "Effects/Sence/luolei_beiji";

		public static string CopsItemBlock = "Effects/Sence/CopsBlock";

		public static string CopsItemBlockAttach = "Effects/Sence/huohua";

		public static string CopsItemCar = "Effects/Sence/CopsCar";

		public static string CopsItemCarAttach = "Effects/Sence/huohua";

		public static string CopsItemNail = "Effects/Sence/CopsNail";

		public static string CopsItemNailAttach = "Effects/Sence/huohua";

		public static string CopsItemOil = "Effects/Sence/CopsOil";

		public static string CopsItemOilAttach = "Effects/Sence/banana 1";

		public static string CopsItemCopter = "Effects/Sence/CopsCopter";

		public static string CopsItemCopterAttach = "Effects/Sence/luolei_beiji";

		public static string CopsItemElectric = "Effects/Sence/CopsElectric";

		public static string CopsItemElectricAttach = "Effects/Sence/jf_dianci_shouji";

		public static int PathCount = 10;

		public static float StartDelayTime = 4f;

		public static float WrongDirDelayTime = 2f;

		public static float PreNavCameraHeight = 0.6f;

		public static float PreNavCameraForward = 5f;

		public static float PreNavCameraFOV = 40f;

		public static float PreNavTime = 4f;

		public static float EndClampTime = 3f;

		public static float EndRotateTime = 3f;

		public static float EndClampSpeed = 10f;

		public static float ResultShowTime = 5f;

		public static float TeamRankResultTime = 5f;

		public static float WinOverDelayTime = 3f;

		public static float CrashSparkSpeed = 3f;

		public static float FinishFlagDuration = 3f;

		public static float OverFlagDuration = 1f;

		public static float SelfTriggerInvalidTime = 3f;

		public static float SparkInterval = 1f;

		public static float emuHitLength = 0.1f;

		public static Float GroupGasTime = 4f;

		public const float ITEMBOXRESETTIME = 3f;

		public static Float ItemboxResetTime = 3f;

		public static float RewardCamMoveTime = 3f;

		public static Quaternion LeftLean = Quaternion.Euler(0f, 0f, -8f);

		public static Quaternion RightLean = Quaternion.Euler(0f, 0f, 8f);

		public static Vector3 RewardLocation = new Vector3(0f, 0f, 0f);

		public static Vector3 HTOffset = new Vector3(0f, 1f, 0f);

		public static Vector3 PlayerLocOffset = new Vector3(0f, 0.1f, 0f);

		public static Vector3 HSEffectOffset = new Vector3(0f, 0f, 200f);

		public static int[] TeamRankFactor = new int[6] { 8, 6, 4, 3, 2, 1 };

		public static Color[] SingleRnkColors = new Color[6]
		{
			Color.blue,
			new Color32(byte.MaxValue, 210, 23, byte.MaxValue),
			Color.red,
			Color.green,
			new Color32(byte.MaxValue, 0, 194, byte.MaxValue),
			new Color32(byte.MaxValue, 148, 0, byte.MaxValue)
		};

		public static Color[] TeamRnkColors = new Color[2]
		{
			Color.red,
			Color.blue
		};

		public static Vector3 WinnerFlameOffset = new Vector3(0f, -3.5f, 4.6f);

		public static Vector3 WinnerFlameScale = new Vector3(0.1f, 0.1f, 0.1f);

		public static float ResetOffsetY = 1f;

		public static Float TimeFactor = 0.02f;

		public static bool UseAI = true;

		public static bool SelfSkidmarkOnly = true;

		public static bool LeanCameraOnDrift = true;

		public static bool WeatherOn = true;

		public static bool HighSpeedEffectOn = true;

		public static bool GlowEffectOn = true;

		public static float FarClipScaler = 1f;

		public static bool LightMapOn = true;

		public static bool SteerTimeScale = true;

		public static float AccelerationSence = 0.05f;

		public static float AccelerationFactor = 0.3f;

		public static float HTScale = 0.007f;

		public static Color HUDNameColor = Color.white;

		public static float BlurUpFactor = 0.2f;

		public static float BlurDownFactor = 0.6f;

		public static float MaxBlurAmount = 0.95f;

		public static float MinBlurAmount = 0.65f;

		public static float CarReflectScale = 4f;

		public static float MinFloatTime = 1f;

		public static Quaternion CamLeftLean = Quaternion.Euler(0f, 0f, -12f);

		public static Quaternion CamRightLean = Quaternion.Euler(0f, 0f, 12f);

		public static float CamLeanStartFactor = 3f;

		public static float CamLeanEndFactor = 4f;

		public static Float TranslateSpeed = 33.333336f;

		public static Float SpeedUpTime = 2f;

		public static Float SpeedUpInterval = 0.1f;

		public static Float SpeedUpAdded = 15;

		public static Float SpeedUpDownTime = 0.2f;

		public static Float SpeedDownDrag = 0.35f;

		public static Float DropDownAdded = 10;

		public static float TransQTECircleDur = 0.75f;

		public static float[] TransCircleDur = new float[3] { 0f, 2.4f, 3.4f };

		public static float[] TRANSLATE_QTE_LEVELS = new float[4]
		{
			0.5f,
			2.5f,
			3.5f,
			float.MaxValue
		};

		public static float Gasclt_Factor = 1f;

		public static Float ScrapeValueScaleInSpeedUp = 0.35f;

		public static Float DriftCollectGasFactor = 0.1f;

		public static float GAS_LEVEL_ONE_MAX = 1f;

		public static float GAS_LEVEL_TWO_MAX = 1.6f;

		public static float GAS_LEVEL_THREE_MAX = 2f;

		public static float GAS_LEVEL_ONE_TIME = 0f;

		public static float GAS_LEVEL_TWO_TIME = 0.5f;

		public static float GAS_LEVEL_THREE_TIME = 1f;

		public static float GAS_LEVEL_ONE_POWER = 0f;

		public static float GAS_LEVEL_TWO_POWER = 0f;

		public static float GAS_LEVEL_THREE_POWER = 0f;
	}
}
