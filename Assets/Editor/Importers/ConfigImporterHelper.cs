using System.Reflection;
using UnityEngine;

namespace IdleRPG.Editor.Importers
{
    /// <summary>
    /// Config 임포터에서 공통으로 사용하는 유틸리티.
    /// </summary>
    internal static class ConfigImporterHelper
    {
        /// <summary>
        /// Google Sheets 스프레드시트 ID.
        /// </summary>
        internal const string SPREADSHEET_ID = "1f1kK8ul7E8Y-B_Ipr0MQDZ1RqzmbL9tVnHLoFZsel2s";

        /// <summary>
        /// 지정된 gid로 Google Sheet URL을 생성한다.
        /// </summary>
        /// <param name="gid">시트 탭의 gid</param>
        /// <returns>프레임워크가 인식하는 형식의 URL</returns>
        internal static string BuildSheetUrl(string gid)
        {
            return $"https://docs.google.com/spreadsheets/d/{SPREADSHEET_ID}/edit#gid={gid}";
        }

        /// <summary>
        /// ScriptableObject의 private [SerializeField] 필드에 값을 설정한다.
        /// </summary>
        /// <param name="target">대상 ScriptableObject</param>
        /// <param name="fieldName">필드 이름</param>
        /// <param name="value">설정할 값</param>
        internal static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogError($"[ConfigImporter] '{target.GetType().Name}'에서 필드 '{fieldName}'을 찾을 수 없습니다.");
                return;
            }

            field.SetValue(target, value);
        }

        /// <summary>
        /// CSV 행에서 빈 enum 필드를 제거한다.
        /// CsvParser.Parse가 enum을 커스텀 역직렬화기보다 먼저 처리하므로,
        /// 빈 값이 Enum.Parse로 넘어가기 전에 키 자체를 제거하여 기본값을 유지한다.
        /// </summary>
        /// <param name="row">CSV 파싱된 행 데이터</param>
        /// <param name="fieldNames">빈 값 시 제거할 필드 이름들</param>
        internal static void RemoveEmptyEnumFields(
            System.Collections.Generic.Dictionary<string, string> row, params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                if (row.TryGetValue(fieldName, out var value) && string.IsNullOrWhiteSpace(value))
                {
                    row.Remove(fieldName);
                }
            }
        }
    }
}
