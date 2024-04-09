namespace CarLogic
{
	public static class RaceDefines
	{
		public const int LAYERMASK_ROAD = 8;

		public const int LAYERMASK_WALL = 9;

		public const int LAYERMASK_ITEM = 12;

		public const int LAYERMASK_ITEMBOX = 15;

		public const int LAYERMASK_CAR_BODY = 14;

		public const int LAYERMASK_CAR_COLLIDER = 30;

		public const int LAYERMASK_SPEEDUP = 18;

		public const int LAYERMASK_TRANSLATE = 19;

		public const int LAYERMASK_OBSTACLE = 20;

		public const int LAYERMASK_ANIMATIONTRIGGER = 23;

		public const int LAYERMASK_FALLDOWN = 24;

		public const int LAYERMASK_SUSTAINED_SPEEDUP = 28;

		public const int LAYER_ROAD = 256;

		public const int LAYER_WALL = 512;

		public const string TAG_ANIMATION = "AnimationObj";

		public const string CAR_SHADER = "CarRace/CarPaintSimple";

		public const string CAR_LIGHTING_SHADER = "CarRace/CarPaintShow5";

		public const long PRE_COUNTDOWN_TIME = 28850000L;

		public const long POST_COUNTDOWN_TIME = 100000000L;

		public static float MAX_GAS_VALUE = 800000f;

		public const int MAX_ITEM_COUNT = 2;

		public const uint NOT_FINISH_TIME_FLAG = 0u;

		public const string RACE_AI_PATH = "a_{0:D}_{1:D}_{2:D}";

		public const string CAR_NAME_PREFIX = "car{0}";

		public const string CARD_NAME_PREFIX = "card{0:D3}";

		public const string TIRE_NAME_PREFIX = "tire{0:D3}";

		public const string EFFECT_NAME_PREFIX = "effect{0:D3}";

		public const string SPERKER_NAME_PREFIX = "speaker{0:D3}";

		public const string ADDITION_NAME_PREFIX = "addition{0:D3}";

		public const string PATH_NAME_PREFIX = "path{0}";

		public const int RACE_AI_VERSION = 1;

		public const float UI_GAS_LEVEL_ONE = 0.51f;

		public const float UI_GAS_LEVEL_TWO = 0.28f;

		public const float UI_GAS_LEVEL_Three = 0.21f;

		public const string RACE_MAIN_CAMERA = "RaceMainCamera";

		public const string RACE_PRE_COUNTDOWN = "PreCountDown";

		public const string RACE_POST_COUNTDOWN = "PostCountDown";

		public const string RACE_ITEM_BOX = "ItemBox";

		public const string RACE_PASSIVE_NAME = "PassiveObject";

		public const string RACE_ITEM_CONTAINER = "ItemBoxes";

		public const string RACE_MODEL = "Models";

		public const string RACE_EFFECT_ROOT = "Effects";

		public const string RACE_EFFECT_GAS = "Effects/tuowei01";

		public const string RACE_EFFECT_HIGHSPEED = "Effects/hspeed";

		public const string RACE_AIRSHIPEFFECT = "Effects/feichuan";

		public const string RACE_AIRSHIPEFFECT_DRIFTOFFSET = "Effects/feichuan/DriftOffset";

		public const string RACE_AIRSHIPEFFECT_NORMAL = "Effects/feichuan/Normal";

		public const string RACE_REWARD_ANCHOR = "#Anchor_";

		public const string RACE_REWARD_LOOKTARGET = "#LookTarget";

		public const string RACE_HUMAN_NAME = "Models/Human";

		public const string RACE_HUMAN_UPBODY = "/char/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1";

		public const string RACE_BODYCOLLIDERS = "BodyColliders";

		public const string RACE_HEADTOP_NAME = "#HTNameLabel{0}";

		public const string RACE_UI_CENTER_PANEL = "CenterPanel";

		public const string RACE_UI_STOP_PANEL = "StopPanel";

		public const string RACE_UI_HEADTOP_PANEL = "HT";

		public const string RACE_CARSHADOW = "CarShadow";

		public const string RACE_UI_PATH = "UIPrefabs/RaceScene/";

		public const string RACE_PATH_PATH = "Custom/Race/Prefabs/path/";

		public const string RACE_VICTORY_PREFAB = "Effects/UI/wansheng";

		public const string RACE_HUD_ROOT = "UIPrefabs/RaceScene/HUD";

		public const string RACE_HUD_NAME = "UIPrefabs/RaceScene/HTName";

		public const string RACE_HUD_SKILL = "UIPrefabs/RaceScene/HTSkillLabel";

		public const string RACE_CAR_PATH = "Custom/Race/Prefabs/car/";

		public const string RACE_CAR_PART_PATH = "Custom/Race/Prefabs/carpart/";

		public const string RACE_AUDIO_PATH = "Audio/RaceSounds/";

		public const string RACE_BGM_PATH = "Audio/MapBGM/";

		public const string RACE_ITEM_BUILD_RATE_CSV = "Public/Xml/ItemRate.txt";

		public const string RACE_EFFECT_PATH = "Effects/Sence/";

		public const string RACE_MINIMAP_MARK = "UIPrefabs/RaceScene/MiniMark";

		public const string RACE_MAINCAMERA_PATH = "Custom/Race/Prefabs/RaceMainCamera";

		public const string RACE_WINNER_FLAME = "Effects/Sence/zhongdianyanhua0001";

		public const string AudioAngle = "Angle";

		public const string AudioAngleDefence = "AngleDefence";

		public const string AudioRocketCrash = "RocketCrash";

		public const string AudioRocketFire = "RocketFire";

		public const string AudioRocketFly = "RocketFly";

		public const string AudioSmallGas = "sound_penqi";

		public const string AudioEngine = "sound_yinqing";

		public const string AudioEvil = "Evil";

		public const string AudioDrift = "sound_piaoyi";

		public const string AudioAim = "Aim";

		public const string AudioAimLose = "AimLose";

		public const string AudioCollide = "Collide";

		public const string AudioBananaToggle = "BananaToggle";
	}
}
