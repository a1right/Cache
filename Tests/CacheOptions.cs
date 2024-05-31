using Caching;
using Caching.Exceptions;

namespace Tests;
public class CacheOptionsTests
{
    [Fact]
    public void Negative_ExpirationTime_Throws_Invalid_Expiration_Time_Exception()
    {
        Assert.Throws<InvalidExpirationTimeException>(() =>
        {
            var options = new CacheOptions()
            {
                ExpirationTime = TimeSpan.MinValue
            };

            ICache<int> cache = new Cache<int>(options);
        });
    }

    [Fact]
    public void CacheOptions_Created_With_No_Value()
    {
        var options = new CacheOptions();
    }
}
