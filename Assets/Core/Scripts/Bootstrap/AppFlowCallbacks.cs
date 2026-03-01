using System;
using Cysharp.Threading.Tasks;

namespace IdleRPG.Core
{
    /// <summary>
    /// <see cref="AppFlowStatechart"/>의 각 상태에서 실행할 콜백을 정의한다.
    /// <see cref="GameInstaller"/>가 구체적인 로직을 주입한다.
    /// </summary>
    /// <remarks>
    /// <see cref="Func{TResult}"/> 프로퍼티에는 null 안전을 위한 기본값이 제공된다.
    /// </remarks>
    public class AppFlowCallbacks
    {
        /// <summary>Bootstrap 상태 진입 시 실행할 액션</summary>
        public Action OnBootstrapEnter { get; set; }

        /// <summary>Loading 상태 진입 시 실행할 액션 (로딩 UI 오픈 등)</summary>
        public Action OnLoadingEnter { get; set; }

        /// <summary>Loading 상태에서 대기할 비동기 작업</summary>
        public Func<UniTask> LoadingTask { get; set; } = () => UniTask.CompletedTask;

        /// <summary>Loading 상태 퇴장 시 실행할 액션 (로딩 UI 닫기 등)</summary>
        public Action OnLoadingExit { get; set; }

        /// <summary>오프라인 보상 표시 여부 판정 (추후 Phase 7에서 구현)</summary>
        public Func<bool> HasOfflineReward { get; set; } = () => false;

        /// <summary>첫 실행(튜토리얼 필요) 여부 판정 (추후 구현)</summary>
        public Func<bool> IsFirstPlay { get; set; } = () => false;

        /// <summary>오프라인 보상 상태에서 대기할 비동기 작업 (팝업 표시 및 수령 대기)</summary>
        public Func<UniTask> OfflineRewardTask { get; set; } = () => UniTask.CompletedTask;

        /// <summary>오프라인 보상 상태 퇴장 시 실행할 액션</summary>
        public Action OnOfflineRewardExit { get; set; }

        /// <summary>InGame 상태 진입 시 실행할 액션 (초기 UI 오픈 등)</summary>
        public Action OnInGameEnter { get; set; }

        /// <summary>InGame 상태 퇴장 시 실행할 액션</summary>
        public Action OnInGameExit { get; set; }
    }
}
