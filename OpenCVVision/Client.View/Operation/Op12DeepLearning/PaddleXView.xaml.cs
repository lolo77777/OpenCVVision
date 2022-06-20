using Microsoft.Win32;

namespace Client.View.Operation.Op12DeepLearning;

/// <summary>
/// PaddleXView.xaml 的交互逻辑
/// </summary>
public partial class PaddleXView
{
    public PaddleXView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.BindCommand(ViewModel, vm => vm.LoadClsModelCommand, v => v.btnClsLoad).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.DestoryClsModelCommand, v => v.btnClsDestory).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.LoadDetModelCommand, v => v.btnDetLoad).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.DestoryDetModelCommand, v => v.btnDetDestory).DisposeWith(d);

            this.OneWayBind(ViewModel, vm => vm.ClsLoaded, v => v.btnClsLoad.IsEnabled, b => !b).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.ClsLoaded, v => v.btnClsDestory.IsEnabled).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.DetLoaded, v => v.btnDetLoad.IsEnabled, b => !b).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.DetLoaded, v => v.btnDetDestory.IsEnabled).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.ClsLoaded, v => v.panelDet.IsEnabled, b => !b).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.DetLoaded, v => v.panelCls.IsEnabled, b => !b).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.ClsResult, v => v.txtClsResult.Text).DisposeWith(d);
        });
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetDataObject("https://gitee.com/paddlepaddle/PaddleX/tree/develop/deploy/cpp/docs/csharp_deploy");
        MessageBox.Show("已复制到剪贴板！");
    }

    private void Modellink_Click(object sender, EventArgs e)
    {
        Clipboard.SetDataObject("https://cloud.189.cn/web/share?code=iQJRJvUBvYfe（访问码：3vll）");
        MessageBox.Show("已复制到剪贴板！");
    }

    private void Modellink_Click1(object sender, EventArgs e)
    {
        Clipboard.SetDataObject("https://cloud.189.cn/web/share?code=RZFnUjfiuUVr（访问码：4bno）");
        MessageBox.Show("已复制到剪贴板！");
    }
}

public class BrowserService
{
    public static void OpenGoogleBrowserUrl(string url)
    {
        try
        {
            //注册表路径
            var openKey = IsWin32() ? @"SOFTWARE\Google\Chrome" : @"SOFTWARE\Wow6432Node\Google\Chrome";
            //打开注册表
            var appPath = Registry.LocalMachine.OpenSubKey(openKey);
            if (appPath != null)
            {
                //用谷歌打开（如果谷歌卸载了，但是注册表没有清空，会有bug）
                var result = Process.Start("chrome.exe", url);
                //用IE打开
                if (result == null) OpenIeBrowserUrl(url);
            }
            else
            {
                //用谷歌打开
                var result = Process.Start("chrome.exe", url);
                //用默认浏览器打开
                if (result == null) OpenDefaultBrowserUrl(url);
            }
        }
        catch (Exception ex)
        {
            //调用默认浏览器打开
            OpenDefaultBrowserUrl(url);
        }
    }

    public static void OpenIeBrowserUrl(string url)
    {
        try
        {
            //打开IE
            Process.Start("iexplore.exe", url);
        }
        catch (Exception ex)
        {
            try
            {
                //ie浏览器路径
                string iePath = @"C:\Program Files\Internet Explorer\iexplore.exe";
                if (File.Exists(iePath))
                {
                    //参数
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = iePath,
                        Arguments = url,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    //打开
                    Process.Start(processStartInfo);
                }
                else
                {
                    //ie浏览器路径
                    iePath = @"C:\Program Files (x86)\Internet Explorer\iexplore.exe";
                    if (File.Exists(iePath))
                    {
                        //参数
                        var processStartInfo = new ProcessStartInfo
                        {
                            FileName = iePath,
                            Arguments = url,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        //打开
                        Process.Start(processStartInfo);
                    }
                    else
                    {
                        //提示
                        throw new Exception("系统未安装IE浏览器");

                        //if (MessageBox.Show(@"系统未安装IE浏览器，是否下载安装？", null, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                        //{
                        //    //打开下载IE网站
                        //    OpenDefaultBrowserUrl("http://windows.microsoft.com/zh-cn/internet-explorer/download-ie");
                        //}
                    }
                }
            }
            catch (Exception exception)
            {
            }
        }
    }

    public static void OpenDefaultBrowserUrl(string url)
    {
        try
        {
            //读取注册表
            var key = Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command\");
            if (key != null)
            {
                //读取（s就是默认浏览器路径，参数格式："D:\Program Files (x86)\Google\Chrome\Application\chrome.exe" -- "%1"）
                string s = key.GetValue("").ToString();
                //索引
                var lastIndex = s.IndexOf(".exe", StringComparison.Ordinal);
                //索引
                if (lastIndex == -1) lastIndex = s.IndexOf(".EXE", StringComparison.Ordinal);
                //路径
                var path = s.Substring(1, lastIndex + 3);
                //打开
                var result = Process.Start(path, url);
                if (result == null)
                {
                    //打开
                    var result1 = Process.Start("explorer.exe", url);
                    //打开
                    if (result1 == null) Process.Start(url);
                }
            }
            else
            {
                //打开
                var result1 = Process.Start("explorer.exe", url);
                //打开
                if (result1 == null) Process.Start(url);
            }
        }
        catch (Exception ex)
        {
            //用IE打开
            OpenIeBrowserUrl(url);
        }
    }

    public static void OpenFireFoxBrowserUrl(string url)
    {
        try
        {
            //注册表路径
            var openKey = IsWin32()
                ? @"SOFTWARE\Mozilla\Mozilla Firefox"
                : @"SOFTWARE\Wow6432Node\Mozilla\Mozilla Firefox";
            //读取
            var appPath = Registry.LocalMachine.OpenSubKey(openKey);
            if (appPath != null)
            {
                //打开火狐
                var result = Process.Start("firefox.exe", url);
                //打开IE
                if (result == null) OpenIeBrowserUrl(url);
            }
            else
            {
                //打开火狐
                var result = Process.Start("firefox.exe", url);
                //打开默认浏览器
                if (result == null) OpenDefaultBrowserUrl(url);
            }
        }
        catch (Exception ex)
        {
            //打开默认浏览器
            OpenDefaultBrowserUrl(url);
        }
    }

    //是否是32位
    private static bool IsWin32() => IntPtr.Size == 4;
}