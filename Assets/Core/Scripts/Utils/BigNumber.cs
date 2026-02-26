using System;
using System.Globalization;
using Newtonsoft.Json;

namespace IdleRPG.Core
{
    /// <summary>
    /// 방치형 게임용 대형 숫자 구조체. double 계수 + int 지수 기반 과학적 표기법으로 표현한다.
    /// </summary>
    /// <remarks>
    /// <para>내부적으로 <c>Coefficient * 10^Exponent</c> 형태로 정규화하여 저장한다.</para>
    /// <para>정규화 규칙: 1.0 &lt;= |Coefficient| &lt; 10.0 (0인 경우 제외)</para>
    /// <para>GC 할당을 방지하기 위해 readonly struct로 구현되었다.</para>
    /// </remarks>
    [Serializable]
    [JsonConverter(typeof(BigNumberJsonConverter))]
    public readonly struct BigNumber : IEquatable<BigNumber>, IComparable<BigNumber>
    {
        /// <summary>
        /// 값이 0인 <see cref="BigNumber"/> 상수
        /// </summary>
        public static readonly BigNumber Zero = new BigNumber(0, 0);

        /// <summary>
        /// 값이 1인 <see cref="BigNumber"/> 상수
        /// </summary>
        public static readonly BigNumber One = new BigNumber(1, 0);

        /// <summary>
        /// 정규화된 계수. 1.0 이상 10.0 미만의 절대값을 가진다 (0 제외).
        /// </summary>
        [JsonProperty("c")]
        public readonly double Coefficient;

        /// <summary>
        /// 10의 거듭제곱 지수. 실제 값 = <see cref="Coefficient"/> * 10^Exponent
        /// </summary>
        [JsonProperty("e")]
        public readonly int Exponent;

        /// <summary>
        /// 계수와 지수로 <see cref="BigNumber"/>를 생성한다. 자동으로 정규화된다.
        /// </summary>
        /// <param name="coefficient">계수 (정규화 전 원시 값)</param>
        /// <param name="exponent">10의 거듭제곱 지수</param>
        public BigNumber(double coefficient, int exponent)
        {
            if (coefficient == 0)
            {
                Coefficient = 0;
                Exponent = 0;
                return;
            }

            var abs = Math.Abs(coefficient);
            var shift = (int)Math.Floor(Math.Log10(abs));
            Coefficient = coefficient / Math.Pow(10, shift);
            Exponent = exponent + shift;
        }

        private static BigNumber Normalized(double coefficient, int exponent)
        {
            return new BigNumber(coefficient, exponent);
        }

        /// <summary>
        /// 일반 double 값으로 변환한다. 지수가 매우 큰 경우 정밀도 손실이 발생할 수 있다.
        /// </summary>
        /// <returns>double로 변환된 값</returns>
        public double ToDouble()
        {
            return Coefficient * Math.Pow(10, Exponent);
        }

        #region Arithmetic Operators

        /// <summary>
        /// 두 <see cref="BigNumber"/>를 더한다. 지수 차이가 18 이상이면 큰 쪽을 그대로 반환한다.
        /// </summary>
        public static BigNumber operator +(BigNumber a, BigNumber b)
        {
            if (a.Coefficient == 0) return b;
            if (b.Coefficient == 0) return a;

            int expDiff = a.Exponent - b.Exponent;

            if (expDiff > 18) return a;
            if (expDiff < -18) return b;

            if (expDiff >= 0)
            {
                double bCoeff = b.Coefficient / Math.Pow(10, expDiff);
                return Normalized(a.Coefficient + bCoeff, a.Exponent);
            }
            else
            {
                double aCoeff = a.Coefficient / Math.Pow(10, -expDiff);
                return Normalized(aCoeff + b.Coefficient, b.Exponent);
            }
        }

        /// <summary>
        /// 두 <see cref="BigNumber"/>를 뺀다.
        /// </summary>
        public static BigNumber operator -(BigNumber a, BigNumber b)
        {
            return a + new BigNumber(-b.Coefficient, b.Exponent);
        }

        /// <summary>
        /// 두 <see cref="BigNumber"/>를 곱한다.
        /// </summary>
        public static BigNumber operator *(BigNumber a, BigNumber b)
        {
            return Normalized(a.Coefficient * b.Coefficient, a.Exponent + b.Exponent);
        }

        /// <summary>
        /// 두 <see cref="BigNumber"/>를 나눈다.
        /// </summary>
        /// <exception cref="DivideByZeroException">제수가 0인 경우</exception>
        public static BigNumber operator /(BigNumber a, BigNumber b)
        {
            if (b.Coefficient == 0) throw new DivideByZeroException("BigNumber division by zero");
            return Normalized(a.Coefficient / b.Coefficient, a.Exponent - b.Exponent);
        }

        /// <summary>
        /// 부호를 반전시킨 <see cref="BigNumber"/>를 반환한다.
        /// </summary>
        public static BigNumber operator -(BigNumber a)
        {
            return Normalized(-a.Coefficient, a.Exponent);
        }

        #endregion

        #region Comparison Operators

        /// <inheritdoc />
        public int CompareTo(BigNumber other)
        {
            if (Coefficient == 0 && other.Coefficient == 0) return 0;
            if (Coefficient == 0) return other.Coefficient > 0 ? -1 : 1;
            if (other.Coefficient == 0) return Coefficient > 0 ? 1 : -1;

            bool aPos = Coefficient > 0;
            bool bPos = other.Coefficient > 0;

            if (aPos != bPos) return aPos ? 1 : -1;

            if (Exponent != other.Exponent)
            {
                int result = Exponent.CompareTo(other.Exponent);
                return aPos ? result : -result;
            }

            return Coefficient.CompareTo(other.Coefficient);
        }

        /// <inheritdoc />
        public bool Equals(BigNumber other)
        {
            return Exponent == other.Exponent &&
                   Math.Abs(Coefficient - other.Coefficient) < 1e-10;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is BigNumber other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Coefficient, Exponent);
        }

