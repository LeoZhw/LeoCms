using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Leo.Data.Abstractions;
using YesSql;

namespace Leo.Data
{
    public class DbConnectionAccessor : IDbConnectionAccessor
    {
        private readonly IStore _store;

        public DbConnectionAccessor(IStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public DbConnection CreateConnection()
        {
            return _store.Configuration.ConnectionFactory.CreateConnection();
        }
    }
}
