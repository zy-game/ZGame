using UnityEngine;

namespace ZGame.Game
{
    public interface IInputSystem : ISystem
    {
        InputData GetInputData();
    }

    public class InputCollectionSystem : IInputSystem
    {
        public uint priority { get; }

        public InputData GetInputData()
        {
            InputData inputData = GameFrameworkFactory.Spawner<InputData>();
            inputData.Set(0, Input.GetAxisRaw("Horizontal"));
            inputData.Set(1, Input.GetAxisRaw("Vertical"));
            return inputData;
        }

        public void Release()
        {
        }
    }
}