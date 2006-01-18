
namespace Simias
{
	/// <summary>
	/// Interface for an external identity sync provider
	/// </summary>
	public interface IIdentitySyncProvider
	{
		#region Properties
		/// <summary>
		/// Gets the name of the provider.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the description of the provider.
		/// </summary>
		string Description { get; }
		#endregion

		#region Public Methods
		/// <summary>
		/// Call to abort an in process synchronization
		/// </summary>
		/// <returns>N/A</returns>
		void Abort();
		
		/// <summary>
		/// Call to inform a provider to start a synchronization cycle
		/// </summary>
		/// <returns>True - provider successfully started a sync cycle, False - provider could
		/// not start the sync cycle.</returns>
		bool Start();

		#endregion
	}
}	
