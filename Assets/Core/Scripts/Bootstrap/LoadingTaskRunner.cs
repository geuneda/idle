using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Geuneda.Services;
using UnityEngine;

namespace IdleRPG.Core
{
    /// <summary>
    /// 로딩 단계를 순차 실행하고 <see cref="LoadingProgressMessage"/>를 발행하는 POCO 러너.
    /// 각 단계는 <see cref="LoadingStep"/>으로 정의되며, 추후 서버/에셋 로딩 단계를 추가할 수 있다.
    /// </summary>
    public class LoadingTaskRunner
    {
        private readonly IMessageBrokerService _messageBroker;
        private readonly IReadOnlyList<LoadingStep> _steps;

        /// <summary>
        /// <see cref="LoadingTaskRunner"/>를 생성한다.
        /// </summary>
        /// <param name="messageBroker">진행률 메시지를 발행할 브로커</param>
        /// <param name="steps">실행할 로딩 단계 목록</param>
        public LoadingTaskRunner(
            IMessageBrokerService messageBroker,
            IReadOnlyList<LoadingStep> steps)
        {
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));
            _steps = steps ?? throw new ArgumentNullException(nameof(steps));
        }

        /// <summary>
        /// 등록된 모든 로딩 단계를 순차 실행하고 진행률을 발행한다.
        /// </summary>
        /// <returns>모든 단계 완료를 나타내는 UniTask</returns>
        public async UniTask RunAsync()
        {
            int totalSteps = _steps.Count;

            if (totalSteps == 0)
            {
                PublishProgress(1f);
                return;
            }

            for (int i = 0; i < totalSteps; i++)
            {
                var step = _steps[i];

                try
                {
                    await step.Execute();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[LoadingTaskRunner] 로딩 단계 실패 ({step.Name}): {ex}");
                    throw;
                }

                float progress = (i + 1) / (float)totalSteps;
                PublishProgress(progress);
            }
        }

        private void PublishProgress(float progress)
        {
            _messageBroker.Publish(new LoadingProgressMessage { Progress = progress });
        }
    }
}
