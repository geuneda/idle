using UnityEngine;

namespace IdleRPG.Stage
{
    /// <summary>
    /// 스테이지 구조 및 밸런스 설정을 담는 ScriptableObject 에셋.
    /// </summary>
    [CreateAssetMenu(fileName = "StageConfigAsset", menuName = "IdleRPG/Configs/Stage Config")]
    public class StageConfigAsset : ScriptableObject
    {
        /// <summary>스테이지 구조 설정 데이터</summary>
        [SerializeField] private StageConfig _config = new StageConfig();

        /// <summary>직렬화된 스테이지 설정 데이터를 반환한다.</summary>
        public StageConfig Config => _config;
    }
}
