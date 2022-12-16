using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFPluginCore
{
    public interface IMEFMetadata
    {
        /* 凡是 IMetadata 中没有默认值的属性，都必须要在插件处赋值，否则找不到插件。*/
        string ID { get; }
        string Name { get; }
        bool NeedCreatInstance { get; }

        /* 已经有默认值的就不必须，可改可不改。*/
        [DefaultValue("1.0.0.0")]
        string Version { get; }
        [DefaultValue("")]
        string Description { get; }     
    }
}
