namespace OpenManta.Framework
{
	public interface IMantaCoreEvents
	{
		void InvokeMantaCoreStopping();

		void RegisterStopRequiredInstance(IStopRequired instance);
	}
}