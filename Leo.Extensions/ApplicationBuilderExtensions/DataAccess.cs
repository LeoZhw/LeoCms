using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Leo.Data;
using Leo.Data.Abstractions;
using Leo.Extensions.ServiceCollectionExtensions;
using YesSql;
using YesSql.Indexes;
using YesSql.Provider.MySql;
using YesSql.Provider.PostgreSql;
using YesSql.Provider.Sqlite;
using YesSql.Provider.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace Leo.Extensions.ApplicationBuilderExtensions
{
    public static class DataAccess
    {
        public static IApplicationBuilder UseDataAccess(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CommitSessionMiddleware>();
        }


        /// <summary>
        /// 添加数据库
        /// </summary>
        /// <param name="services"></param>
        /// <param name="databaseType">数据库类型，支持：SqlConnection，Sqlite，MySql，Postgres</param>
        /// <param name="connectionString">Sqlite为yessql.db文件所在路径，其他数据库为连接字符串</param>
        /// <param name="tablePrefix">表名前缀</param>
        /// <returns></returns>
        public static IServiceCollection AddDataAccess(this IServiceCollection services, string databaseType, string connectionString, string tablePrefix = null)
        {
            services.AddScoped<IDataMigrationManager, DataMigrationManager>();

            // Adding supported databases
            services.TryAddDataProvider(name: "Sql Server", value: "SqlConnection", hasConnectionString: true, hasTablePrefix: true, isDefault: false);
            services.TryAddDataProvider(name: "Sqlite", value: "Sqlite", hasConnectionString: false, hasTablePrefix: false, isDefault: true);
            services.TryAddDataProvider(name: "MySql", value: "MySql", hasConnectionString: true, hasTablePrefix: true, isDefault: false);
            services.TryAddDataProvider(name: "Postgres", value: "Postgres", hasConnectionString: true, hasTablePrefix: true, isDefault: false);

            // Configuring data access
            services.AddSingleton<IStore>(sp =>
            {
                IConfiguration storeConfiguration = new YesSql.Configuration();

                switch (databaseType)
                {
                    case "SqlConnection":
                        storeConfiguration
                            .UseSqlServer(connectionString, IsolationLevel.ReadUncommitted)
                            .UseBlockIdGenerator();
                        break;
                    case "Sqlite":
                        var databaseFolder = connectionString;
                        var databaseFile = Path.Combine(databaseFolder, "yessql.db");
                        Directory.CreateDirectory(databaseFolder);
                        storeConfiguration
                            .UseSqLite($"Data Source={databaseFile};Cache=Shared", IsolationLevel.ReadUncommitted)
                            .UseDefaultIdGenerator();
                        break;
                    case "MySql":
                        storeConfiguration
                            .UseMySql(connectionString, IsolationLevel.ReadUncommitted)
                            .UseBlockIdGenerator();
                        break;
                    case "Postgres":
                        storeConfiguration
                            .UsePostgreSql(connectionString, IsolationLevel.ReadUncommitted)
                            .UseBlockIdGenerator();
                        break;
                    default:
                        throw new ArgumentException("Unknown database type: " + databaseType);
                }

                if (!string.IsNullOrWhiteSpace(tablePrefix))
                {
                    storeConfiguration = storeConfiguration.SetTablePrefix(tablePrefix + "_");
                }

                var store = StoreFactory.CreateAsync(storeConfiguration).GetAwaiter().GetResult();
                var indexes = sp.GetServices<IIndexProvider>();

                store.RegisterIndexes(indexes);

                return store;
            });

            services.AddScoped(sp =>
            {
                var store = sp.GetService<IStore>();

                if (store == null)
                {
                    return null;
                }

                var session = store.CreateSession();

                var scopedServices = sp.GetServices<IIndexProvider>();

                session.RegisterIndexes(scopedServices.ToArray());

                var httpContext = sp.GetRequiredService<IHttpContextAccessor>()?.HttpContext;

                if (httpContext != null)
                {
                    httpContext.Items[typeof(YesSql.ISession)] = session;
                }

                return session;
            });

            services.AddTransient<IDbConnectionAccessor, DbConnectionAccessor>();

            return services;
        }
    }

    public class CommitSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public CommitSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await _next.Invoke(httpContext);

            // Don't resolve to prevent instantiating one in case of static sites
            var session = httpContext.Items[typeof(YesSql.ISession)] as YesSql.ISession;

            if (session != null)
            {
                await session.CommitAsync();
            }
        }
    }
}