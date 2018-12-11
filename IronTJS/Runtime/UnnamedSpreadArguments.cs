using System;
using System.Collections.Generic;

namespace IronTJS.Runtime
{
    class UnnamedSpreadArguments : IList<object>
	{
		public UnnamedSpreadArguments(object[] arguments, int spreadStart)
		{
			_arguments = arguments;
			_spreadStart = spreadStart;
		}

		object[] _arguments;
		int _spreadStart;

		public int IndexOf(object item)
		{
			int i = 0;
			foreach (var obj in this)
			{
				if (Equals(item, obj))
					return i;
				i++;
			}
			return -1;
		}

		public void Insert(int index, object item) { throw new NotSupportedException(); }

		public void RemoveAt(int index) { throw new NotSupportedException(); }

		public object this[int index]
		{
			get { return _arguments[_spreadStart + index]; }
			set { _arguments[_spreadStart + index] = value; }
		}

		public void Add(object item) { throw new NotSupportedException(); }

		public void Clear() { throw new NotSupportedException(); }

		public bool Contains(object item) { return IndexOf(item) >= 0; }

		public void CopyTo(object[] array, int arrayIndex)
		{
			foreach (var item in this)
				array[arrayIndex++] = item;
		}

		public int Count { get { return _spreadStart > _arguments.Length ? 0 : _arguments.Length - _spreadStart; } }

		public bool IsReadOnly { get { return false; } }

		public bool Remove(object item) { throw new NotSupportedException(); }

		public IEnumerator<object> GetEnumerator()
		{
			for (int i = _spreadStart; i < _arguments.Length; i++)
				yield return _arguments[i];
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}
