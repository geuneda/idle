using UnityEngine;

namespace IdleRPG.Skill
{
    /// <summary>
    /// 스킬 설정 데이터의 ScriptableObject 래퍼. Addressables를 통해 런타임에 로딩된다.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillConfigAsset", menuName = "IdleRPG/Configs/Skill Config")]
    public class SkillConfigAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private SkillConfigCollection _config = new();

        /// <summary>스킬 설정 컬렉션</summary>
        public SkillConfigCollection Config => _config;

        /// <inheritdoc />
        public void OnBeforeSerialize() { }

        /// <inheritdoc />
        public void OnAfterDeserialize()
        {
            _config.BuildLookup();
        }
    }
}
