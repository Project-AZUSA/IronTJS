using System;

namespace IronTjs.Builtins
{
    public static class Math
	{
		static MersenneTwister _generator = new MersenneTwister();

		public static object abs(object value)
		{
			if (value == null)
				return value;
			if (Runtime.Binding.Binders.IsFloatingPoint(value.GetType()))
				return System.Math.Abs(Convert.ToDouble(value));
			else
				return System.Math.Abs(Convert.ToInt64(value));
		}

		public static double acos(double d) { return System.Math.Acos(d); }

		public static double asin(double d) { return System.Math.Asin(d); }

		public static double atan(double d) { return System.Math.Atan(d); }

		public static double atan2(double y, double x) { return System.Math.Atan2(y, x); }

		public static double ceil(double d) { return System.Math.Ceiling(d); }

		public static double floor(double d) { return System.Math.Floor(d); }

		public static double exp(double d) { return System.Math.Exp(d); }

		public static double log(double d) { return System.Math.Log(d); }

		public static object max(object x, object y)
		{
			if (x == null || y == null)
				return null;
			if (Runtime.Binding.Binders.IsFloatingPoint(x.GetType()) ||
				Runtime.Binding.Binders.IsFloatingPoint(y.GetType()))
				return System.Math.Max(Convert.ToDouble(x), Convert.ToDouble(y));
			else
				return System.Math.Max(Convert.ToInt64(x), Convert.ToInt64(y));
		}

		public static object min(object x, object y)
		{
			if (x == null || y == null)
				return null;
			if (Runtime.Binding.Binders.IsFloatingPoint(x.GetType()) ||
				Runtime.Binding.Binders.IsFloatingPoint(y.GetType()))
				return System.Math.Min(Convert.ToDouble(x), Convert.ToDouble(y));
			else
				return System.Math.Min(Convert.ToInt64(x), Convert.ToInt64(y));
		}

		public static double random() { return _generator.NextDouble(); }

		public static double pow(double x, double y) { return System.Math.Pow(x, y); }

		public static double round(double d) { return System.Math.Round(d); }

		public static double sin(double d) { return System.Math.Sin(d); }

		public static double cos(double d) { return System.Math.Cos(d); }

		public static double tan(double d) { return System.Math.Tan(d); }

		public static double sqrt(double d) { return System.Math.Sqrt(d); }

		public static double E { get { return System.Math.E; } }

		public static double LN10 { get { return System.Math.Log(10); } }

		public static double LN2 { get { return System.Math.Log(2); } }

		public static double LOG10E { get { return System.Math.Log10(E); } }

		public static double LOG2E { get { return System.Math.Log(E, 2); } }

		public static double PI { get { return System.Math.PI; } }

		public static double SQRT1_2 { get { return System.Math.Sqrt(0.5); } }

		public static double SQRT2 { get { return System.Math.Sqrt(2); } }

		public class RandomGenerator
		{
			public RandomGenerator() { twister = new MersenneTwister(); }

			public RandomGenerator(long seed) { twister = new MersenneTwister(LongToUInt32Array(seed)); }

			public RandomGenerator(Dictionary storage) { twister = MersenneTwister.FromDictionary(storage); }

			static uint[] LongToUInt32Array(long value) { return new uint[] { (uint)((ulong)value >> 32), (uint)(value & 0xffffffff) }; }

			MersenneTwister twister;

			public void randomize() { twister.Initialize(LongToUInt32Array(DateTime.Now.Ticks)); }

			public void randomize(long seed) { twister.Initialize(LongToUInt32Array(seed)); }

			public void randomize(Dictionary storage) { twister = MersenneTwister.FromDictionary(storage); }

			public double random() { return twister.NextDouble(); }

			public long random32() { return twister.NextUInt32(); }

			public long random63() { return (long)twister.Next() << 32 | twister.NextUInt32(); }

			public long random64() { return (long)twister.NextUInt32() << 32 | twister.NextUInt32(); }

			public Dictionary serialize() { return new Dictionary(twister.ToDictionary()); }
		}
	}
}
