namespace NetMemCache.Interfaces
{
    public interface IKeyHandler
    {
        string NormalizeKey( string key, bool isPath );
        string ComputeKey( object rawKey );
	}
}