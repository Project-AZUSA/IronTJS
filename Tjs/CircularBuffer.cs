using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Scripting.Utils;

namespace IronTjs
{
    public class CircularBuffer<T> : IList<T>
	{
		public CircularBuffer() : this(DefaultCapacity) { }

		public CircularBuffer(int capacity)
		{
			capacity = Pow2((uint)capacity);
			_data = new T[capacity];
			_top = _bottom = 0;
		}

		public CircularBuffer(IEnumerable<T> collection)
		{
			var array = collection.ToArray();
			_data = new T[Math.Max(Pow2((uint)array.Length), DefaultCapacity)];
			array.CopyTo(_data, 0);
			_top = 0;
			_bottom = array.Length;
		}

		const int DefaultCapacity = 256;
		T[] _data;
		int _top, _bottom;

		static int Pow2(uint n)
		{
			--n;
			int p = 0;
			for (; n != 0; n >>= 1)
				p = (p << 1) + 1;
			return p + 1;
		}

		int Mask { get { return _data.Length - 1; } }

		void EnsureCapacity()
		{
			if (Count >= _data.Length - 1)
			{
				var data = new T[_data.Length * 2];
				CopyTo(data, 0);
				_bottom = Count;
				_top = 0;
				_data = data;
			}
		}

		public void Sort(IComparer<T> comparer, bool stable)
		{
			if (stable)
			{
				var data = this.OrderBy(x => x, comparer).ToArray();
				Array.Copy(data, _data, Count);
				Array.Clear(_data, Count, _data.Length - Count);
			}
			else
			{
				// ソートしやすいようにデータを詰める
				var data = new T[_data.Length];
				CopyTo(data, 0);
				Array.Sort(data, 0, Count, comparer);
				_data = data;
			}
			_bottom = Count;
			_top = 0;
		}

		public int IndexOf(T item)
		{
			for (int i = 0; i < Count; i++)
			{
				if (EqualityComparer<T>.Default.Equals(this[i], item))
					return i;
			}
			return -1;
		}

		public void Insert(int index, T item)
		{
			ContractUtils.RequiresArrayInsertIndex(this, index, "index");
			EnsureCapacity();
			if (index < Count / 2)
			{
				_top = (_top - 1) & Mask;
				for (var i = 0; i < index; ++i)
					this[i] = this[i + 1];
				this[index] = item;
			}
			else
			{
				_bottom = (_bottom + 1) & Mask;
				for (var i = Count - 1; i > index; --i)
					this[i] = this[i - 1];
				this[index] = item;
			}
		}

		public void RemoveAt(int index)
		{
			ContractUtils.RequiresArrayIndex(this, index, "index");
			if (index < Count / 2)
			{
				for (var i = index; i > 0; --i)
					this[i] = this[i - 1];
				_top = (_top + 1) & Mask;
			}
			else
			{
				for (var i = index; i < Count - 1; ++i)
					this[i] = this[i + 1];
				_bottom = (_bottom - 1) & Mask;
			}
		}

		public T this[int index]
		{
			get
			{
				ContractUtils.RequiresArrayIndex(this, index, "index");
				return _data[(index + _top) & Mask];
			}
			set
			{
				ContractUtils.RequiresArrayIndex(this, index, "index");
				_data[(index + _top) & Mask] = value;
			}
		}

		public void Add(T item) { Insert(Count, item); }

		public void Clear() { _top = _bottom = 0; }

		public bool Contains(T item) { return IndexOf(item) >= 0; }

		public void CopyTo(T[] array, int arrayIndex)
		{
			ContractUtils.RequiresNotNull(array, "array");
			ContractUtils.RequiresArrayRange(array, arrayIndex, Count, "arrayIndex", "Count");
			foreach (var item in this)
				array[arrayIndex++] = item;
		}

		public int Count
		{
			get
			{
				var count = _bottom - _top;
				if (count < 0)
					count += _data.Length;
				return count;
			}
		}

		public bool IsReadOnly { get { return false; } }

		public bool Remove(T item)
		{
			var index = IndexOf(item);
			if (index >= 0)
			{
				RemoveAt(index);
				return true;
			}
			return false;
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (_top <= _bottom)
			{
				for (var i = _top; i < _bottom; ++i)
					yield return _data[i];
			}
			else
			{
				for (var i = _top; i < _data.Length; ++i)
					yield return _data[i];
				for (var i = 0; i < _bottom; ++i)
					yield return _data[i];
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}
