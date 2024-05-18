namespace ShardingYearAndMonth.Entities;

public class OrderItem
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// 购买用户
    /// </summary>
    public string User { get; set; }
    /// <summary>
    /// 付款金额
    /// </summary>
    public decimal PayAmount { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }
}