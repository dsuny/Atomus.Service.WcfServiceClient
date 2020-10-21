using Atomus.Diagnostics;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using System.Xml;

namespace Atomus.Service
{
    public class WcfServiceClient : IService//, IServiceAsync
    {
        //private IService Service;
        //private IServiceAsync ServiceAsync;
        private int tryConnectCount;
        private List<ServiceInfo> listServicePool;
        private int servicePoolMaxCount;

        public WcfServiceClient()
        {
            this.listServicePool = new List<ServiceInfo>();
            this.tryConnectCount = 0;
            //this.CreateService();

            try
            {
                this.servicePoolMaxCount = this.GetAttribute("ServicePoolMaxCount").ToInt();
            }
            catch (Exception exception)
            {
                new AtomusException(exception);

                this.servicePoolMaxCount = 1;
            }
        }
        public WcfServiceClient(string bindingName, string bindingConfigName, string address) : this()
        {
            this.CreateService(bindingName, bindingConfigName, address);
        }

        private bool CreateService()
        {
            string url;
            string bindingName;
            string bindingConfigName;

            try
            {
                url = this.GetAttribute("Url");

                bindingName = this.GetAttribute("Binding");
                bindingConfigName = this.GetAttribute("BindingConfigName");

                return this.CreateService(bindingName, bindingConfigName, url);
            }
            catch (AtomusException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
                throw new AtomusException(exception);
            }
        }
        private bool CreateService(string bindingName, string bindingConfigName, string address)
        {
            return this.LoadBizProxy(bindingName, bindingConfigName, address);
        }

        private bool LoadBizProxy(string bindingName, string bindingConfigName, string address)
        {
            Binding binding;
            Uri uri;
            ServiceEndpoint serviceEndpoint;
            ChannelFactory<IService> channelFactory;
            //ChannelFactory<IServiceAsync> _ChannelFactoryAsync;

            try
            {
                //if (this.listServicePool.Count == 0)
                //{
                //    //this.LoadBinding();

                //    this.LoadBinding(this.GetttributeInnerXml("system.serviceModel", 0));
                //    //this.LoadBinding(Factory.FactoryConfig.XmlDocument.SelectNodes(this.GetType().FullName.Replace(".", "/") + "/system.serviceModel").Item(0).InnerXml);
                //}

                switch (bindingName)
                {
                    case "BasicHttpBinding":
                        if (bindingConfigName.IsNullOrEmpty())
                            binding = this.GetBasicHttpBinding();
                        else
                            binding = this.GetBasicHttpBinding(bindingConfigName);
                        break;

                    case "BasicHttpsBinding":
                        if (bindingConfigName.IsNullOrEmpty())
                            binding = this.GetBasicHttpsBinding();
                        else
                            binding = this.GetBasicHttpsBinding(bindingConfigName);
                        break;

                    case "CustomBinding":
                        if (bindingConfigName.IsNullOrEmpty())
                            binding = this.GetCustomBinding();
                        else
                            binding = this.GetCustomBinding(bindingConfigName);
                        break;

                    case "NetMsmqBinding":
                        if (bindingConfigName.IsNullOrEmpty())
                            binding = this.GetNetMsmqBinding();
                        else
                            binding = this.GetNetMsmqBinding(bindingConfigName);
                        break;

                    case "NetNamedPipeBinding":
                        if (bindingConfigName.IsNullOrEmpty())
                            binding = this.GetNetNamedPipeBinding();
                        else
                            binding = this.GetNetNamedPipeBinding(bindingConfigName);
                        break;

                    //case "NetPeerTcpBinding":
                    //    binding = new NetPeerTcpBinding(bindingConfigName);
                    //    ((NetPeerTcpBinding)binding).MaxReceivedMessageSize = 2147483647;
                    //    break;

                    case "NetTcpBinding":
                        if (bindingConfigName.IsNullOrEmpty())
                            binding = this.GetNetTcpBinding();
                        else
                            binding = this.GetNetTcpBinding(bindingConfigName);
                        break;

                    case "WSDualHttpBinding":
                        if (bindingConfigName.IsNullOrEmpty())
                            binding = this.GetWSDualHttpBinding();
                        else
                            binding = this.GetWSDualHttpBinding(bindingConfigName);
                        break;

                    case "WSHttpBinding":
                        if (bindingConfigName.IsNullOrEmpty())
                            binding = this.GetWSHttpBinding();
                        else
                            binding = this.GetWSHttpBinding(bindingConfigName);
                        break;

                    default:
                        binding = null;
                        break;
                }

                try
                {
                    string[] tmps = this.GetAttribute("Timeout").Split(',');

                    int hours = tmps[0].ToInt();
                    int minute = tmps[1].ToInt();
                    int seconds = tmps[2].ToInt();

                    binding.OpenTimeout = new TimeSpan(hours, minute, seconds);
                    binding.CloseTimeout = new TimeSpan(hours, minute, seconds);
                    binding.SendTimeout = new TimeSpan(hours, minute, seconds);
                    binding.ReceiveTimeout = new TimeSpan(hours, minute, seconds);
                }
                catch (Exception ex)
                {
                    binding.OpenTimeout = new TimeSpan(0, 10, 0);
                    binding.CloseTimeout = new TimeSpan(0, 10, 0);
                    binding.SendTimeout = new TimeSpan(0, 10, 0);
                    binding.ReceiveTimeout = new TimeSpan(0, 10, 0);

                    DiagnosticsTool.MyTrace(ex);
                }

                //_Binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                uri = new Uri(address);

                serviceEndpoint = new ServiceEndpoint(ContractDescription.GetContract(this.GetType()), binding, new EndpointAddress(uri));
                channelFactory = new ChannelFactory<IService>(serviceEndpoint);
                //this.Service = _ChannelFactory.CreateChannel();

                //_ServiceEndpoint = new ServiceEndpoint(ContractDescription.GetContract(this.GetType()), _Binding, new EndpointAddress(_Uri));
                //_ChannelFactoryAsync = new ChannelFactory<IServiceAsync>(_ServiceEndpoint);
                //this.ServiceAsync = _ChannelFactoryAsync.CreateChannel();

                this.listServicePool.Add(new ServiceInfo()
                {
                    Service = channelFactory.CreateChannel(),
                    IsBusy = false
                });

                return true;
            }
            catch (AtomusException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
                throw new AtomusException(exception);
            }
        }

