using UnityEngine;

namespace IdleRPG.Equipment
{
    /// <summary>
    /// <see cref="EquipmentConfigCollection"/>의 ScriptableObject 래퍼.
    /// Addressables를 통해 비동기 로딩된다.
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentConfigAsset", menuName = "IdleRPG/Configs/Equipment Config")]
    public class EquipmentConfigAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private EquipmentConfigCollection _config = new();

        /// <summary>장비 설정 데이터 컬렉션</summary>
        public EquipmentConfigCollection Config => _config;

        /// <inheritdoc />
        public void OnBeforeSerialize()
        {
        }

        /// <inheritdoc />
        public void OnAfterDeserialize()
        {
            _config.BuildLookup();
        }
    }
}
