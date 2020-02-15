using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;

namespace BiliLiveRecorder
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 根据昵称获取UID
        /// </summary>
        /// <param name="NickName">用户昵称</param>
        /// <returns>UID</returns>
        private int GetUID(string NickName)
        {
            WebRequest request = WebRequest.Create("https://api.bilibili.com/x/web-interface/search/type?page=1&search_type=bili_user&changing=mid&__refresh__=true&__reload__=false&highlight=1&single_column=0&jsonp=jsonp&keyword=" + NickName);
            WebResponse response = request.GetResponse();
            Stream s = response.GetResponseStream();
            StreamReader sr = new StreamReader(s);
            // Json字符串
            string str = sr.ReadToEnd();
            sr.Dispose();
            sr.Close();
            s.Dispose();
            s.Close();
            response.Dispose();
            response.Close();
            // 第一个符合搜索条件的UID字符串首下标
            int pos = str.IndexOf("\"mid\":");
            // 没有搜索到符合条件的用户
            if (pos == -1)
            {
                return -1;
            }
            else
            {
                // 第一个符合搜索条件的UID字符串开头
                str = str.Substring(pos + 6);
                // UID字符串
                str = str.Substring(0, str.IndexOfAny(new char[] { ',', '}' }));
                return int.Parse(str);
            }
        }
        /// <summary>
        /// 根据UID获取用户信息
        /// </summary>
        /// <param name="UID">用户UID</param>
        /// <returns>用户信息对象</returns>
        private UserInfo GetUserInfo(string UID)
        {
            UserInfo userInfo = new UserInfo();
            WebRequest request = WebRequest.Create("https://api.bilibili.com/x/space/acc/info?mid=" + UID);
            WebResponse response = request.GetResponse();
            Stream s = response.GetResponseStream();
            StreamReader sr = new StreamReader(s);
            // Json字符串
            string str = sr.ReadToEnd();
            sr.Dispose();
            sr.Close();
            s.Dispose();
            s.Close();
            response.Dispose();
            response.Close();
            // 分离的用户信息
            string[] items = str.Split(new char[] { ',', '{', '}' }, System.StringSplitOptions.RemoveEmptyEntries);
            // UID无效
            if (items[0].Equals("\"code\":0") == false)
            {
                userInfo.Valid = false;
            }
            // UID有效
            else
            {
                userInfo.Valid = true;
                userInfo.UID = int.Parse(items[4].Substring(6));
                userInfo.Name = items[5].Substring(7).Trim(new char[] { '"' });
                userInfo.Sex = items[6].Substring(6).Trim(new char[] { '"' });
                userInfo.FaceURL = items[7].Substring(7).Trim(new char[] { '"' });
                userInfo.Sign = items[8].Substring(7).Trim(new char[] { '"' }).Replace("\\n", "\n");
                userInfo.Level = int.Parse(items[10].Substring(8));
                userInfo.Birthday = items[14].Substring(11).Trim(new char[] { '"' });
            }
            return userInfo;
        }
        /// <summary>
        /// 单击锁定监视按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 目标主播UID
            string targetUID = string.Empty;
            // 根据昵称查找
            if (SelectMode.SelectedIndex == 0)
            {
                int tmpUID = GetUID(SearchText.Text);
                // 查找失败
                if (tmpUID == -1)
                {
                    MessageBox.Show("昵称无效, 请确认后输入！");
                    return;
                }
                else
                {
                    targetUID = tmpUID.ToString();
                }
            }
            // 根据UID查找
            else
            {
                targetUID = SearchText.Text;
            }
            UserInfo userInfo = GetUserInfo(targetUID);
            // 用户有效, 启动监视窗口
            if (userInfo.Valid == true)
            {
                LiveMonitor liveMonitor = new LiveMonitor(userInfo);
                liveMonitor.Show();
            }
            else
            {
                MessageBox.Show("UID无效, 请确认后输入！");
            }
        }
        /// <summary>
        /// 关闭程序前确认所有监视都已关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (LiveMonitor.MonitorList.Count != 0)
            {
                if (MessageBox.Show("当前有未结束的监视/录制任务, 是否直接结束程序？", "检测到任务", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    foreach (var wnd in LiveMonitor.MonitorList)
                    {
                        wnd.CloseMonitor();
                    }
                    Application.Current.Shutdown();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
