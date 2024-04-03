using System;
using System.Collections.Generic;

namespace TrueSync
{
	public delegate void TrueSyncUpdateCallback(List<InputDataBase> allInputData);
	public delegate void TrueSyncInputCallback(InputDataBase playerInputData);
	public delegate void TrueSyncEventCallback();
	public delegate void TrueSyncPlayerDisconnectionCallback(byte playerId);
	public delegate void ReplayRecordSave(byte[] replayRecord, int numberOfPlayers);
	public delegate bool TrueSyncIsReady();
	public delegate InputDataBase TrueSyncInputDataProvider();
	public delegate void OnEventReceived(byte eventCode, object content);
}
