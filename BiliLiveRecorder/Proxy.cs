using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace BiliLiveRecorder
{
    /// <summary>
    /// 从代理池获取代理
    /// </summary>
    class Proxy
    {
        /// <summary>
        /// 获取中国代理IP的接口
        /// </summary>
        private const string URL_ProxyAPI = "https://ip.jiangxianli.com/api/proxy_ips?order_by=speed&order_rule=ASC";
        /// <summary>
        /// 代理IP-端口号列表
        /// </summary>
        private List<string[]> ProxyList = new List<string[]>();
        /// <summary>
        /// 总页数
        /// </summary>
        private int PageCount = 0;
        /// <summary>
        /// 当前页数
        /// </summary>
        private int PageNow = 1;
        /// <summary>
        /// 当前选择的代理条目索引
        /// </summary>
        private int IndexNow = 0;
        /// <summary>
        /// 所有代理获取结束
        /// </summary>
        public bool End = false;
        /// <summary>
        /// 获取第一页代理列表
        /// </summary>
        public Proxy()
        {
            PageCount = GetPageProxy(PageNow);
            if (PageCount == 0)
            {
                End = true;
            }
        }
        /// <summary>
        /// 从代理列表中获取一个代理
        /// </summary>
        /// <returns></returns>
        public WebProxy GetOne()
        {
            // 本页结束
            if (IndexNow >= ProxyList.Count)
            {
                // 到达尾页
                if (PageNow >= PageCount)
                {
                    End = true;
                    return null;
                }
                else
                {
                    Thread.Sleep(500);
                    PageCount = GetPageProxy(++PageNow);
                    IndexNow = 0;
                    if (PageNow > PageCount)
                    {
                        End = true;
                        return null;
                    }
                }
            }
            WebProxy webProxy = new WebProxy(ProxyList[IndexNow][0], int.Parse(ProxyList[IndexNow][1]));
            ++IndexNow;
            return webProxy;
        }
        /// <summary>
        /// 获取指定页的代理列表
        /// </summary>
        /// <param name="Page">页码</param>
        /// <returns>总页数</returns>
        private int GetPageProxy(int Page)
        {
            if (Page != 1 && Page > PageCount)
            {
                return PageCount;
            }
            else
            {
                try
                {
                    WebRequest request = WebRequest.Create(URL_ProxyAPI + "&page=" + Page.ToString());
                    request.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
                    WebResponse response = request.GetResponse();
                    Stream s = response.GetResponseStream();
                    StreamReader sr = new StreamReader(s);
                    string str = sr.ReadToEnd();
                    sr.Dispose();
                    sr.Close();
                    s.Dispose();
                    s.Close();
                    response.Dispose();
                    response.Close();
                    // 总页数
                    int totalPage;
                    // 没有可用代理
                    if (str.IndexOf("\"total\":0") != -1)
                    {
                        return 0;
                    }
                    else
                    {
                        string total = str.Substring(str.IndexOf("last_page\":") + 11);
                        total = total.Substring(0, total.IndexOf(','));
                        totalPage = int.Parse(total);
                    }
                    ProxyList.Clear();
                    // 写入代理列表
                    str = str.Substring(str.IndexOf("\"ip\":\"") + 6);
                    string[] list = str.Split(new string[] { "\"ip\":\"" },StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in list)
                    {
                        string IP = item.Substring(0, item.IndexOf('"'));
                        string Port = item.Substring(item.IndexOf("port\":\"") + 7);
                        Port = Port.Substring(0, Port.IndexOf('"'));
                        ProxyList.Add(new string[] { IP, Port });
                    }
                    return totalPage;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
    }
}
