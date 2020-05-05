﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;


namespace HangfireLAB.Web.Models
{
    [Hangfire.Dashboard.Management.Metadata.ManagementPage("排程任務", "default")]
    public class MyJob
    {
        public static void 即時任務(PerformContext context)
        {
            context.WriteLine($"執行即時任務:{DateTime.Now:hh:mm}");
        }
        
        [Hangfire.Dashboard.Management.Support.Job]
        [DisplayName("呼叫內部方法")]
        public void Action(PerformContext context = null, IJobCancellationToken cancellationToken = null)
        {
            if (cancellationToken.ShutdownToken.IsCancellationRequested)
            {
                return;
            }
 
            context.WriteLine($"測試用，Now:{DateTime.Now}");
            Thread.Sleep(30000);
        }
        
        [Queue("default")]
        [Hangfire.Dashboard.Management.Support.Job]
        [DisplayName("请求连接")]
        [Description("请求外部连接,除非指定任务")]
        [AutomaticRetry(Attempts = 3)]//自动重试
        [DisableConcurrentExecution(90)]//禁用并行执行
        public string SendRequest(
            [Hangfire.Dashboard.Management.Metadata.DisplayData("请求链接","http://localhost:8080/test.html?name=youname","请求外部连接,必须http或者https开头")]
            Uri url,
            [Hangfire.Dashboard.Management.Metadata.DisplayData("请求方式","POST","常用的请求方式有 GET、POST、PUT、DELETE 等")]
            HttpMethod method = HttpMethod.POST,
            [Hangfire.Dashboard.Management.Metadata.DisplayData("请求内容",IsMultiLine =true)]
            string content = null,
            [Hangfire.Dashboard.Management.Metadata.DisplayData("请求协议","application/x-www-form-urlencoded","常见的有 application/x-www-form-urlencoded、multipart/form-data、text/plain、application/json 等","application/x-www-form-urlencoded")]
            string contentType = "application/x-www-form-urlencoded",
            [Hangfire.Dashboard.Management.Metadata.DisplayData("提交内容的编码方式","utf-8","常见的有 UTF-8、Unicode、ASCII，错误的编码会导致内容无法识别","utf-8",ConvertType=typeof(EncodingsInputDataList))]
            string contentEncoding = "UTF-8",
            [Hangfire.Dashboard.Management.Metadata.DisplayData("返回结果内容的编码方式","utf-8","常见的有 UTF-8、Unicode、ASCII，错误的编码会导致内容无法识别","utf-8",ConvertType=typeof(EncodingsInputDataList))]
            string responseEncoding = "UTF-8",
            PerformContext context = null)
        {
            var _contentEncoding = Encoding.GetEncoding(contentEncoding) ?? Encoding.UTF8;
            var _responseEncoding = Encoding.GetEncoding(responseEncoding) ?? Encoding.UTF8;
            var r = SendData(url, method, content, contentType, _contentEncoding, _responseEncoding);

            return r;
        }
        
        /// <summary>  
        /// 指定Post地址使用Get 方式获取全部字符串  
        /// </summary>  
        /// <param name="url">请求后台地址</param>  
        /// <param name="content">Post提交数据内容(utf-8编码的)</param>  
        /// <returns>结果</returns>  
        private string SendData(Uri url, HttpMethod method = HttpMethod.POST, string content = "", string contentType = "application/x-www-form-urlencoded", Encoding contentEncoding = null, Encoding responseEncoding = null)
        {
            //申明一个容器result接收数据
            string result = "";
            //首先创建一个HttpWebRequest,申明传输方式POST
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = method.ToString();
            req.ContentType = contentType;

            if (!string.IsNullOrWhiteSpace(content) && method != HttpMethod.GET)
            {
                //添加POST参数
                byte[] data = (contentEncoding ?? Encoding.UTF8).GetBytes(content);
                req.ContentLength = data.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                }
            }
            //申明一个容器resp接收返回数据
            var resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容  
            using (StreamReader reader = new StreamReader(stream, (responseEncoding ?? Encoding.UTF8)))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
    }
    
    public enum HttpMethod
    {
        GET, POST, PUT, HEAD, TRACE, DELETE, SEARCH, CONNECT, PROPFIND, PROPPATCH, PATCH, MKCOL, COPY, MOVE, LOCK, UNLOCK, OPTIONS
    }
    
    public class EncodingsInputDataList : Hangfire.Dashboard.Management.Metadata.IInputDataList
    {
        public System.Collections.Generic.Dictionary<string, string> GetData()
        {
            return System.Text.Encoding.GetEncodings().GroupBy(f => f.Name, (f1, f2) => f2.FirstOrDefault()).ToDictionary(f => f.Name, f => f.DisplayName);
        }

        public string GetDefaultValue()
        {
            return null;
        }
    }
}