        /// <summary>두 값이 같은지 비교한다.</summary>
        public static bool operator ==(BigNumber a, BigNumber b) => a.Equals(b);

        /// <summary>두 값이 다른지 비교한다.</summary>
        public static bool operator !=(BigNumber a, BigNumber b) => !a.Equals(b);

        /// <summary>a가 b보다 작은지 비교한다.</summary>
        public static bool operator <(BigNumber a, BigNumber b) => a.CompareTo(b) < 0;

        /// <summary>a가 b보다 큰지 비교한다.</summary>
        public static bool operator >(BigNumber a, BigNumber b) => a.CompareTo(b) > 0;

        /// <summary>a가 b 이하인지 비교한다.</summary>
        public static bool operator <=(BigNumber a, BigNumber b) => a.CompareTo(b) <= 0;

        /// <summary>a가 b 이상인지 비교한다.</summary>
        public static bool operator >=(BigNumber a, BigNumber b) => a.CompareTo(b) >= 0;

        #endregion

        #region Implicit Conversions

        /// <summary>int에서 <see cref="BigNumber"/>로 암시적 변환</summary>
        public static implicit operator BigNumber(int value) => new BigNumber(value, 0);

        /// <summary>long에서 <see cref="BigNumber"/>로 암시적 변환</summary>
        public static implicit operator BigNumber(long value) => new BigNumber(value, 0);

        /// <summary>float에서 <see cref="BigNumber"/>로 암시적 변환</summary>
        public static implicit operator BigNumber(float value) => new BigNumber(value, 0);

        /// <summary>double에서 <see cref="BigNumber"/>로 암시적 변환</summary>
        public static implicit operator BigNumber(double value) => new BigNumber(value, 0);

        #endregion

        #region Math Utilities

        /// <summary>
        /// 두 값 중 큰 쪽을 반환한다.
        /// </summary>
        public static BigNumber Max(BigNumber a, BigNumber b) => a >= b ? a : b;

        /// <summary>
        /// 두 값 중 작은 쪽을 반환한다.
        /// </summary>
        public static BigNumber Min(BigNumber a, BigNumber b) => a <= b ? a : b;

        /// <summary>
        /// 절대값을 반환한다.
        /// </summary>
        public static BigNumber Abs(BigNumber value) => value.Coefficient < 0 ? -value : value;

        /// <summary>
        /// 소수점 이하를 버린 정수 부분을 반환한다. 지수가 15 이상이면 원본을 그대로 반환한다.
        /// </summary>
        public static BigNumber Floor(BigNumber value)
        {
            if (value.Exponent < 0) return value.Coefficient > 0 ? Zero : -One;
            if (value.Exponent >= 15) return value;
            double full = value.ToDouble();
            return new BigNumber(Math.Floor(full), 0);
        }

