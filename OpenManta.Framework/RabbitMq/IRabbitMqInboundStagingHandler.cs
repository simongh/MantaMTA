namespace OpenManta.Framework.RabbitMq
{
	public interface IRabbitMqInboundStagingHandler
	{
		void Start();

		void Stop();
	}
}