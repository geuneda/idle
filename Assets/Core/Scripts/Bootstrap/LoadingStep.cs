using System;
using Cysharp.Threading.Tasks;

namespace IdleRPG.Core
{
    /// <summary>
    /// 개별 로딩 단계를 나타내는 불변 데이터.
    /// 이름과 비동기 실행 함수를 포함하며, <see cref="LoadingTaskRunner"/>에서 순차 실행된다.
    /// </summary>
    public class LoadingStep
    {
        /// <summary>로딩 단계 이름 (로그 및 UI 표시용)</summary>
        public string Name { get; }

        private readonly Func<UniTask> _execute;

        /// <summary>
        /// <see cref="LoadingStep"/>을 생성한다.
        /// </summary>
        /// <param name="name">단계 이름</param>
        /// <param name="execute">실행할 비동기 함수</param>
        public LoadingStep(string name, Func<UniTask> execute)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        /// <summary>
        /// 이 단계의 비동기 작업을 실행한다.
        /// </summary>
        /// <returns>작업 완료를 나타내는 UniTask</returns>
        public UniTask Execute() => _execute();
    }
}
