using UnityEngine;

namespace IdleRPG.Hero
{
    /// <summary>
    /// 영웅 기본 스탯 설정을 담는 ScriptableObject 에셋.
    /// <see cref="HeroConfig"/> POCO를 직렬화하여 인스펙터에서 편집 가능하게 한다.
    /// </summary>
    [CreateAssetMenu(fileName = "HeroConfigAsset", menuName = "IdleRPG/Configs/Hero Config")]
    public class HeroConfigAsset : ScriptableObject
    {
        /// <summary>영웅 기본 스탯 설정 데이터</summary>
        [SerializeField] private HeroConfig _config = new HeroConfig();

        /// <summary>직렬화된 영웅 설정 데이터를 반환한다.</summary>
        public HeroConfig Config => _config;
    }
}
