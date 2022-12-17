using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFPluginCore
{
    /* ****【自定义导出特性】****
     * [MEFCustomExportMetadata("1122","插件1")]
     * public partial class PluginPageView : Page, IMEFView
     * {}
     * 等效于：
     * [Export(typeof(IMEFView))
     *     ,ExportMetadata("ID","12345")
     *     ,ExportMetadata("Name","插件1")]
     * public partial class PluginPageView : Page, IMEFView
     * {}
     * 
     * 《特性化编程模型概述 (MEF)》
     * https://learn.microsoft.com/zh-cn/dotnet/framework/mef/attributed-programming-model-overview-mef
     */

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MEFCustomExportMetadataAttribute : ExportAttribute, IMEFMetadata
    {
        public string ID { get; }
        public string Name { get; }
        public string Version { get; }//接口中有默认值的属性也要实现。
        public string Description { get; }
        public bool NeedCreatNewInstanceEverytime { get; }

        public MEFCustomExportMetadataAttribute(bool needCreatNewInstanceEverytime, string id, string name, string version = "1.0.0.0", string description = "")
            : base(typeof(IMEFView))
        {
            ID = id;
            Name = name;
            Version = version;//接口中有默认值的属性也要赋值。
            Description = description;
            NeedCreatNewInstanceEverytime = needCreatNewInstanceEverytime;
        }
    }
}
