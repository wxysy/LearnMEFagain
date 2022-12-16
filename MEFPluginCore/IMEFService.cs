using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFPluginCore
{
	public interface IMEFService
    {
		void AddNumber(int numuber, Action<int> action);
	}
}
