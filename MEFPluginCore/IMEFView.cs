using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFPluginCore
{
    public interface IMEFView//**MEF插件的主接口**
    {
        void CreatInstance(params object[] input);//创建MEF插件实例的方法。
        /* 该方法的作用：
         * 1、如果该插件不需要创建实例(如 Page、UserControl)，那只需要在该方法的实现中保持为空。
         * 2、如果该插件需要创建实例(如 Window)，就要在该方法的实现中生成实例。
         */
    }
}
