using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Data.Abstractions
{
    /// <summary>
    /// 数据库迁移管理
    /// </summary>
    public interface IDataMigrationManager
    {
        /// <summary>
        ///返回具有至少一个数据迁移类的特性，并调用相应的升级方法
        /// </summary>
        Task<IEnumerable<string>> GetFeaturesThatNeedUpdateAsync();

        /// <summary>
        /// 运行所有需要更新的迁移。
        /// </summary>
        Task UpdateAllFeaturesAsync();

        /// <summary>
        /// 将数据库更新为指定功能的最新版本
        /// </summary>
        Task UpdateAsync(string feature);

        /// <summary>
        /// 将数据库更新为指定功能的最新版本
        /// </summary>
        Task UpdateAsync(IEnumerable<string> features);

        /// <summary>
        /// 执行脚本删除与该特性相关的任何信息
        /// </summary>
        /// <param name="feature"></param>
        Task Uninstall(string feature);
    }
}
