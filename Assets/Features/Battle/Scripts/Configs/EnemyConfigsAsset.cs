using System.Collections.Generic;
using UnityEngine;

namespace IdleRPG.Battle
{
    /// <summary>
    /// 적 설정 컬렉션을 담는 ScriptableObject 에셋.
    /// 일반 적, 보스 등 여러 종류의 <see cref="EnemyConfig"/>를 관리한다.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyConfigsAsset", menuName = "IdleRPG/Configs/Enemy Configs")]
    public class EnemyConfigsAsset : ScriptableObject
    {
        /// <summary>적 설정 목록. 각 항목의 Id로 식별된다.</summary>
        [SerializeField] private List<EnemyConfig> _configs = new List<EnemyConfig>();

        /// <summary>직렬화된 적 설정 목록을 반환한다.</summary>
        public IList<EnemyConfig> Configs => _configs;
    }
}
