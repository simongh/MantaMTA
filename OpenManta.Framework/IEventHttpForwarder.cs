using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenManta.Framework
{
	internal interface IEventHttpForwarder : IStopRequired
	{
		void Start();
	}
}