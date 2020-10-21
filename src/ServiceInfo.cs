namespace Atomus.Service
{
    /// <summary>
    /// 서비스 정보 클래스
    /// </summary>
    internal class ServiceInfo
    {
        public IService Service { get; set; }
        public bool IsBusy { get; set; }
        public void End()
        {
            this.IsBusy = false;
        }
    }
}