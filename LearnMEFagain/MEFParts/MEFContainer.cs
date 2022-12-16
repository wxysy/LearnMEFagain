using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MEFPluginCore;

namespace LearnMEFagain.MEFParts
{
    public class MEFContainer
    {
        //该静态方法，可行。
        public static CompositionContainer CreatMEFContainer(object[] attributedParts, string pluginExtendDirectoryPath = "", string searchPattern = "AppMEF.*")
        {
            //0-创建用于合并 ComposablePartCatalog 对象元素的目录（就是一个集合）
            AggregateCatalog catalogs = new();

            //1-指出插件放在哪个位置（**这里要根据实际情况改**）
            // 主程序设定的输出路径为：..\Output
            // 插  件设定的输出路径为：..\Output
            // 事实上，只要一个插件设定为与主程序相同的输出路径，
            // 其他插件即使不设定，也依然会在主程序输出路径出现（*.dll 和 *.pdb 文件），很神奇。
            var dir1 = AppDomain.CurrentDomain.BaseDirectory;// 方法1
            //var dir2 = Path.Combine(dir1, "Plugins");// 方法2
            // 方法2的目录是："..\..\Debug\net6.0-windows\Plugins"，不同于方法3。
            //var dir3 = @"..\..\Plugins\Debug\net6.0-windows";// 方法3           

            //2-初始化每个目录下所有符合搜索条件的DLL对象，并放入对象集合之中。
            /* 
             * 《关于搜索模式 searchPattern》
             * 1、允许使用通配符 * 和 ？，支持字符串内插 $，支持原义识别符 @；但不支持正则表达式。
             * 2、一次只能匹配一个模式，希望像OpenFileDialog对话框一样同时匹配两个模式"*.txt|*.xlsx"是不行的，只能是"*.txt"或"*.xlsx"。
             * 
             * 可以使用占位符 * 和 ?，如果想搜索全部文件，直接 searchPattern = "*"或者"*.*"。
             * 一般都会是搜索特定名称，如 searchPattern = "PluginMEF.*";
             * 代表搜寻 PluginMEF.AppOneMEF.dll 这样的文件，后缀名不限。
             * 也可以结合内插字符串用，如 searchPattern = $"PluginMEF.{guid}.*";
             * string searchPattern = $"*";
             * 
             * string searchPattern = $"PluginMEF.*";
             * 这句话就限定了，创建MEF插件项目时的项目名称格式。
             * 必须是“PluginMEF.AppOne”这样的，否则就不会搜索到。
             */
            string searchP = searchPattern;

            var catalog1 = new DirectoryCatalog(dir1, searchP);
            //var catalog2 = new DirectoryCatalog(dir2);

            catalogs.Catalogs.Add(catalog1);
            //catalogs.Catalogs.Add(catalog2);

            //从一个程序集获取所有的组件定义，AssemblyCatalog：表示从程序集中搜索部件的目录。
            //catalog.Catalogs.Add(new AssemblyCatalog(typeof(MyCalculate).Assembly));

            //增加一个搜索范围Extend
            string dirExtend = pluginExtendDirectoryPath;
            if (Directory.Exists(dirExtend))
            {
                var catalogExtend = new DirectoryCatalog(dirExtend, searchP);
                catalogs.Catalogs.Add(catalogExtend);
            }
            else
            { }

            //3-将所有DLL对象进行组装，创建容器。
            var mefContainer = new CompositionContainer(catalogs);
            //using CompositionContainer mefContainer = new CompositionContainer(catalogs);

            //4-指定最终组装承载的容器对象
            //NuGet中引用：System.ComponentModel.Composition
            //添加引用 using System.ComponentModel.Composition
            mefContainer.ComposeParts(attributedParts);//通常 ComposeParts(this); 搞定

            return mefContainer;

            //----这剩下的工作，就是之后的事情了。----
            //5-加载控件列表，指定当前控件。

            //6-释放MEF容器
            //mefContainer.Dispose();//哪里需要就用在哪里。
        }

        //该静态方法，可行。
        public static ObservableCollection<Lazy<T, TMetadata>> GetExportsLazyFromContainer<T, TMetadata>(CompositionContainer container)
        {
            var temp = container.GetExports<T, TMetadata>();
            var res = new ObservableCollection<Lazy<T, TMetadata>>(temp);
            return res;

            /// <summary>
            /// 如果使用 [ImportMany] 特性的方式，类中使用了该特性的变量或属性会自动获取插件。
            /// 但该方式只能在类中变量或属性使用。
            /// 与本方法等效，某些情况下更方便。
            /// </summary>
            //[ImportMany]
            //private ObservableCollection<Lazy<IMEFView, IMEFMetadata>>? pList_LazyMode = new();
        }
    }
}
