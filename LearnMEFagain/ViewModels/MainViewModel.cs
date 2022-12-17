using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LearnMEFagain.MEFParts;
using MEFPluginCore;

namespace LearnMEFagain.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        #region 定义属性发生变化时引发的事件及相关操作（里面的内容是固定的，直接用。）
        /*--------监听事件处理程序------------------------------------------------------------------------*/
        /// <summary>
        /// 属性发生变化时引发的事件
        /// </summary>
        //[field: NonSerializedAttribute()]//保证事件PropertyChanged不被序列化的必要设定。事件不能序列化！
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 提醒侦听者们（listeners）属性已经变化
        /// </summary>
        /// <param name="propertyName">变化的属性名称。
        /// 这是可选参数，能够被CallerMemberName自动提供。
        /// 当然你也可以手动输入</param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /* 上面原型
         protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
         */

        // 下面这个方法目前不知道干啥的
        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(storage, value))
            {
                return false;
            }
            else
            {
                storage = value;
                NotifyPropertyChanged(propertyName);

                return true;
            }
        }
        /*------------------------------------------------------------------------------------------------*/
        #endregion

        public MainViewModel()
        {
            mefContainer = MEFContainer.CreatMEFContainer(new object[] { this }, "", "AppMEF.*");//注意用法
            //pList = MEFContainer.GetExportsLazyFromContainer<IMEFView, IMEFMetadata>(mefContainer);
            //在pList上使用特性[ImportMany]替代。
            StartLoadingPlugList();
        }
        private CompositionContainer? mefContainer;

        #region 插件集合通用型导入流程(插件视图View 为用户控件UserControl、页面Page、窗体Window均可)
        /* 《情况说明》
         * --需添加NuGget包：System.ComponentModel.Composition--
         * 以往，对于 不需要每次创建实例(如 Page、UserControl)的部件 和 需要每次创建实例(如 Window) 的MEF组件。
         * 往往采用不同的处理方式，分情况处理。
         * 情况1：不需要每次创建实例(如 Page、UserControl)的MEF组件，使用 Lazy<T, TMetadata> 来加载。
         * 情况2：需要每次创建实例(如 Window) 的MEF组件，则使用 ExportFactory<T, TMetadata> 来加载。
         * —-参照《【WPF】运用MEF实现窗口的动态扩展》https://www.cnblogs.com/tcjiaan/p/5844619.html
         * --参照《ExportFactory<T,TMetadata> 类》https://learn.microsoft.com/zh-cn/dotnet/api/system.composition.exportfactory-2?view=dotnet-plat-ext-7.0
         * 这样虽然能用，但是：
         * 1、还是不方便，要分情况处理。
         * 2、容器 CompositionContainer 的方法中， GetExports 和 GetExport 方法均返回 Lazy 类型。
         * 
         * 那么，能不能将 情况2 统一到 情况1 中去？
         * ****【用 Lazy类型 来加载需要每次创建实例(如 Window)的 MEF组件】****
         * 可以，解决方案就是：
         * 1、在MEF组件继承的接口中(本例为IMEFView)，定义一个 CreatInstance 方法。
         * 2、按照常规方式使用 Lazy类型 加载MEF组件。
         * 3、通过调用CreatInstanceEverytime方法来创建新的实例窗口。
         * 该方法的作用：
         * 1、如果该插件不需要创建实例(如 Page、UserControl)，那只需要在该方法的实现中保持为空。
         * 2、如果该插件需要创建实例(如 Window)，就要在该方法的实现中生成实例。
         * 
         * [ImportMany]特性可修饰字段、属性或构造函数参数。
         * Lazy<T,TMetadata> -- Lazy<[协定类型接口],[元数据视图接口]>
         * 延迟导入本质上就是“延迟初始化”，这是Lazy<T>的特性。
         * 所谓“延迟初始化”，即只有在首次访问 Lazy<T>.Value 属性时，才创建实例的方式。
         * ****Lazy<T,TMetadata>能够自动隐式转换为Lazy<T>。****
         * ****MEF插件生成位置要求和搜索模式searchPattern设置见MEFContainer.CreatMEFContainer()方法中注释。****
         * 参考：
         * 《特性化编程模型概述 (MEF)》
         * https://docs.microsoft.com/zh-cn/dotnet/framework/mef/attributed-programming-model-overview-mef
         * 《Lazy<T,TMetadata> 类》
         * https://learn.microsoft.com/zh-cn/dotnet/api/system.lazy-2?view=net-7.0
         */

        /// <summary>
        /// 使用 [ImportMany] 特性的方式。
        /// 可以省略(等同于)：
        /// var res = mefContainer.GetExports<IMEFView, IMEFMetadata>();
        /// pList = new ObservableCollection<Lazy<IMEFView, IMEFMetadata>>(res);
        /// 即本类构造函数中使用的 MEFContainer.GetExportsLazyFromContainer 方法。
        /// </summary>
        [ImportMany]
        private ObservableCollection<Lazy<IMEFView, IMEFMetadata>>? pList = new();
        public ObservableCollection<Lazy<IMEFView, IMEFMetadata>> PluginList { get; private set; } = new();

        private IMEFView? currentPlugin_Page;
        public IMEFView? CurrentPlugin_Page//不需要每次创建实例(如 Page、UserControl)的MEF组件。
        {
            get { return currentPlugin_Page; }
            set { currentPlugin_Page = value; NotifyPropertyChanged(); }
        }

        private IMEFView? currentPlugin_Window;
        public IMEFView? CurrentPlugin_Window//需要每次创建实例(如 Window) 的MEF组件。
        {
            get { return currentPlugin_Window; }
            set { currentPlugin_Window = value; NotifyPropertyChanged(); }
        }

        private void StartLoadingPlugList()
        {
            PluginList.Clear();
            if (pList != null)
            {
                foreach (var p in pList)
                {
                    //根据元数据信息判断哪个插件需要加载。
                    if (p.Metadata.Name == "插件1")
                    {
                        PluginList.Add(p);
                        CurrentPlugin_Page = p.Value;//不需要每次创建实例(如 Page、UserControl)的MEF组件。
                    }
                    else if(p.Metadata.Name == "插件2")
                    {
                        PluginList.Add(p);
                        CurrentPlugin_Window = p.Value;//需要每次创建实例(如 Window) 的MEF组件。
                    }
                    else
                    { }
                }
                //PluginsList_LazyMode.OrderByDescending(i => i.PluginVersion);
                //CurrentPlugin = PluginList?.FirstOrDefault();//Lazy<T,TMetadata>能够自动隐式转换为Lazy<T>。
            }
            else
            { }
        }

        #endregion

        public void ActivactedWindowPlugin()
        {
            /* 使用 Lazy<T,TMetadata> 时，如何加载需要每次创建实例的窗口。*/
            CurrentPlugin_Window?.CreatNewInstanceEverytime();

            /* 使用 ExportFactory<T,TMetadata> 时，如何加载需要每次创建实例的窗口。
             * var temp = CurrentPlug_CreatInstanceMode?.CreateExport();
             * Window? w = temp?.Value as Window;
             * w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
             * w?.Show();
             */
        }
    }
}
