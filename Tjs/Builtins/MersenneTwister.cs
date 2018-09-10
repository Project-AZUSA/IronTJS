/* 
   Copyright (C) 1997 - 2002, Makoto Matsumoto and Takuji Nishimura,
   All rights reserved.                          

   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:

     1. Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.

     2. Redistributions in binary form must reproduce the above copyright
        notice, this list of conditions and the following disclaimer in the
        documentation and/or other materials provided with the distribution.

     3. The names of its contributors may not be used to endorse or promote 
        products derived from this software without specific prior written 
        permission.

   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
   A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
   NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
   SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace IronTjs.Builtins
{
    public class MersenneTwister : Random
	{
		// Period parameters
		const int StateVectorLength = 624;
		const int M = 397;
		const uint AVector = 0x9908b0dfU;   // constant vector a
		const uint UpperMask = 0x80000000U; // most significant w-r bits
		const uint LowerMask = 0x7fffffffU; // least significant r bits
		static uint[] mag01 = new[] { 0x0U, AVector }; // mag01[x] = x * MATRIX_A  for x=0,1

		public MersenneTwister()
		{
			var value = DateTime.Now.Ticks;
			Initialize(new uint[] { (uint)((ulong)value >> 32), (uint)(value & 0xffffffff) });
		}

		public MersenneTwister(uint seed) { Initialize(seed); }

		public MersenneTwister(uint[] initializationVectors) { Initialize(initializationVectors); }

		uint[] stateVector = new uint[StateVectorLength];
		int stateVectorIndex = StateVectorLength + 1; // mti == N+1 means mt[N] is not initialized

		public void Initialize(uint seed)
		{
			stateVector[0] = seed;
			for (stateVectorIndex = 1; stateVectorIndex < stateVector.Length; stateVectorIndex++)
			{
				stateVector[stateVectorIndex] = (uint)(1812433253U * (stateVector[stateVectorIndex - 1] ^ (stateVector[stateVectorIndex - 1] >> 30)) + stateVectorIndex);
				// See Knuth TAOCP Vol2. 3rd Ed. P.106 for multiplier.
				// In the previous versions, MSBs of the seed affect only MSBs of the array mt[].
				// 2002/01/09 modified by Makoto Matsumoto
			}
		}

		public void Initialize(uint[] initializationVectors)
		{
			int i = 1;
			Initialize(19650218U);
			for (int j = 0, k = System.Math.Max(stateVector.Length, initializationVectors.Length); k > 0; k--)
			{
				stateVector[i] = (uint)((stateVector[i] ^ ((stateVector[i - 1] ^ (stateVector[i - 1] >> 30)) * 1664525U)) + initializationVectors[j] + j); // non linear
				i++;
				j++;
				if (i >= stateVector.Length)
				{
					stateVector[0] = stateVector[stateVector.Length - 1];
					i = 1;
				}
				if (j >= initializationVectors.Length)
					j = 0;
			}
			for (int k = stateVector.Length - 1; k > 0; k--)
			{
				stateVector[i] = (uint)((stateVector[i] ^ ((stateVector[i - 1] ^ (stateVector[i - 1] >> 30)) * 1566083941U)) - i); // non linear
				i++;
				if (i >= stateVector.Length)
				{
					stateVector[0] = stateVector[stateVector.Length - 1];
					i = 1;
				}
			}
			stateVector[0] = 0x80000000U; // MSB is 1; assuring non-zero initial array
		}

		// generates a random number on [0,0xffffffff]-interval
		public uint NextUInt32()
		{
			uint y;
			if (stateVectorIndex >= stateVector.Length)
			{
				// generate N words at one time
				int i;
				// if Initialize(uint) has not been called, a default initial seed is used
				if (stateVectorIndex == stateVector.Length + 1)
					Initialize(5489U);
				for (i = 0; i < stateVector.Length - M; i++)
				{
					y = (stateVector[i] & UpperMask) | (stateVector[i + 1] & LowerMask);
					stateVector[i] = stateVector[i + M] ^ (y >> 1) ^ mag01[y & 0x1UL];
				}
				for (; i < stateVector.Length - 1; i++)
				{
					y = (stateVector[i] & UpperMask) | (stateVector[i + 1] & LowerMask);
					stateVector[i] = stateVector[i + (M - stateVector.Length)] ^ (y >> 1) ^ mag01[y & 0x1UL];
				}
				y = (stateVector[stateVector.Length - 1] & UpperMask) | (stateVector[0] & LowerMask);
				stateVector[stateVector.Length - 1] = stateVector[M - 1] ^ (y >> 1) ^ mag01[y & 0x1UL];
				stateVectorIndex = 0;
			}
			y = stateVector[stateVectorIndex++];
			// Tempering
			y ^= (y >> 11);
			y ^= (y << 7) & 0x9d2c5680U;
			y ^= (y << 15) & 0xefc60000U;
			y ^= (y >> 18);
			return y;
		}

		// generates a random number on [0,0x7fffffff]-interval
		public override int Next() { return (int)(NextUInt32() >> 1); }

		public override int Next(int maxValue) { return (int)(NextUInt32() * maxValue / 4294967296); }

		public override int Next(int minValue, int maxValue) { return (int)(NextUInt32() * (maxValue - minValue) / 4294967296) + minValue; }

		// generates a random number on [0,1]-real-interval
		public double NextDoubleClosed() { return NextUInt32() * (1.0 / 4294967295.0); } // divided by 2^32-1

		// generates a random number on [0,1)-real-interval
		public override double NextDouble() { return NextUInt32() * (1.0 / 4294967296.0); } // divided by 2^32
		
		// generates a random number on (0,1)-real-interval
		public double NextDoubleOpen() { return (((double)NextUInt32()) + 0.5) * (1.0 / 4294967296.0); } // divided by 2^32

		// generates a random number on [0,1) with 53-bit resolution
		public double NextDouble53()
		{
			var a = NextUInt32() >> 5;
			var b = NextUInt32() >> 6;
			return (a * 67108864.0 + b) * (1.0 / 9007199254740992.0);
		}

		public override void NextBytes(byte[] buffer)
		{
			int j = 0;
			uint sample = 0;
			for (int i = 0; i < buffer.Length; i++)
			{
				if (j <= 0)
				{
					sample = NextUInt32();
					j = 4;
				}
				buffer[i] = (byte)(sample & 0xff);
				sample >>= 8;
				j--;
			}
		}

		protected override double Sample() { return NextDouble(); }

		public Dictionary<string, object> ToDictionary()
		{
			return new Dictionary<string, object>()
			{
				{ "stateVector", stateVector.ToArray() },
				{ "vectorIndex", stateVectorIndex }
			};
		}

		public static MersenneTwister FromDictionary(IDictionary<string, object> storage)
		{
			var inst = new MersenneTwister();
			int i = 0;
			foreach (var item in (System.Collections.IEnumerable)storage["stateVector"])
				inst.stateVector[i++] = Convert.ToUInt32(item);
			inst.stateVectorIndex = Convert.ToInt32(storage["vectorIndex"]);
			return inst;
		}
	}
}
