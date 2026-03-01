using UnityEngine;

namespace IdleRPG.OfflineReward
{
    /// <summary>
    /// 오프라인 보상 설정을 담는 ScriptableObject 에셋.
    /// </summary>
    [CreateAssetMenu(fileName = "OfflineRewardConfigAsset", menuName = "IdleRPG/Configs/Offline Reward Config")]
    public class OfflineRewardConfigAsset : ScriptableObject
    {
        /// <summary>오프라인 보상 설정 데이터</summary>
        [SerializeField] private OfflineRewardConfig _config = new OfflineRewardConfig();

        /// <summary>직렬화된 오프라인 보상 설정 데이터를 반환한다.</summary>
        public OfflineRewardConfig Config => _config;
    }
}
