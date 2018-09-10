namespace IronTjs.Runtime
{
    public interface IContextChangeable
	{
		object Context { get; }

		IContextChangeable ChangeContext(object context);
	}
}
