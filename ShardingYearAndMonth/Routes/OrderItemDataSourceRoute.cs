using System.Collections.Concurrent;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions;
using ShardingYearAndMonth.Entities;

namespace ShardingYearAndMonth.Routes;

public class OrderItemDataSourceRoute:AbstractShardingOperatorVirtualDataSourceRoute<OrderItem,DateTime>
{
    private readonly ConcurrentBag<string> dataSources = new ConcurrentBag<string>();
    private readonly object _lock = new object();
    public override string ShardingKeyToDataSourceName(object shardingKey)
    {
        return $"{shardingKey:yyyy}";//年份作为分库数据源名称
    }

    public override List<string> GetAllDataSourceNames()
    {
        return dataSources.ToList();
    }

    public override bool AddDataSourceName(string dataSourceName)
    {
        var acquire = Monitor.TryEnter(_lock, TimeSpan.FromSeconds(3));
        if (!acquire)
        {
            return false;
        }
        try
        {
            var contains = dataSources.Contains(dataSourceName);
            if (!contains)
            {
                dataSources.Add(dataSourceName);
                return true;
            }
        }
        finally
        {
            Monitor.Exit(_lock);
        }

        return false;
    }

    public override void Configure(EntityMetadataDataSourceBuilder<OrderItem> builder)
    {
        builder.ShardingProperty(o => o.CreateTime);
    }

    /// <summary>
    /// tail就是2020，2021，2022，2023 所以分片只需要格式化年就可以直接比较了
    /// </summary>
    /// <param name="shardingKey"></param>
    /// <param name="shardingOperator"></param>
    /// <returns></returns>
    public override Func<string, bool> GetRouteToFilter(DateTime shardingKey, ShardingOperatorEnum shardingOperator)
    {
        var t = $"{shardingKey:yyyyy}";
        
        switch (shardingOperator)
        {
            case ShardingOperatorEnum.GreaterThan:
            case ShardingOperatorEnum.GreaterThanOrEqual:
                return tail => String.Compare(tail, t, StringComparison.Ordinal) >= 0;
            case ShardingOperatorEnum.LessThan:
            {
                var currentYear =new DateTime(shardingKey.Year,1,1);
                //处于临界值 o=>o.time < [2021-01-01 00:00:00] 尾巴20210101不应该被返回
                if (currentYear == shardingKey)
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) < 0;
                return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
            }
            case ShardingOperatorEnum.LessThanOrEqual:
                return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
            case ShardingOperatorEnum.Equal: return tail => tail == t;
            default:
            {
                return tail => true;
            }
        }
    }
}