using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace OpenManta.Service
{
	[RunInstaller(true)]
	public class OpenMantaServiceInstaller : Installer
	{
		/// <summary>
        /// Public Constructor for WindowsServiceInstaller.
        /// - Put all of your Initialization code here.
        /// </summary>
		public OpenMantaServiceInstaller()
        {
            this.Installers.Add(new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalService,
                Username = null,
                Password = null
            });
            this.Installers.Add(new ServiceInstaller {
                ServiceName = "OpenManta",
                Description = "The OpenManta mail transfer agent",
                DisplayName = "OpenManta",
                StartType = ServiceStartMode.Automatic
        });
        }
	}
}
