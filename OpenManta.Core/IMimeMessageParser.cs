namespace OpenManta.Core
{
	public interface IMimeMessageParser
	{
		MimeMessage Parse(string message);

		string UnfoldHeaders(string headersBlock);
	}
}