        private Binding GetBasicHttpBinding()
        {
            return SetBasicHttpBinding(new BasicHttpBinding());
        }
        private Binding GetBasicHttpsBinding()
        {
            return SetBasicHttpsBinding(new BasicHttpsBinding());
        }
        private Binding GetBasicHttpBinding(string bindingConfigName)
        {
            return SetBasicHttpBinding(new BasicHttpBinding(bindingConfigName));
        }
        private Binding GetBasicHttpsBinding(string bindingConfigName)
        {
            return SetBasicHttpsBinding(new BasicHttpsBinding(bindingConfigName));
        }
        private Binding SetBasicHttpBinding(BasicHttpBinding binding)
        {
            binding.MaxReceivedMessageSize = 2147483647;
            binding.MaxBufferSize = 2147483647;

            return binding;
        }
        private Binding SetBasicHttpsBinding(BasicHttpsBinding binding)
        {
            binding.MaxReceivedMessageSize = 2147483647;
            binding.MaxBufferSize = 2147483647;

            return binding;
        }

        private Binding GetCustomBinding()
        {
            return new CustomBinding();
        }
        private Binding GetCustomBinding(string bindingConfigName)
        {
            return new CustomBinding(bindingConfigName);
        }

        private Binding GetNetMsmqBinding()
        {
            return SetNetMsmqBinding(new NetMsmqBinding());
        }
        private Binding GetNetMsmqBinding(string bindingConfigName)
        {
            return SetNetMsmqBinding(new NetMsmqBinding(bindingConfigName));
        }
        private Binding SetNetMsmqBinding(NetMsmqBinding binding)
        {
            binding.MaxReceivedMessageSize = 2147483647;

            return binding;
        }

