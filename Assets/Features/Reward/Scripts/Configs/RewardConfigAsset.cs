using UnityEngine;

namespace IdleRPG.Reward
{
    /// <summary>
    /// 보상 설정을 담는 ScriptableObject 에셋.
    /// </summary>
    [CreateAssetMenu(fileName = "RewardConfigAsset", menuName = "IdleRPG/Configs/Reward Config")]
    public class RewardConfigAsset : ScriptableObject
    {
        /// <summary>보상 설정 데이터</summary>
        [SerializeField] private RewardConfig _config = new RewardConfig();

        /// <summary>직렬화된 보상 설정 데이터를 반환한다.</summary>
        public RewardConfig Config => _config;
    }
}
