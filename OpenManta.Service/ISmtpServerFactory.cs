namespace OpenManta.Service
{
	public interface ISmtpServerFactory
	{
		Framework.ISmtpServer Create();
	}
}