        private Binding GetNetNamedPipeBinding()
        {
            return SetNetNamedPipeBinding(new NetNamedPipeBinding());
        }
        private Binding GetNetNamedPipeBinding(string bindingConfigName)
        {
            return SetNetNamedPipeBinding(new NetNamedPipeBinding(bindingConfigName));
        }
        private Binding SetNetNamedPipeBinding(NetNamedPipeBinding binding)
        {
            binding.MaxReceivedMessageSize = 2147483647;
            binding.MaxBufferSize = 2147483647;

            return binding;
        }

        private Binding GetNetTcpBinding()
        {
            return SetNetTcpBinding(new NetTcpBinding());
        }
        private Binding GetNetTcpBinding(string bindingConfigName)
        {
            return SetNetTcpBinding(new NetTcpBinding(bindingConfigName));
        }
        private Binding SetNetTcpBinding(NetTcpBinding binding)
        {
            binding.MaxReceivedMessageSize = 2147483647;
            binding.MaxBufferSize = 2147483647;
            binding.Security.Mode = SecurityMode.None;

            return binding;
        }

        private Binding GetWSDualHttpBinding()
        {
            return SetWSDualHttpBinding(new WSDualHttpBinding());
        }
        private Binding GetWSDualHttpBinding(string bindingConfigName)
        {
            return SetWSDualHttpBinding(new WSDualHttpBinding(bindingConfigName));
        }
        private Binding SetWSDualHttpBinding(WSDualHttpBinding binding)
        {
            binding.MaxReceivedMessageSize = 2147483647;

            return binding;
        }

        private Binding GetWSHttpBinding()
        {
            return SetWSHttpBinding(new WSHttpBinding());
        }
        private Binding GetWSHttpBinding(string bindingConfigName)
        {
            return SetWSHttpBinding(new WSHttpBinding(bindingConfigName));
        }
        private Binding SetWSHttpBinding(WSHttpBinding binding)
        {
            binding.MaxReceivedMessageSize = 2147483647;

            return binding;
        }

        private void LoadBinding()
        {
            Configuration configuration;
            XmlDocument xmlDocument;
            XmlNode exeNode;
            XmlNodeList xmlNodeList;

            try
            {
                if (System.Web.HttpContext.Current == null)
                    configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                else
                    return;

                xmlDocument = new XmlDocument();
                xmlDocument.Load(configuration.FilePath);

                exeNode = xmlDocument.SelectSingleNode("configuration/system.serviceModel");

                if (exeNode != null)
                {
                    xmlNodeList = Factory.FactoryConfig.XmlDocument.SelectNodes(this.GetType().FullName.Replace(".", "/") + "/system.serviceModel");

                    exeNode.InnerXml = "";
                    foreach (XmlNode xmlNode in xmlNodeList)
                    {
                        exeNode.InnerXml += xmlNode.InnerXml;
                    }
                }

                xmlDocument.Save(configuration.FilePath);
                ConfigurationManager.RefreshSection("system.serviceModel");
            }
            catch (AtomusException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
                throw new AtomusException(exception);
            }
        }
        private void LoadBinding(string innerXml)
        {
            Configuration configuration;
            XmlDocument xmlDocument;
            XmlNode xmlNode;

            try
            {
                if (System.Web.HttpContext.Current == null)
                    configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                else
                    return;

                xmlDocument = new XmlDocument();
                xmlDocument.Load(configuration.FilePath);

                xmlNode = xmlDocument.SelectSingleNode("configuration/system.serviceModel");

                if (xmlNode != null)
                    xmlNode.InnerXml = innerXml;

                xmlDocument.Save(configuration.FilePath);
                ConfigurationManager.RefreshSection("system.serviceModel");
            }
            catch (AtomusException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
                throw new AtomusException(exception);
            }
        }




