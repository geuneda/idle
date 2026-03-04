using System;
using Geuneda.Services;

namespace IdleRPG.Core
{
    /// <summary>
    /// 피처별 저장 데이터 수집/적용 계약.
    /// 각 피처가 자체 Extract/Apply/이벤트 구독 로직을 캡슐화하여
    /// <see cref="SaveService"/>의 피처 의존성을 제거한다.
    /// </summary>
    public interface ISaveDataCollector
    {
        /// <summary>현재 모델 상태를 저장 데이터에 기록한다.</summary>
        /// <param name="saveData">기록 대상 저장 데이터</param>
        void ExtractTo(GameSaveData saveData);

        /// <summary>저장 데이터를 모델에 적용한다.</summary>
        /// <param name="saveData">적용할 저장 데이터</param>
        void ApplyFrom(GameSaveData saveData);

        /// <summary>피처의 dirty 이벤트를 구독한다.</summary>
        /// <param name="broker">메시지 브로커</param>
        /// <param name="markDirty">dirty 마킹 콜백</param>
        void SubscribeDirtyEvents(IMessageBrokerService broker, Action markDirty);

        /// <summary>피처의 dirty 이벤트 구독을 해제한다.</summary>
        /// <param name="broker">메시지 브로커</param>
        void UnsubscribeDirtyEvents(IMessageBrokerService broker);
    }
}
