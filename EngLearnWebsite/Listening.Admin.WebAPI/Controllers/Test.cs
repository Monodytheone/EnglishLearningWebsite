using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Listening.Admin.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Test : ControllerBase
    {
        private readonly IConnectionMultiplexer _redisConn;

        public Test(IConnectionMultiplexer redisMultiplexer)
        {
            _redisConn = redisMultiplexer;
        }

        [HttpPost]
        public async Task<ActionResult> Set(string key, string value)
        {
            var db = _redisConn.GetDatabase();
            await db.StringSetAsync($"EngLearningRedisTest_{key}", value);
            return Ok();
        }

        [HttpGet]
        [Route("{key}")]
        public async Task<ActionResult<string>> Get(string key)
        {
            var db = _redisConn.GetDatabase();
            string value = await db.StringGetAsync($"EngLearningRedisTest_{key}");
            return value;
        }

        [HttpGet]
        public List<int> AttempToReturnList()
        {
            return new List<int>() { 1, 2, 3, 4 };
        }
    }
}