        Response IService.Request(ServiceDataSet serviceDataSet)
        {
            IResponse response;
            ServiceInfo serviceInfo;

            serviceInfo = null;

            try
            {
                //_Response = this.Service.Request(_ServiceDataSet);

                serviceInfo = this.GetService();
                serviceInfo.IsBusy = true;
                response = serviceInfo.Service.Request(serviceDataSet);

                this.tryConnectCount = 0;//정상적으로 처리가 되면 재시도 횟수 초기화

                return (Response)response;
            }
            catch (CommunicationException _Exception)
            {
                if (this.tryConnectCount > 3)
                    throw _Exception;

                this.tryConnectCount += 1;

                this.RemoveService(serviceInfo);

                return ((IService)this).Request(serviceDataSet);
            }

            catch (AtomusException _Exception)
            {
                return (Response)Factory.CreateInstance("Atomus.Service.Response", false, true, _Exception);
            }
            catch (Exception _Exception)
            {
                return (Response)Factory.CreateInstance("Atomus.Service.Response", false, true, _Exception);
            }
            finally
            {
                serviceInfo?.End();
            }

            //return (Response)_Response;
        }

        public async Task<Response> RequestAsync(ServiceDataSet serviceDataSet)
        {
            IResponse response;
            //ServiceInfo serviceInfo;

            Task pendingTask = Task.FromResult<bool>(true);
            var previousTask = pendingTask;

            //serviceInfo = null;
            response = null;

            pendingTask = Task.Run(async () =>
            {
                try
                {
                    await previousTask;
                    //_Response = this.Service.Request(_ServiceDataSet);
                    //serviceInfo = this.GetService();
                    //serviceInfo.IsBusy = true;
                    response = ((IService)this).Request(serviceDataSet);

                    //this.tryConnectCount = 0;//정상적으로 처리가 되면 재시도 횟수 초기화
                }
                finally
                {
                    //serviceInfo?.End();
                }
            }
                                    );
            await pendingTask;

            //try
            //{
            //    pendingTask = Task.Run(async () =>
            //    {
            //        try
            //        {
            //            await previousTask;
            //            //_Response = this.Service.Request(_ServiceDataSet);
            //            //serviceInfo = this.GetService();
            //            //serviceInfo.IsBusy = true;
            //            response = ((IService)this).Request(serviceDataSet);

            //            //this.tryConnectCount = 0;//정상적으로 처리가 되면 재시도 횟수 초기화
            //        }
            //        finally
            //        {
            //            //serviceInfo?.End();
            //        }
            //    }
            //                            );
            //    await pendingTask;
            //}
            //catch (CommunicationException exception)
            //{
            //    if (this.tryConnectCount > 3)
            //        throw exception;

            //    this.tryConnectCount += 1;

            //    this.RemoveService(serviceInfo);

            //    return await this.RequestAsync(serviceDataSet);
            //}
            //catch (AtomusException exception)
            //{
            //    return (Response)Factory.CreateInstance("Atomus.Service.Response", false, true, exception);
            //}
            //catch (Exception exception)
            //{
            //    return (Response)Factory.CreateInstance("Atomus.Service.Response", false, true, exception);
            //}

            return (Response)response;
        }



        /// <summary>
        /// 서비스 가져오기
        /// 사용 가능한 서비스가 없으면 신규로 생성해서 가져 온다
        /// </summary>
        /// <returns></returns>
        private ServiceInfo GetService()
        {
            var service = from Tmp in this.listServicePool
                          where Tmp.IsBusy.Equals(false)
                          select Tmp;

            if (service.Count() == 0)
            {
                if (this.listServicePool.Count() >= this.servicePoolMaxCount)
                {
                    DiagnosticsTool.MyTrace(new AtomusException("서비스 풀이 가득 찼습니다."));

                    return this.listServicePool[0];
                }
                else
                {
                    this.CreateService();
                    return this.GetService();
                }
            }
            else
            {
                return service.First();
            }
        }

        /// <summary>
        /// 서비스 Pool에서 해당 서비스 제거
        /// </summary>
        /// <param name="serviceInfo"></param>
        private void RemoveService(ServiceInfo serviceInfo)
        {
            this.listServicePool.RemoveAll(x => x.Equals(serviceInfo));
        }
    }
}