        /// <summary>값이 0인지 여부</summary>
        public bool IsZero => Coefficient == 0;

        /// <summary>값이 양수인지 여부</summary>
        public bool IsPositive => Coefficient > 0;

        /// <summary>값이 음수인지 여부</summary>
        public bool IsNegative => Coefficient < 0;

        #endregion

        #region Formatting

        private static readonly string[] ShortSuffixes =
        {
            "", "K", "M", "B", "T"
        };

        /// <summary>
        /// 접미사가 포함된 포맷 문자열로 변환한다. (예: "1.23K", "4.56M", "7.89T", "1.00aa")
        /// </summary>
        /// <param name="decimalPlaces">소수점 자릿수 (기본값: 2)</param>
        /// <returns>포맷된 문자열</returns>
        /// <remarks>
        /// 접미사 체계: K(1e3), M(1e6), B(1e9), T(1e12), 이후 aa~zz(1e15~)
        /// </remarks>
        public string ToFormattedString(int decimalPlaces = 2)
        {
            if (Coefficient == 0) return "0";

            int suffixIndex = Exponent / 3;
            int remainder = Exponent % 3;

            if (Exponent < 0)
            {
                return ToDouble().ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture);
            }

            double displayValue = Coefficient * Math.Pow(10, remainder);

            if (suffixIndex < ShortSuffixes.Length)
            {
                string suffix = ShortSuffixes[suffixIndex];
                if (string.IsNullOrEmpty(suffix))
                {
                    return displayValue.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture)
                        .TrimEnd('0').TrimEnd('.');
                }
                return displayValue.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture) + suffix;
            }

            int alphabeticIndex = suffixIndex - ShortSuffixes.Length;
            string alphaSuffix = GetAlphabeticSuffix(alphabeticIndex);
            return displayValue.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture) + alphaSuffix;
        }

        private static string GetAlphabeticSuffix(int index)
        {
            int first = index / 26;
            int second = index % 26;
            char c1 = (char)('a' + first);
            char c2 = (char)('a' + second);
            return $"{c1}{c2}";
        }

        /// <summary>
        /// 과학적 표기법 문자열로 변환한다. (예: "1.5e6")
        /// </summary>
        /// <returns>과학적 표기법 문자열. JSON 직렬화에 사용된다.</returns>
        public override string ToString()
        {
            if (Coefficient == 0) return "0";
            if (Exponent == 0)
                return Coefficient.ToString("G", CultureInfo.InvariantCulture);
            return $"{Coefficient.ToString("G", CultureInfo.InvariantCulture)}e{Exponent}";
        }

        #endregion
    }

    /// <summary>
    /// <see cref="BigNumber"/>의 JSON 직렬화/역직렬화를 담당하는 컨버터.
    /// 문자열("1.5e6"), 객체({"c":1.5,"e":6}), 숫자 리터럴 형식을 모두 지원한다.
    /// </summary>
    public class BigNumberJsonConverter : JsonConverter<BigNumber>
    {
        /// <inheritdoc />
        public override BigNumber ReadJson(JsonReader reader, Type objectType, BigNumber existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string str = (string)reader.Value;
                return ParseString(str);
            }

            if (reader.TokenType == JsonToken.StartObject)
            {
                double c = 0;
                int e = 0;
                while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                {
                    string prop = (string)reader.Value;
                    reader.Read();
                    if (prop == "c") c = Convert.ToDouble(reader.Value);
                    else if (prop == "e") e = Convert.ToInt32(reader.Value);
                }
                return new BigNumber(c, e);
            }

            if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float)
            {
                return new BigNumber(Convert.ToDouble(reader.Value), 0);
            }

            return BigNumber.Zero;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, BigNumber value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        private static BigNumber ParseString(string str)
        {
            if (string.IsNullOrEmpty(str)) return BigNumber.Zero;

            int eIndex = str.IndexOf('e');
            if (eIndex >= 0)
            {
                double c = double.Parse(str.Substring(0, eIndex), CultureInfo.InvariantCulture);
                int e = int.Parse(str.Substring(eIndex + 1), CultureInfo.InvariantCulture);
                return new BigNumber(c, e);
            }

            double val = double.Parse(str, CultureInfo.InvariantCulture);
            return new BigNumber(val, 0);
        }
    }
}
