using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using ShardingCore;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Helpers;
using ShardingCore.TableExists;
using ShardingCore.TableExists.Abstractions;
using ShardingYearAndMonth;
using ShardingYearAndMonth.DbContexts;
using ShardingYearAndMonth.Entities;
using ShardingYearAndMonth.Routes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
ILoggerFactory efLogger = LoggerFactory.Create(builder =>
{
    builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information).AddConsole();
});
builder.Services.AddShardingDbContext<TestDbContext>()
    .UseRouteConfig(o =>
    {
        o.AddShardingDataSourceRoute<OrderItemDataSourceRoute>();
        o.AddShardingTableRoute<OrderItemTableRoute>();
    })
    .UseConfig((sp, o) =>
    {
        o.ThrowIfQueryRouteNotMatch = false;

        // var redisConfig = sp.GetService<RedisConfig>();
        // o.AddDefaultDataSource(redisConfig.Default, redisConfig.DefaultConn);
        // //redisConfig.ExtraConfigs
        // o.AddExtraDataSource();
        
        o.AddDefaultDataSource("2024", "server=127.0.0.1;port=3306;database=sd2024;userid=root;password=root;");
        o.UseShardingQuery((conn, b) =>
        {
            b.UseMySql(conn, new MySqlServerVersion(new Version())).UseLoggerFactory(efLogger);
        });
        o.UseShardingTransaction((conn, b) =>
        {
            b.UseMySql(conn, new MySqlServerVersion(new Version())).UseLoggerFactory(efLogger);
        });
        o.UseShardingMigrationConfigure(b =>
        {
            b.ReplaceService<IMigrationsSqlGenerator, ShardingMySqlMigrationsSqlGenerator>();
        });
    }).AddShardingCore();




var app = builder.Build();

var shardingRuntimeContext = app.Services.GetService<IShardingRuntimeContext<TestDbContext>>();
var dataSourceRouteManager = shardingRuntimeContext.GetDataSourceRouteManager();
var virtualDataSourceRoute = dataSourceRouteManager.GetRoute(typeof(OrderItem));
virtualDataSourceRoute.AddDataSourceName("2024");
virtualDataSourceRoute.AddDataSourceName("2023");
virtualDataSourceRoute.AddDataSourceName("2022");
DynamicShardingHelper.DynamicAppendDataSource(shardingRuntimeContext,"2023","server=127.0.0.1;port=3306;database=sd2023;userid=root;password=root;",false,false);
DynamicShardingHelper.DynamicAppendDataSource(shardingRuntimeContext,"2022","server=127.0.0.1;port=3306;database=sd2022;userid=root;password=root;",false,false);

using (var scope = app.Services.CreateScope())
{
    var testDbContext = scope.ServiceProvider.GetService<TestDbContext>();
    testDbContext.Database.EnsureCreated();
}

app.Services.UseAutoTryCompensateTable();

app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapControllers());
app.Run();
