using UnityEngine;

namespace IdleRPG.Economy
{
    /// <summary>
    /// 재화 표시 설정 데이터를 담는 ScriptableObject 래퍼.
    /// </summary>
    [CreateAssetMenu(fileName = "CurrencyDisplayConfigAsset", menuName = "IdleRPG/Configs/Currency Display Config")]
    public class CurrencyDisplayConfigAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>재화 표시 설정 컬렉션</summary>
        [SerializeField] private CurrencyDisplayConfigCollection _config = new CurrencyDisplayConfigCollection();

        /// <summary>직렬화된 재화 표시 설정 컬렉션을 반환한다.</summary>
        public CurrencyDisplayConfigCollection Config => _config;

        /// <inheritdoc />
        public void OnBeforeSerialize() { }

        /// <inheritdoc />
        public void OnAfterDeserialize()
        {
            _config.BuildLookup();
        }
    }
}
