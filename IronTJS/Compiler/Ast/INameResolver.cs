namespace IronTJS.Compiler.Ast
{
    /// <summary>
    /// 変数宣言と識別子解決に関係する機能をカプセル化します。
    /// このインターフェイスが実装されたノードに対して、var を使用して変数を宣言したり、特定の識別子がどの変数または引数にマッピングされるかを調べたりすることができます。
    /// </summary>
    public interface INameResolver
	{
		/// <summary>指定された識別子に対する値の読み込みを行う式を取得します。読み込むことができない場合は <c>null</c> を返します。</summary>
		/// <param name="name">値を読み込む識別子を指定します。</param>
		/// <param name="direct">識別子をオブジェクトが保持している場合に、プロパティオブジェクトを取得するかどうかを示す値を指定します。</param>
		/// <returns>識別子の値を読み込む式。読み込めないか識別子がノード内で見つからない場合は <c>null</c>。</returns>
		System.Linq.Expressions.Expression ResolveForRead(string name, bool direct);

		/// <summary>指定された識別子に対する値の書き込みを行う式を取得します。書き込むことができない場合は <c>null</c> を返します。</summary>
		/// <param name="name">値を書き込む識別子を指定します。</param>
		/// <param name="value">識別子に書き込む値を指定します。</param>
		/// <param name="direct">識別子をオブジェクトが保持している場合に、プロパティオブジェクトを設定するかどうかを示す値を指定します。</param>
		/// <returns>識別子に値を書き込む式。書き込めないか識別子がノード内で見つからない場合は <c>null</c>。</returns>
		System.Linq.Expressions.Expression ResolveForWrite(string name, System.Linq.Expressions.Expression value, bool direct);

		/// <summary>指定された識別子と値の関連付けを解除する式を取得します。解除できない場合は <c>null</c> を返します。</summary>
		/// <param name="name">値との関連付けを解除する識別子を指定します。</param>
		/// <returns>値と識別子の関連付けを解除する式。解除できないか識別子がノード内で見つからない場合は <c>null</c>。</returns>
		System.Linq.Expressions.Expression ResolveForDelete(string name);

		/// <summary>このノードに指定された識別子の変数を宣言します。変数宣言がサポートされていない場合は <c>null</c> を返します。</summary>
		/// <param name="name">変数につける名前を指定します。</param>
		/// <param name="value">変数の初期値を指定します。</param>
		/// <returns>指定された名前の変数に初期値を設定する式。変数宣言がサポートされていない場合は <c>null</c>。</returns>
		System.Linq.Expressions.Expression DeclareVariable(string name, System.Linq.Expressions.Expression value);
	}
}
