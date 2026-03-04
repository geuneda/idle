using UnityEngine;

namespace IdleRPG.Pet
{
    /// <summary>
    /// 펫 설정 데이터의 ScriptableObject 래퍼. Addressables를 통해 런타임에 로딩된다.
    /// </summary>
    [CreateAssetMenu(fileName = "PetConfigAsset", menuName = "IdleRPG/Configs/Pet Config")]
    public class PetConfigAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private PetConfigCollection _config = new();

        /// <summary>펫 설정 컬렉션</summary>
        public PetConfigCollection Config => _config;

        /// <inheritdoc />
        public void OnBeforeSerialize() { }

        /// <inheritdoc />
        public void OnAfterDeserialize()
        {
            _config.BuildLookup();
        }
    }
}
