using Geuneda.DataExtensions;
using IdleRPG.Core;

namespace IdleRPG.Economy
{
    /// <summary>
    /// 재화 관리 서비스 인터페이스.
    /// 재화 추가/소비/조회 기능을 제공하며, 모든 변경은 커맨드 패턴으로 처리된다.
    /// </summary>
    /// <remarks>
    /// <see cref="Geuneda.Services.MainInstaller"/>에 바인딩하여 사용한다.
    /// </remarks>
    public interface ICurrencyService
    {
        /// <summary>
        /// 재화 보유량의 읽기 전용 뷰. UI에서 옵저빙하여 표시를 갱신할 수 있다.
        /// </summary>
        IObservableDictionaryReader<CurrencyType, BigNumber> Currencies { get; }

        /// <summary>
        /// 지정한 재화를 추가한다.
        /// </summary>
        /// <param name="type">재화 종류</param>
        /// <param name="amount">추가할 양 (양수)</param>
        void Add(CurrencyType type, BigNumber amount);

        /// <summary>
        /// 지정한 재화를 소비한다. 잔액이 부족하면 소비하지 않고 false를 반환한다.
        /// </summary>
        /// <param name="type">재화 종류</param>
        /// <param name="amount">소비할 양 (양수)</param>
        /// <returns>소비 성공 여부</returns>
        bool TrySpend(CurrencyType type, BigNumber amount);

        /// <summary>
        /// 지정한 재화가 충분한지 확인한다.
        /// </summary>
        /// <param name="type">재화 종류</param>
        /// <param name="amount">필요한 양</param>
        /// <returns>보유량이 필요량 이상이면 true</returns>
        bool HasEnough(CurrencyType type, BigNumber amount);

        /// <summary>
        /// 지정한 재화의 현재 보유량을 반환한다.
        /// </summary>
        /// <param name="type">재화 종류</param>
        /// <returns>현재 보유량</returns>
        BigNumber GetAmount(CurrencyType type);
    }
}
