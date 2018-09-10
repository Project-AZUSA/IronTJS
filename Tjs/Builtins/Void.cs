namespace IronTjs.Builtins
{
    public sealed class Void
	{
		public static readonly Void Value = new Void();

		Void() { }

		public override string ToString() { return ""; }
	}
}
