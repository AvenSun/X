﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using NewLife.Log;
using System.Reflection;

namespace XAgent
{
    /// <summary>
    /// 服务程序基类
    /// </summary>
    /// <typeparam name="ServiceType">服务类型</typeparam>
    public abstract class AgentServiceBase<ServiceType> : ServiceBase
         where ServiceType : AgentServiceBase<ServiceType>, new()
    {
        #region 属性
        /// <summary>描述</summary>
        public virtual String Description { get { return AgentServiceName + "服务"; } }

        /// <summary>
        /// 线程数
        /// </summary>
        public virtual Int32 ThreadCount { get { return 1; } }

        /// <summary>
        /// 线程名
        /// </summary>
        public virtual String[] ThreadNames { get { return null; } }

        /// <summary>
        /// Exe程序名
        /// </summary>
        private static String ExeName
        {
            get
            {
                //String filename= AppDomain.CurrentDomain.FriendlyName.Replace(".vshost.", ".");
                //if (filename.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) return filename;

                //filename = Assembly.GetExecutingAssembly().Location;
                //return filename;
                //String filename = Assembly.GetEntryAssembly().Location;
                Process p = Process.GetCurrentProcess();
                String filename = p.MainModule.FileName;
                filename = Path.GetFileName(filename);
                filename = filename.Replace(".vshost.", ".");
                return filename;
            }
        }

        /// <summary>
        /// 服务实例，以方便调用基类的重载
        /// </summary>
        protected static ServiceType Instance = new ServiceType();

        private static ServiceController _Controller;
        /// <summary>控制器</summary>
        protected static ServiceController Controller
        {
            get
            {
                if (_Controller == null)
                {
                    try
                    {
                        ServiceController control = GetService(AgentServiceName);
                        if (control != null)
                        {
                            try
                            {
                                //尝试访问一下才知道是否已安装
                                Boolean b = control.CanShutdown;
                                _Controller = control;
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
                return _Controller;
            }
            set
            {
                _Controller = null;
            }
        }
        #endregion

        #region 服务安装和启动
        /// <summary>
        /// 服务名
        /// </summary>
        public static String AgentServiceName { get { return Instance.ServiceName; } }

        /// <summary>
        /// 服务描述
        /// </summary>
        public static String AgentDescription { get { return Instance.Description; } }

        /// <summary>
        /// 安装、卸载 服务
        /// </summary>
        /// <param name="isinstall">是否安装</param>
        public static void Install(Boolean isinstall)
        {
            String filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ExeName);
            if (isinstall)
                InstallService(AgentServiceName, filename, AgentDescription);
            else
                RunSC("Delete " + AgentServiceName);
        }

        /// <summary>
        /// 安装服务
        /// </summary>
        /// <param name="name"></param>
        /// <param name="filename"></param>
        /// <param name="description"></param>
        public static void InstallService(String name, String filename, String description)
        {
            RunSC("create " + name + " BinPath= \"" + filename + " -s\" start= auto DisplayName= " + description);
            RunSC("description " + name + " " + description);
        }

        /// <summary>
        /// 卸载服务
        /// </summary>
        /// <param name="name"></param>
        public static void UnInstalService(String name)
        {
            RunSC("Delete " + name);
        }

        /// <summary>
        /// 启动、停止 服务
        /// </summary>
        /// <param name="isstart"></param>
        public static void ControlService(Boolean isstart)
        {
            if (isstart)
                RunCmd("net start " + AgentServiceName, false, true);
            else
                RunCmd("net stop " + AgentServiceName, false, true);
        }

        /// <summary>
        /// 执行一个命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="showWindow"></param>
        /// <param name="waitForExit"></param>
        protected static void RunCmd(String cmd, Boolean showWindow, Boolean waitForExit)
        {
            Process p = new Process();
            ProcessStartInfo si = new ProcessStartInfo();
            String path = Environment.SystemDirectory;
            path = Path.Combine(path, @"cmd.exe");
            si.FileName = path;
            if (!cmd.StartsWith(@"/")) cmd = @"/c " + cmd;
            si.Arguments = cmd;
            si.UseShellExecute = false;
            si.CreateNoWindow = !showWindow;
            si.RedirectStandardOutput = true;
            si.RedirectStandardError = true;
            p.StartInfo = si;

            p.Start();
            if (waitForExit)
            {
                p.WaitForExit();

                String str = p.StandardOutput.ReadToEnd();
                if (!String.IsNullOrEmpty(str)) WriteLine(str.Trim(new Char[] { '\r', '\n', '\t' }).Trim());
                str = p.StandardError.ReadToEnd();
                if (!String.IsNullOrEmpty(str)) WriteLine(str.Trim(new Char[] { '\r', '\n', '\t' }).Trim());
            }
        }

        /// <summary>
        /// 延迟执行命令
        /// </summary>
        /// <param name="cmd">要执行的命令</param>
        /// <param name="cmd2">延时后执行的命令</param>
        /// <param name="delay">延时时间（单位：秒）</param>
        protected static void RunCmd(String cmd, String cmd2, Int32 delay)
        {
            //在临时目录生成重启服务的批处理文件
            String filename = Path.GetTempFileName() + ".bat";
            if (File.Exists(filename)) File.Delete(filename);

            File.AppendAllText(filename, cmd);
            File.AppendAllText(filename, Environment.NewLine);
            File.AppendAllText(filename, "ping 127.0.0.1 -n " + delay + " > nul ");
            File.AppendAllText(filename, Environment.NewLine);
            File.AppendAllText(filename, cmd2);

            Process p = new Process();
            ProcessStartInfo si = new ProcessStartInfo();
            si.FileName = filename;
            si.UseShellExecute = true;
            si.CreateNoWindow = true;
            p.StartInfo = si;

            p.Start();

            File.Delete(filename);
        }

        /// <summary>
        /// 执行SC命令
        /// </summary>
        /// <param name="cmd"></param>
        protected static void RunSC(String cmd)
        {
            String path = Environment.SystemDirectory;
            path = Path.Combine(path, @"sc.exe");
            if (!File.Exists(path)) path = "sc.exe";
            if (!File.Exists(path)) return;
            RunCmd(path + " " + cmd, false, true);
        }

        /// <summary>
        /// 是否已安装
        /// </summary>
        public static Boolean? IsInstalled
        {
            get
            {
                try
                {
                    ServiceController control = GetService(AgentServiceName);
                    if (control == null) return false;
                    try
                    {
                        //尝试访问一下才知道是否已安装
                        Boolean b = control.CanShutdown;
                        return true;
                    }
                    catch { return false; }
                }
                catch { return null; }

                //return Controller != null;
            }
        }

        /// <summary>
        /// 是否已启动
        /// </summary>
        public static Boolean? IsRunning
        {
            get
            {
                try
                {
                    //ServiceController control = GetService(AgentServiceName);
                    ServiceController control = Controller;
                    if (control == null) return null;
                    control.Refresh();
                    if (control.Status == ServiceControllerStatus.Running) return true;
                    if (control.Status == ServiceControllerStatus.Stopped) return false;
                    return null;
                }
                catch { return null; }
            }
        }
        #endregion

        #region 静态辅助函数
        /// <summary>
        /// 生成批处理
        /// </summary>
        protected virtual void MakeBat()
        {
            //if (!File.Exists("安装.bat")) File.WriteAllText("安装.bat", AppDomain.CurrentDomain.FriendlyName.Replace(".vshost.", ".") + " -i");
            //if (!File.Exists("卸载.bat")) File.WriteAllText("卸载.bat", AppDomain.CurrentDomain.FriendlyName.Replace(".vshost.", ".") + " -u");
            //if (!File.Exists("启动.bat")) File.WriteAllText("启动.bat", AppDomain.CurrentDomain.FriendlyName.Replace(".vshost.", ".") + " -start");
            //if (!File.Exists("停止.bat")) File.WriteAllText("停止.bat", AppDomain.CurrentDomain.FriendlyName.Replace(".vshost.", ".") + " -stop");
            File.WriteAllText("安装.bat", ExeName + " -i");
            File.WriteAllText("卸载.bat", ExeName + " -u");
            File.WriteAllText("启动.bat", ExeName + " -start");
            File.WriteAllText("停止.bat", ExeName + " -stop");
        }

        /// <summary>
        /// 服务主函数
        /// </summary>
        public static void ServiceMain()
        {
            //提升进程优先级
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            Instance.MakeBat();

            String[] Args = Environment.GetCommandLineArgs();

            if (Args.Length > 1)
            {
                #region 命令
                if (Args[1].ToLower() == "-s")  //启动服务
                {
                    ServiceBase[] ServicesToRun = new ServiceBase[] { new ServiceType() };
                    try
                    {
                        ServiceBase.Run(ServicesToRun);
                    }
                    catch (Exception ex)
                    {
                        XTrace.WriteLine(ex.ToString());
                    }
                    return;
                }
                else if (Args[1].ToLower() == "-i") //安装服务
                {
                    Install(true);
                    return;
                }
                else if (Args[1].ToLower() == "-u") //卸载服务
                {
                    Install(false);
                    return;
                }
                else if (Args[1].ToLower() == "-start") //启动服务
                {
                    ControlService(true);
                    return;
                }
                else if (Args[1].ToLower() == "-stop") //停止服务
                {
                    ControlService(false);
                    return;
                }
                else if (Args[1].ToLower() == "-run") //循环执行任务
                {
                    ServiceType service = new ServiceType();
                    service.StartWork();
                    Console.ReadKey();
                    return;
                }
                else if (Args[1].ToLower() == "-step") //单步执行任务
                {
                    ServiceType service = new ServiceType();
                    for (int i = 0; i < service.ThreadCount; i++)
                    {
                        service.Work(i);
                    }
                    return;
                }
                #endregion
            }
            else
            {
                #region 命令行
                XTrace.OnWriteLog += new EventHandler<WriteLogEventArgs>(XTrace_OnWriteLog);

                while (true)
                {
                    //输出菜单
                    Instance.ShowMenu();
                    Console.Write("请选择操作：");

                    //读取命令
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.KeyChar == '0') break;
                    Console.WriteLine();
                    Console.WriteLine();

                    switch (key.KeyChar)
                    {
                        case '1':
                            //输出状态
                            Instance.ShowStatus();

                            break;
                        case '2':
                            if (IsInstalled == true)
                                Install(false);
                            else
                                Install(true);
                            break;
                        case '3':
                            if (IsRunning == true)
                                ControlService(false);
                            else
                                ControlService(true);
                            break;
                        case '4':
                            #region 单步调试
                            try
                            {
                                Int32 n = 0;
                                if (Instance.ThreadCount > 1)
                                {
                                    Console.Write("请输入要调试的任务（任务数：{0}）：", Instance.ThreadCount);
                                    ConsoleKeyInfo k = Console.ReadKey();
                                    Console.WriteLine();
                                    n = k.KeyChar - '0';
                                }

                                Console.WriteLine("正在单步调试……");
                                if (n < 0 || n > Instance.ThreadCount - 1)
                                {
                                    for (int i = 0; i < Instance.ThreadCount; i++)
                                    {
                                        Instance.Work(i);
                                    }
                                }
                                else
                                {
                                    Instance.Work(n);
                                }
                                Console.WriteLine("单步调试完成！");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                            #endregion
                            break;
                        case '5':
                            #region 循环调试
                            try
                            {
                                Console.WriteLine("正在循环调试……");
                                Instance.StartWork();

                                Console.WriteLine("任意键结束循环调试！");
                                Console.ReadKey();

                                Instance.StopWork();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                            #endregion
                            break;
                        default:
                            break;
                    }
                }
                #endregion
            }
        }

        static void XTrace_OnWriteLog(object sender, WriteLogEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        /// <summary>
        /// 显示状态
        /// </summary>
        protected virtual void ShowStatus()
        {
            Console.WriteLine("服务：{0}", AgentServiceName);
            Console.WriteLine("描述：{0}", AgentDescription);
            Console.Write("状态：");
            if (IsInstalled == null)
                Console.WriteLine("未知");
            else if (IsInstalled == false)
                Console.WriteLine("未安装");
            else
            {
                if (IsRunning == null)
                    Console.WriteLine("未知");
                else
                {
                    if (IsRunning == false)
                        Console.WriteLine("未启动");
                    else
                        Console.WriteLine("运行中");
                }
            }
            Console.WriteLine("程序：{0}", Version);
            Console.WriteLine("文件：{0}", FileVersion);
            Console.WriteLine("发布：{0}", Compile.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private static String _Version;
        /// <summary>
        /// 程序集版本
        /// </summary>
        public static String Version
        {
            get
            {
                if (String.IsNullOrEmpty(_Version))
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    _Version = asm.GetName().Version.ToString();
                    if (String.IsNullOrEmpty(_Version)) _Version = "1.0";
                }
                return _Version;
            }
        }

        private static String _FileVersion;
        /// <summary>
        /// 文件版本
        /// </summary>
        public static String FileVersion
        {
            get
            {
                if (String.IsNullOrEmpty(_FileVersion))
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    AssemblyFileVersionAttribute av = Attribute.GetCustomAttribute(asm, typeof(AssemblyFileVersionAttribute)) as AssemblyFileVersionAttribute;
                    if (av != null) _FileVersion = av.Version;
                    if (String.IsNullOrEmpty(_FileVersion)) _FileVersion = "1.0";
                }
                return _FileVersion;
            }
        }

        private static DateTime _Compile;
        /// <summary>编译</summary>
        public static DateTime Compile
        {
            get
            {
                if (_Compile <= DateTime.MinValue)
                {
                    String[] ss = Version.Split(new Char[] { '.' });
                    Int32 d = Convert.ToInt32(ss[2]);
                    Int32 s = Convert.ToInt32(ss[3]);

                    DateTime dt = new DateTime(2000, 1, 1);
                    dt = dt.AddDays(d).AddSeconds(s * 2);

                    _Compile = dt;
                }
                return _Compile;
            }
        }
        /// <summary>
        /// 显示菜单
        /// </summary>
        protected virtual void ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine("1 显示状态");

            if (IsInstalled == true)
            {
                if (IsRunning == true)
                {
                    Console.WriteLine("3 停止服务");
                }
                else
                {
                    Console.WriteLine("2 卸载服务");

                    Console.WriteLine("3 启动服务");
                }
            }
            else
            {
                Console.WriteLine("2 安装服务");
            }

            if (IsRunning != true)
            {
                Console.WriteLine("4 单步调试");
                Console.WriteLine("5 循环调试");
            }

            Console.WriteLine("0 退出");
        }

        /// <summary>
        /// 取得服务
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ServiceController GetService(String name)
        {
            List<ServiceController> list = new List<ServiceController>(ServiceController.GetServices());
            if (list == null || list.Count < 1) return null;

            return list.Find(delegate(ServiceController item) { return item.ServiceName == name; });
        }
        #endregion

        #region 服务控制
        private Thread[] _Threads;
        /// <summary>线程组</summary>
        private Thread[] Threads
        {
            get
            {
                if (_Threads == null) _Threads = new Thread[ThreadCount];
                return _Threads;
            }
            set { _Threads = value; }
        }

        /// <summary>
        /// 服务启动事件
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            StartWork();
        }

        /// <summary>
        /// 服务停止事件
        /// </summary>
        protected override void OnStop()
        {
            StopWork();
        }

        /// <summary>
        /// 开始循环工作
        /// </summary>
        public virtual void StartWork()
        {
            WriteLine("服务启动");

            try
            {
                for (int i = 0; i < ThreadCount; i++)
                {
                    StartWork(i);
                }

                //启动服务管理线程
                StartManagerThread();
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// 开始循环工作
        /// </summary>
        /// <param name="index">线程序号</param>
        public virtual void StartWork(Int32 index)
        {
            if (index < 0 || index >= ThreadCount) throw new ArgumentOutOfRangeException("index");

            Threads[index] = new Thread(workWaper);
            String name = "服务工作线程" + index;
            if (ThreadNames != null && ThreadNames.Length > index && !String.IsNullOrEmpty(ThreadNames[index]))
                name = ThreadNames[index];
            Threads[index].Name = name;
            Threads[index].IsBackground = true;
            Threads[index].Priority = ThreadPriority.AboveNormal;
            Threads[index].Start(index);
        }

        /// <summary>
        /// 是否有数据库连接错误。只是为了使得数据库连接出错时少报错误日志
        /// </summary>
        Boolean hasdberr = false;

        /// <summary>
        /// 线程包装
        /// </summary>
        /// <param name="data">线程序号</param>
        private void workWaper(Object data)
        {
            Int32 index = (Int32)data;

            while (true)
            {
                Boolean isContinute = false;
                Active[index] = DateTime.Now;

                try
                {
                    isContinute = Work(index);

                    hasdberr = false;
                }
                catch (ThreadAbortException) //线程被取消
                {
                    WriteLine("线程" + index + "被取消！");
                    break;
                }
                catch (ThreadInterruptedException) //线程中断错误
                {
                    WriteLine("线程" + index + "中断错误！");
                    break;
                }
                catch (DbException ex)
                {
                    //确保只报一次数据库错误
                    if (!hasdberr)
                    {
                        hasdberr = true;
                        WriteLine(ex.ToString());
                    }
                }
                catch (Exception ex) //确保拦截了所有的异常，保证服务稳定运行
                {
                    WriteLine(ex.ToString());
                }
                Active[index] = DateTime.Now;

                //检查服务是否正在重启
                if (IsShutdowning)
                {
                    WriteLine("服务准备重启，" + Thread.CurrentThread.Name + "退出！");
                    break;
                }

                Int32 time = Interval;
                //使用专用的时间间隔
                if (index < Intervals.Length) time = Intervals[index];

                //如果有数据库连接错误，则将等待间隔放大十倍
                if (hasdberr) time *= 10;

                if (!isContinute) Thread.Sleep(time * 1000);
            }
        }

        /// <summary>
        /// 核心工作方法。调度线程会定期调用该方法
        /// </summary>
        /// <param name="index">线程序号</param>
        /// <returns>是否立即开始下一步工作。某些任务能达到满负荷，线程可以不做等待</returns>
        public abstract Boolean Work(Int32 index);

        /// <summary>
        /// 停止循环工作。
        /// 只能停止循环而已，如果已经有一批任务在处理，
        /// 则内部需要捕获ThreadAbortException异常，否则无法停止任务处理。
        /// </summary>
        public virtual void StopWork()
        {
            WriteLine("服务停止");

            //停止服务管理线程
            StopManagerThread();

            //if (threads != null && threads.IsAlive) threads.Abort();
            if (Threads != null)
            {
                foreach (Thread item in Threads)
                {
                    try
                    {
                        if (item != null && item.IsAlive) item.Abort();
                    }
                    catch (Exception ex)
                    {
                        WriteLine(ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 停止循环工作
        /// </summary>
        /// <param name="index">线程序号</param>
        public virtual void StopWork(Int32 index)
        {
            if (index < 0 || index >= ThreadCount) throw new ArgumentOutOfRangeException("index");

            Thread item = Threads[index];
            try
            {
                if (item != null && item.IsAlive) item.Abort();
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
            }
        }
        #endregion

        #region 服务维护线程
        /// <summary>
        /// 服务管理线程
        /// </summary>
        private Thread ManagerThread;

        /// <summary>
        /// 开始服务管理线程
        /// </summary>
        public void StartManagerThread()
        {
            ManagerThread = new Thread(ManagerThreadWaper);
            ManagerThread.Name = "服务管理线程";
            ManagerThread.IsBackground = true;
            ManagerThread.Priority = ThreadPriority.Highest;
            ManagerThread.Start();
        }

        /// <summary>
        /// 停止服务管理线程
        /// </summary>
        public void StopManagerThread()
        {
            if (ManagerThread == null) return;
            if (ManagerThread.IsAlive)
            {
                try
                {
                    ManagerThread.Abort();
                }
                catch (Exception ex)
                {
                    WriteLine(ex.ToString());
                }
            }
        }

        /// <summary>
        /// 服务管理线程封装
        /// </summary>
        /// <param name="data"></param>
        protected virtual void ManagerThreadWaper(Object data)
        {
            while (true)
            {
                try
                {
                    CheckActive();

                    //如果某一项检查需要重启服务，则返回true，这里跳出循环，等待服务重启
                    if (CheckMemory()) break;
                    if (CheckThread()) break;
                    if (CheckAutoRestart()) break;

                    Thread.Sleep(60 * 1000);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    WriteLine(ex.ToString());
                }
            }
        }

        private DateTime[] _Active;
        /// <summary>活动时间</summary>
        public DateTime[] Active
        {
            get
            {
                if (_Active == null)
                {
                    _Active = new DateTime[ThreadCount];
                    for (int i = 0; i < ThreadCount; i++)
                    {
                        _Active[i] = DateTime.Now;
                    }
                }
                return _Active;
            }
            set { _Active = value; }
        }

        /// <summary>
        /// 检查是否有工作线程死亡
        /// </summary>
        protected virtual void CheckActive()
        {
            if (Threads == null || Threads.Length < 1) return;

            //检查已经停止了的工作线程
            for (int i = 0; i < ThreadCount; i++)
            {
                if (Threads[i] != null && !Threads[i].IsAlive)
                {
                    WriteLine(Threads[i].Name + "处于停止状态，准备重新启动！");

                    StartWork(i);
                }
            }

            //是否检查最大活动时间
            if (MaxActive <= 0) return;

            for (int i = 0; i < ThreadCount; i++)
            {
                TimeSpan ts = DateTime.Now - Active[i];
                if (ts.TotalSeconds > MaxActive)
                {
                    WriteLine(Threads[i].Name + "已经" + ts.TotalSeconds + "秒没有活动了，准备重新启动！");

                    StopWork(i);
                    //等待线程结束
                    Threads[i].Join(5000);
                    StartWork(i);
                }
            }
        }

        /// <summary>
        /// 检查内存是否超标
        /// </summary>
        /// <returns>是否超标重启</returns>
        protected virtual Boolean CheckMemory()
        {
            if (MaxMemory <= 0) return false;

            Process p = Process.GetCurrentProcess();
            long cur = p.WorkingSet64 + p.PrivateMemorySize64;
            cur = cur / 1024 / 1024;
            if (cur > MaxMemory)
            {
                WriteLine("当前进程占用内存" + cur + "M，超过阀值" + MaxMemory + "M，准备重新启动！");

                RestartService();

                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查服务进程的总线程数是否超标
        /// </summary>
        /// <returns></returns>
        protected virtual Boolean CheckThread()
        {
            if (MaxThread <= 0) return false;

            Process p = Process.GetCurrentProcess();
            if (p.Threads.Count > MaxThread)
            {
                WriteLine("当前进程总线程" + p.Threads.Count + "个，超过阀值" + MaxThread + "个，准备重新启动！");

                RestartService();

                return true;
            }

            return false;
        }

        /// <summary>
        /// 服务开始时间
        /// </summary>
        private DateTime Start = DateTime.Now;

        /// <summary>
        /// 检查自动重启
        /// </summary>
        /// <returns></returns>
        protected virtual Boolean CheckAutoRestart()
        {
            if (AutoRestart <= 0) return false;

            TimeSpan ts = DateTime.Now - Start;
            if (ts.TotalMinutes > AutoRestart)
            {
                WriteLine("服务已运行" + ts.TotalMinutes + "分钟，达到预设重启时间（" + AutoRestart + "分钟），准备重启！");

                RestartService();

                return true;
            }

            return false;
        }

        /// <summary>
        /// 是否正在重启
        /// </summary>
        private Boolean IsShutdowning = false;

        /// <summary>
        /// 重启服务
        /// </summary>
        protected void RestartService()
        {
            WriteLine("重启服务！");

            //在临时目录生成重启服务的批处理文件
            String filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "重启.bat");
            if (File.Exists(filename)) File.Delete(filename);

            File.AppendAllText(filename, "net stop " + AgentServiceName);
            File.AppendAllText(filename, Environment.NewLine);
            File.AppendAllText(filename, "ping 127.0.0.1 -n 5 > nul ");
            File.AppendAllText(filename, Environment.NewLine);
            File.AppendAllText(filename, "net start " + AgentServiceName);

            //准备重启服务，等待所有工作线程返回
            IsShutdowning = true;
            for (int i = 0; i < 10; i++)
            {
                Boolean b = false;
                foreach (Thread item in Threads)
                {
                    if (item.IsAlive)
                    {
                        b = true;
                        break;
                    }
                }
                if (!b) break;
                Thread.Sleep(1000);
            }

            //执行重启服务的批处理
            //RunCmd(filename, false, false);
            Process p = new Process();
            ProcessStartInfo si = new ProcessStartInfo();
            si.FileName = filename;
            si.UseShellExecute = true;
            si.CreateNoWindow = true;
            p.StartInfo = si;

            p.Start();

            //if (File.Exists(filename)) File.Delete(filename);
        }
        #endregion

        #region 辅助函数及属性

        void Service_OnWriteLog(object sender, WriteLogEventArgs e)
        {
            if (XTrace.Debug) XTrace.WriteLine(e.Message);
        }

        /// <summary>
        /// 间隔。默认60秒
        /// </summary>
        public static Int32 Interval
        {
            get
            {
                if (Intervals == null || Intervals.Length < 1)
                    return 60;
                else
                    return Intervals[0];
            }
        }

        private static Int32[] _Intervals;
        /// <summary>
        /// 间隔数组。默认60秒
        /// </summary>
        public static Int32[] Intervals
        {
            get
            {
                if (_Intervals != null) return _Intervals;

                String str = ConfigurationManager.AppSettings["Interval"];
                if (String.IsNullOrEmpty(str))
                {
                    _Intervals = new Int32[] { 60 };
                }
                else
                {
                    String[] ss = str.Split(new Char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    List<Int32> list = new List<Int32>(ss.Length);
                    foreach (String item in ss)
                    {
                        str = item.Trim();
                        if (String.IsNullOrEmpty(str)) continue;

                        Int32 result = 60;
                        if (!Int32.TryParse(str, out result)) result = 60;
                        if (result <= 0) result = 60;
                        list.Add(result);
                    }
                    _Intervals = list.ToArray();
                }
                return _Intervals;
            }
            set { _Intervals = value; }
        }

        private static Int32? _MaxActive;
        /// <summary>
        /// 最大活动时间。超过最大活动时间都还没有响应的线程将会被重启，防止线程执行时间过长。默认0秒，表示无限
        /// </summary>
        public static Int32 MaxActive
        {
            get
            {
                if (_MaxActive == null) _MaxActive = GetConfigInt32("MaxActive", 0);
                return _MaxActive.Value;
            }
            set { _MaxActive = value; }
        }

        private static Int32? _MaxMemory;
        /// <summary>
        /// 最大占用内存。超过最大占用时，整个服务进程将会重启，以释放资源。默认0秒，表示无限
        /// </summary>
        public static Int32 MaxMemory
        {
            get
            {
                if (_MaxMemory == null) _MaxMemory = GetConfigInt32("MaxMemory", 0);
                return _MaxMemory.Value;
            }
            set { _MaxMemory = value; }
        }

        private static Int32? _MaxThread;
        /// <summary>
        /// 最大总线程数。超过最大占用时，整个服务进程将会重启，以释放资源。默认0秒，表示无限
        /// </summary>
        public static Int32 MaxThread
        {
            get
            {
                if (_MaxThread == null) _MaxThread = GetConfigInt32("MaxThread", 0);
                return _MaxThread.Value;
            }
            set { _MaxThread = value; }
        }

        private static Int32? _AutoRestart;
        /// <summary>
        /// 自动重启时间，单位：分钟。到达自动重启时间时，整个服务进程将会重启，以释放资源。默认0秒，表示无限
        /// </summary>
        public static Int32 AutoRestart
        {
            get
            {
                if (_AutoRestart == null) _AutoRestart = GetConfigInt32("AutoRestart", 0);
                return _AutoRestart.Value;
            }
            set { _AutoRestart = value; }
        }

        /// <summary>
        /// 从配置文件中读取整型配置值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static Int32 GetConfigInt32(String name, Int32 defaultValue)
        {
            String str = ConfigurationManager.AppSettings[name];
            if (String.IsNullOrEmpty(str)) return defaultValue;

            Int32 result = defaultValue;
            if (!Int32.TryParse(str, out result) && result > 0) result = defaultValue;

            return result;
        }
        #endregion

        #region IWriteLog 成员
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void WriteLine(String format, params Object[] args)
        {
            if (XTrace.Debug) XTrace.WriteLine(format, args);
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteLine(String msg)
        {
            if (XTrace.Debug) XTrace.WriteLine(msg);
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteLog(String msg)
        {
            if (XTrace.Debug) XTrace.WriteLine(msg);
        }
        #endregion
    }
}