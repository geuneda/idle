using UnityEngine;

namespace IdleRPG.Growth
{
    /// <summary>
    /// 영웅 성장 시스템 설정을 담는 ScriptableObject 에셋.
    /// 인스펙터에서 스탯별 <see cref="StatGrowthEntry"/>를 편집한다.
    /// </summary>
    [CreateAssetMenu(fileName = "GrowthConfigAsset", menuName = "IdleRPG/Configs/Growth Config")]
    public class GrowthConfigAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>성장 설정 데이터</summary>
        [SerializeField] private GrowthConfig _config = new GrowthConfig();

        /// <summary>직렬화된 성장 설정 데이터를 반환한다.</summary>
        public GrowthConfig Config => _config;

        /// <inheritdoc />
        public void OnBeforeSerialize() { }

        /// <inheritdoc />
        public void OnAfterDeserialize()
        {
            _config.BuildLookup();
        }
    }
}
