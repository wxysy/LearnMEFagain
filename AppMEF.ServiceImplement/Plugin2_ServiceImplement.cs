using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEFPluginCore;

namespace AppMEF.ServiceImplement
{
    [Export("Plugin2_Implement", typeof(IMEFService))]
    public class Plugin2_ServiceImplement : IMEFService
    {
        public void AddNumber(int numuber, Action<int> action)
        {
            action(numuber + 10);
        }
    }
}
