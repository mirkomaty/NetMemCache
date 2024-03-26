namespace BinaryRage.Interfaces
{
    public interface IKeyHandler
    {
        string NormalizeKey( string key );
        string ComputeKey( object rawKey );
	}
}