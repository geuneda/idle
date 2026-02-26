using System;
using System.Globalization;
using Newtonsoft.Json;

namespace IdleRPG.Core
{
    [Serializable]
    [JsonConverter(typeof(BigNumberJsonConverter))]
    public readonly struct BigNumber : IEquatable<BigNumber>, IComparable<BigNumber>
    {
        public static readonly BigNumber Zero = new BigNumber(0, 0);
        public static readonly BigNumber One = new BigNumber(1, 0);

        [JsonProperty("c")]
        public readonly double Coefficient;

        [JsonProperty("e")]
        public readonly int Exponent;

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

        public double ToDouble()
        {
            return Coefficient * Math.Pow(10, Exponent);
        }

        #region Arithmetic Operators

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

        public static BigNumber operator -(BigNumber a, BigNumber b)
        {
            return a + new BigNumber(-b.Coefficient, b.Exponent);
        }

        public static BigNumber operator *(BigNumber a, BigNumber b)
        {
            return Normalized(a.Coefficient * b.Coefficient, a.Exponent + b.Exponent);
        }

        public static BigNumber operator /(BigNumber a, BigNumber b)
        {
            if (b.Coefficient == 0) throw new DivideByZeroException("BigNumber division by zero");
            return Normalized(a.Coefficient / b.Coefficient, a.Exponent - b.Exponent);
        }

        public static BigNumber operator -(BigNumber a)
        {
            return Normalized(-a.Coefficient, a.Exponent);
        }

        #endregion

        #region Comparison Operators

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

        public bool Equals(BigNumber other)
        {
            return Exponent == other.Exponent &&
                   Math.Abs(Coefficient - other.Coefficient) < 1e-10;
        }

        public override bool Equals(object obj)
        {
            return obj is BigNumber other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Coefficient, Exponent);
        }

        public static bool operator ==(BigNumber a, BigNumber b) => a.Equals(b);
        public static bool operator !=(BigNumber a, BigNumber b) => !a.Equals(b);
        public static bool operator <(BigNumber a, BigNumber b) => a.CompareTo(b) < 0;
        public static bool operator >(BigNumber a, BigNumber b) => a.CompareTo(b) > 0;
        public static bool operator <=(BigNumber a, BigNumber b) => a.CompareTo(b) <= 0;
        public static bool operator >=(BigNumber a, BigNumber b) => a.CompareTo(b) >= 0;

        #endregion

        #region Implicit Conversions

        public static implicit operator BigNumber(int value) => new BigNumber(value, 0);
        public static implicit operator BigNumber(long value) => new BigNumber(value, 0);
        public static implicit operator BigNumber(float value) => new BigNumber(value, 0);
        public static implicit operator BigNumber(double value) => new BigNumber(value, 0);

        #endregion

        #region Math Utilities

        public static BigNumber Max(BigNumber a, BigNumber b) => a >= b ? a : b;
        public static BigNumber Min(BigNumber a, BigNumber b) => a <= b ? a : b;
        public static BigNumber Abs(BigNumber value) => value.Coefficient < 0 ? -value : value;

        public static BigNumber Floor(BigNumber value)
        {
            if (value.Exponent < 0) return value.Coefficient > 0 ? Zero : -One;
            if (value.Exponent >= 15) return value;
            double full = value.ToDouble();
            return new BigNumber(Math.Floor(full), 0);
        }

        public bool IsZero => Coefficient == 0;
        public bool IsPositive => Coefficient > 0;
        public bool IsNegative => Coefficient < 0;

        #endregion

        #region Formatting

        private static readonly string[] ShortSuffixes =
        {
            "", "K", "M", "B", "T"
        };

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

        public override string ToString()
        {
            if (Coefficient == 0) return "0";
            if (Exponent == 0)
                return Coefficient.ToString("G", CultureInfo.InvariantCulture);
            return $"{Coefficient.ToString("G", CultureInfo.InvariantCulture)}e{Exponent}";
        }

        #endregion
    }

    public class BigNumberJsonConverter : JsonConverter<BigNumber>
    {
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
