using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.Microservice.Abstractions.Config
{
    public class AppConfig
    {
        #region 字段
        //private static AddressSelectorMode _loadBalanceMode = AddressSelectorMode.Polling;
        private static ServerOptions _serverOptions = new ServerOptions();
        #endregion

        public static IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// 负载均衡模式
        /// </summary>
        //public static AddressSelectorMode LoadBalanceMode
        //{
        //    get
        //    {
        //        AddressSelectorMode mode = _loadBalanceMode; ;
        //        if (Configuration != null
        //            && Configuration["AccessTokenExpireTimeSpan"] != null
        //            && !Enum.TryParse(Configuration["AccessTokenExpireTimeSpan"], out mode))
        //        {
        //            mode = _loadBalanceMode;
        //        }
        //        return mode;
        //    }
        //    internal set
        //    {
        //        _loadBalanceMode = value;
        //    }
        //}

        public static IConfigurationSection GetSection(string name)
        {
            return Configuration?.GetSection(name);
        }

        public static ServerOptions ServerOptions
        {
            get
            {
                return _serverOptions;
            }
            set
            {
                _serverOptions = value;
            }
        }
    }
}
