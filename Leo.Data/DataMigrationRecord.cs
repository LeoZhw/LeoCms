using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.Data
{
    /// <summary>
    /// 数据库迁移记录实体类
    /// </summary>
    public class DataMigrationRecord
    {
        public DataMigrationRecord()
        {
            DataMigrations = new List<DataMigration>();
        }
        public int Id { get; set; }
        public List<DataMigration> DataMigrations { get; set; }
    }

    /// <summary>
    /// 迁移对象
    /// </summary>
    public class DataMigration
    {
        public string DataMigrationClass { get; set; }
        public int? Version { get; set; }
    }
}
