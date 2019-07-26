using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.Data.Abstractions
{
    /// <summary>
    /// 数据库迁移工具，封装YesSql功能，直接修改数据库结构
    /// </summary>
    public interface ISchemaBuilder
    {
        YesSql.Sql.ISchemaBuilder SchemaBuilder { get; set; }
    }
}
