using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.Abstractions.Route
{
    /// <summary>
    /// 一个抽象的服务路由发现者。
    /// </summary>
    public interface IServiceRouteManager
    {

        /// <summary>
        /// 服务路由被创建。
        /// </summary>
        event EventHandler<ServiceRouteEventArgs> Created;

        /// <summary>
        /// 服务路由被删除。
        /// </summary>
        event EventHandler<ServiceRouteEventArgs> Removed;

        /// <summary>
        /// 服务路由被修改。
        /// </summary>
        event EventHandler<ServiceRouteChangedEventArgs> Changed;

        /// <summary>
        /// 获取所有可用的服务路由信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        Task<IEnumerable<ServiceRoute>> GetRoutesAsync();

        /// <summary>
        /// 设置服务路由。
        /// </summary>
        /// <param name="routes">服务路由集合。</param>
        /// <returns>一个任务。</returns>
        Task SetRoutesAsync(IEnumerable<ServiceRoute> routes);

        /// <summary>
        /// 移除地址列表
        /// </summary>
        /// <param name="routes">地址列表。</param>
        /// <returns>一个任务。</returns>
        Task RemveAddressAsync(IEnumerable<EndPoint> Address);
        /// <summary>
        /// 清空所有的服务路由。
        /// </summary>
        /// <returns>一个任务。</returns>
        Task ClearAsync();
    }

    /// <summary>
    /// 服务路由事件参数。
    /// </summary>
    public class ServiceRouteEventArgs
    {
        public ServiceRouteEventArgs(ServiceRoute route)
        {
            Route = route;
        }

        /// <summary>
        /// 服务路由信息。
        /// </summary>
        public ServiceRoute Route { get; private set; }
    }

    /// <summary>
    /// 服务路由变更事件参数。
    /// </summary>
    public class ServiceRouteChangedEventArgs : ServiceRouteEventArgs
    {
        public ServiceRouteChangedEventArgs(ServiceRoute route, ServiceRoute oldRoute) : base(route)
        {
            OldRoute = oldRoute;
        }

        /// <summary>
        /// 旧的服务路由信息。
        /// </summary>
        public ServiceRoute OldRoute { get; set; }
    }
}
