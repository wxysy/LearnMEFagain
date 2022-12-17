using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MEFPluginCore;

namespace AppMEF.Plugin2
{
    /// <summary>
    /// PluginWindowView.xaml 的交互逻辑
    /// </summary>
    [MEFCustomExportMetadata(true,"2233", "插件2")]
    public partial class PluginWindowView : Window, IMEFView//**必须继承IMEFView接口**
    {
        [ImportingConstructor]//构造函数有参数时就要使用
        public PluginWindowView([Import("Plugin2_Implement")] IMEFService service)
        {
            /* 【导入其他MEF组件供本组件使用】--构造函数参数
             * 当构造函数参数使用[ImportMany]时：
             * ****构造函数参数用的 ImportMany 必须用 IEnumerable<T> 或者 T[]。****
             * 不能用 List，也不能用 ObservableCollection。
             * 而且，既然能导入类，那么自然也能导入 方法 和 属性。
             * 导入导出要有同样的输出位置。
             * 构造函数参数导入仍然是需要容器的，它只能寻找容器内的MEF组件。
             * 只是这里的容器为 主项目-MainViewModel.cs 里的 mefContainer。
             * 和其他导入公用容器。
             */
            InitializeComponent();
            mEFService = service;
        }

        /* 该变量用于存储导入的MEF部件，非常重要。CreatInstance方法也可能会用到。*/
        IMEFService mEFService;

        public void CreatInstanceEverytime(params object[] input)
        {
            /* 该方法为接口 IMEFView 中的方法 CreatInstance 的实现。
             * 1、如果该插件不需要创建实例(如 Page、UserControl)，那只需要在该方法的实现中保持为空。
             * 2、如果该插件需要创建实例(如 Window)，就要在该方法的实现中生成实例。
             */
            //----这里就是情况2----

            /**** 使用本类中的 字段(mEFService) 来带入其他MEF组件。****/
            PluginWindowView pp = new(mEFService);
            pp.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            pp.Show();
        }

        int i = 15;
        private void Btn_Action_Click(object sender, RoutedEventArgs e)
        {
            mEFService?.AddNumber(i, p =>
            {
                i = p;
                //虽然在这里i=p没变化，但是在服务实现Plugin2_ServiceImplement中，每次p都加10。
            });
            textBox.Text = i.ToString();
        }      
    }
}
