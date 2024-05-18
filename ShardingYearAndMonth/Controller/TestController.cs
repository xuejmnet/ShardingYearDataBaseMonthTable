using Microsoft.AspNetCore.Mvc;
using ShardingCore.Extensions.ShardingPageExtensions;
using ShardingYearAndMonth.DbContexts;
using ShardingYearAndMonth.Entities;

namespace ShardingYearAndMonth.Controller;

[ApiController]
[Route("[controller]/[action]")]
public class TestController : ControllerBase
{
    
    private readonly TestDbContext _testDbContext;

    public TestController(TestDbContext testDbContext)
    {
        _testDbContext = testDbContext;
    }

    public IActionResult HelloWorld()
    {
        return Ok("hello world");
    }

    public async Task<IActionResult> Init()
    {
        var orderItems = new List<OrderItem>();
        var dateTime = new DateTime(2022,1,1);
        var end = new DateTime(2025,1,1);
        int i = 0;
        while (dateTime < end)
        {
            orderItems.Add(new OrderItem()
            {
                Id = i.ToString(),
                User = "用户"+i.ToString(),
                PayAmount=i,
                CreateTime = dateTime,
            });
            i++;
            dateTime = dateTime.AddDays(15);
        }

        await _testDbContext.OrderItems.AddRangeAsync(orderItems);
        await _testDbContext.SaveChangesAsync();
        return Ok("hello world");
    }

    public async Task<IActionResult> Query([FromQuery]int current)
    {
        var dateTime = new DateTime(2023,1,1);
        var shardingPagedResult = await _testDbContext.OrderItems
            .Where(o => o.CreateTime > dateTime)
            .OrderBy(o=>o.CreateTime)
            .ToShardingPageAsync(current, 20);
        return Ok(shardingPagedResult);
    }
}