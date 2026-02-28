using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace IdleRPG.Core
{
    /// <summary>
    /// DEV 스크립팅 심볼이 정의된 빌드에서만 로그를 출력하는 유틸리티.
    /// 프로덕션 빌드에서는 호출 자체가 컴파일러에 의해 제거된다.
    /// </summary>
    /// <remarks>
    /// <para><c>[Conditional("DEV")]</c> 어트리뷰트로 호출부까지 완전 제거되므로
    /// 문자열 할당/보간 오버헤드가 프로덕션에서 발생하지 않는다.</para>
    /// <para>Unity Editor에서는 <c>DEV</c> 심볼이 기본 정의되어야 한다.
    /// Project Settings > Player > Scripting Define Symbols에 <c>DEV</c>를 추가할 것.</para>
    /// </remarks>
    public static class DevLog
    {
        /// <summary>
        /// 정보 로그를 출력한다. DEV 빌드에서만 동작한다.
        /// </summary>
        /// <param name="message">출력할 메시지</param>
        [Conditional("DEV")]
        public static void Log(string message)
        {
            Debug.Log(message);
        }

        /// <summary>
        /// 컨텍스트 오브젝트와 함께 정보 로그를 출력한다. DEV 빌드에서만 동작한다.
        /// </summary>
        /// <param name="message">출력할 메시지</param>
        /// <param name="context">로그와 연결할 Unity 오브젝트</param>
        [Conditional("DEV")]
        public static void Log(string message, Object context)
        {
            Debug.Log(message, context);
        }

        /// <summary>
        /// 경고 로그를 출력한다. DEV 빌드에서만 동작한다.
        /// </summary>
        /// <param name="message">출력할 메시지</param>
        [Conditional("DEV")]
        public static void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        /// <summary>
        /// 경고 로그를 컨텍스트 오브젝트와 함께 출력한다. DEV 빌드에서만 동작한다.
        /// </summary>
        /// <param name="message">출력할 메시지</param>
        /// <param name="context">로그와 연결할 Unity 오브젝트</param>
        [Conditional("DEV")]
        public static void LogWarning(string message, Object context)
        {
            Debug.LogWarning(message, context);
        }
    }
}
