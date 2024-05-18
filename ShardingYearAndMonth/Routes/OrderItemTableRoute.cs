using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;
using ShardingCore.Sharding.EntityQueryConfigurations;
using ShardingYearAndMonth.Entities;

namespace ShardingYearAndMonth.Routes;

public class OrderItemTableRoute:AbstractShardingOperatorVirtualTableRoute<OrderItem,DateTime>
{
    private readonly List<string> allTails = Enumerable.Range(1, 12).Select(o => o.ToString().PadLeft(2, '0')).ToList();
    public override string ShardingKeyToTail(object shardingKey)
    {
        var time = Convert.ToDateTime(shardingKey);
        return $"{time:MM}";//01,02.....11,12
    }

    public override List<string> GetTails()
    {
        return allTails;
    }

    public override void Configure(EntityMetadataTableBuilder<OrderItem> builder)
    {
        builder.ShardingProperty(o => o.CreateTime);
    }

    protected override bool RouteIgnoreDataSource => false;

    public override Func<string, bool> GetRouteToFilter(DateTime shardingKey, ShardingOperatorEnum shardingOperator)
    {
        var t = $"{shardingKey:yyyyy.MM}";
        
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