using System;
using System.Globalization;
using System.Linq;
using System.Text;
using IronTJS.Runtime.Binding;
using Microsoft.Scripting.Actions;

namespace IronTJS.Builtins
{
    public static class String
	{
		enum SprintfFlag
		{
			None,
			WriteSign,
			WritePrefix,
			PadZeros,
		}

		public static readonly ExtensionPropertyTracker lengthProperty = new ExtensionPropertyTracker("length", typeof(string).GetMethod("get_Length"), null, null, typeof(string));

		public static string charAt(this string value, int index) { return index >= 0 && index < value.Length ? value[index].ToString() : string.Empty; }

		public static long indexOf(this string value, string text, int startIndex = 0) { return value.IndexOf(text, startIndex); }

		public static string toLowerCase(this string value) { return value.ToLower(); }

		public static string toUpperCase(this string value) { return value.ToUpper(); }

		public static string substring(this string value, int start, int? count = null) { return count == null ? value.Substring(start) : value.Substring(start, (int)count); }

		public static string substr(this string value, int start, int? count = null) { return substring(value, start, count); }

		public static string sprintf(this string value, params object[] args)
		{
			StringBuilder sb = new StringBuilder();
			int chIndex = 0;
			int argIndex = 0;
			while (chIndex < value.Length)
			{
				if (value[chIndex] == '%')
				{
					chIndex++;
					SprintfFlag flag = SprintfFlag.None;
					if (chIndex < value.Length && value[chIndex] == '+')
					{
						chIndex++;
						flag = SprintfFlag.WriteSign;
					}
					else if (chIndex < value.Length && value[chIndex] == '#')
					{
						chIndex++;
						flag = SprintfFlag.WritePrefix;
					}
					else if (chIndex < value.Length && value[chIndex] == '0')
					{
						chIndex++;
						flag = SprintfFlag.PadZeros;
					}
					int fieldWidth = 0;
					while (chIndex < value.Length && char.IsDigit(value[chIndex]))
						fieldWidth = 10 * fieldWidth + value[chIndex++] - '0';
					int precision = 0;
					if (chIndex < value.Length && value[chIndex] == '.')
					{
						chIndex++;
						while (chIndex < value.Length && char.IsDigit(value[chIndex]))
							precision = 10 * precision + value[chIndex++] - '0';
					}
					string specifier = "";
					if (chIndex < value.Length)
					{
						switch (value[chIndex])
						{
							case 'd':
							case 'i':
								chIndex++;
								if (argIndex < args.Length)
									sb.Append(FormatNumber((long)TjsBinder.ConvertInternal(args[argIndex++], typeof(long)), fieldWidth, flag, "d"));
								break;
							case 'u':
								chIndex++;
								if (argIndex < args.Length)
								{
									var uinteger = unchecked((ulong)(long)TjsBinder.ConvertInternal(args[argIndex++], typeof(long)));
									if (flag == SprintfFlag.PadZeros)
										sb.Append(uinteger.ToString().PadLeft(fieldWidth, '0'));
									else
										sb.Append(uinteger.ToString().PadLeft(fieldWidth, ' '));
								}
								break;
							case 'o':
								chIndex++;
								if (argIndex < args.Length)
								{
									var uinteger = unchecked((ulong)(long)TjsBinder.ConvertInternal(args[argIndex++], typeof(long)));
									if (flag == SprintfFlag.PadZeros)
										sb.Append(ToOctal(uinteger).PadLeft(fieldWidth, '0'));
									else if (flag == SprintfFlag.WritePrefix)
										sb.Append(("0" + ToOctal(uinteger)).PadLeft(fieldWidth, ' '));
									else
										sb.Append(ToOctal(uinteger).PadLeft(fieldWidth, ' '));
								}
								break;
							case 'x':
							case 'X':
								if (argIndex < args.Length)
								{
									var uinteger = unchecked((ulong)(long)TjsBinder.ConvertInternal(args[argIndex++], typeof(long)));
									if (flag == SprintfFlag.PadZeros)
										sb.Append(uinteger.ToString(value[chIndex].ToString()).PadLeft(fieldWidth, '0'));
									else if (flag == SprintfFlag.WritePrefix)
										sb.Append(("0x" + uinteger.ToString(value[chIndex].ToString())).PadLeft(fieldWidth, ' '));
									else
										sb.Append(uinteger.ToString(value[chIndex].ToString()).PadLeft(fieldWidth, ' '));
								}
								chIndex++;
								break;
							case 'e':
							case 'E':
							case 'f':
							case 'g':
							case 'G':
								specifier = value[chIndex++].ToString() + precision.ToString();
								if (argIndex < args.Length)
									sb.Append(FormatNumber((double)TjsBinder.ConvertInternal(args[argIndex++], typeof(double)), fieldWidth, flag, specifier));
								break;
							case 'c':
								chIndex++;
								if (argIndex < args.Length)
									sb.Append(args[argIndex++].ToString().charAt(0).PadLeft(fieldWidth, ' '));
								break;
							case 's':
								chIndex++;
								if (argIndex < args.Length)
									sb.Append(args[argIndex++].ToString().PadLeft(fieldWidth, ' '));
								break;
							case '%':
								sb.Append(value[chIndex++]);
								break;
						}
					}
				}
				else
					sb.Append(value[chIndex++]);
			}
			return sb.ToString();
		}

		static string ToOctal(ulong value)
		{
			StringBuilder sb = new StringBuilder();
			do
			{
				sb.Insert(0, (char)('0' + value % 8));
				value /= 8;
			} while (value > 0);
			return sb.ToString();
		}

		static string FormatNumber<T>(T number, int fieldWidth, SprintfFlag flag, string specifier) where T : IComparable<T>, IFormattable
		{
			var zero = default(T);
			var culture = CultureInfo.InvariantCulture;
			if (flag == SprintfFlag.WriteSign)
			{
				if (number.CompareTo(zero) >= 0)
					return (culture.NumberFormat.PositiveSign + number.ToString(specifier, culture)).PadLeft(fieldWidth, ' ');
				else
					return number.ToString(specifier, culture).PadLeft(fieldWidth, ' ');
			}
			else if (flag == SprintfFlag.PadZeros)
			{
				if (number.CompareTo(zero) >= 0)
					return number.ToString(specifier, culture).PadLeft(fieldWidth, '0');
				else
					return culture.NumberFormat.NegativeSign + number.ToString(specifier, culture).Replace(culture.NumberFormat.NegativeSign, "").PadLeft(fieldWidth - 1, '0');
			}
			else
				return number.ToString(specifier, culture).PadLeft(fieldWidth, ' ');
		}

		public static string replace(this string value, string pattern, string replacement) { return System.Text.RegularExpressions.Regex.Replace(value, pattern, replacement); }

		public static Array split(this string value, object patternOrDelimiters, object reserved = null, bool removeEmptyEntries = false)
		{
			var res = new Array();
			res.split(patternOrDelimiters, value, reserved, removeEmptyEntries);
			return res;
		}

		public static string escape(this string value)
		{
			return string.Concat(value.Select(x =>
				{
					switch (x)
					{
						case '\'': return @"\'";
						case '"': return "\\\"";
						case '\\': return @"\\";
						default: return x.ToString();
					}
				}));
		}

		public static string trim(this string value) { return value.Trim(); }

		public static string reverse(this string value) { return string.Concat(value.Reverse()); }

		public static string repeat(this string value, int count) { return string.Concat(Enumerable.Repeat(value, count)); }
	}
}
