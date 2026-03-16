using UnityEngine;

namespace IdleRPG.Mine
{
    /// <summary>
    /// 광산 설정 데이터를 담는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "MineConfigAsset", menuName = "IdleRPG/Configs/Mine Config")]
    public class MineConfigAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private MineConfigCollection _config = new MineConfigCollection();

        /// <summary>광산 설정 컬렉션</summary>
        public MineConfigCollection Config => _config;

        /// <inheritdoc />
        public void OnBeforeSerialize() { }

        /// <inheritdoc />
        public void OnAfterDeserialize()
        {
            _config.BuildLookup();
        }
    }
}
