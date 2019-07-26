using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Leo.Data.Abstractions
{
    public interface IDbConnectionAccessor
    {
        /// <summary>
        /// 创建数据库连接
        /// </summary>
        /// <returns></returns>
        DbConnection CreateConnection();
    }
}
