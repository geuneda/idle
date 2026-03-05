using UnityEngine;

namespace IdleRPG.Dungeon
{
    /// <summary>
    /// 던전 설정 데이터를 담는 ScriptableObject 래퍼.
    /// </summary>
    [CreateAssetMenu(fileName = "DungeonConfigAsset", menuName = "IdleRPG/Configs/Dungeon Config")]
    public class DungeonConfigAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>던전 설정 컬렉션</summary>
        [SerializeField] private DungeonConfigCollection _config = new DungeonConfigCollection();

        /// <summary>직렬화된 던전 설정 컬렉션을 반환한다.</summary>
        public DungeonConfigCollection Config => _config;

        /// <inheritdoc />
        public void OnBeforeSerialize() { }

        /// <inheritdoc />
        public void OnAfterDeserialize()
        {
            _config.BuildLookup();
        }
    }
}
