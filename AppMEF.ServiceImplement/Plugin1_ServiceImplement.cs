using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEFPluginCore;

namespace AppMEF.ServiceImplement
{
    [Export("Plugin1_Implement", typeof(IMEFService))]
    public class Plugin1_ServiceImplement : IMEFService
    {
        public void AddNumber(int numuber, Action<int> action)
        {
            action(numuber + 5);
        }
    }
}
