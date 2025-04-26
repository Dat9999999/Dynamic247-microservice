using StackExchange.Redis;

namespace NewsPage.helpers
{
    public class OtpHelper
    {
        private readonly IDatabase _redisDb;

        public OtpHelper(IConfiguration configuration)
        {
            var redis = ConnectionMultiplexer.Connect(configuration["Redis:ConnectionString"]);
            _redisDb = redis.GetDatabase();
        }

        public (string Otp, DateTime Expiry) GenerateOtp(string key, int expiryMinutes = 3)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Email cannot be null or empty.");
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

            try
            {
                bool isSet = _redisDb.StringSet(key, otp, TimeSpan.FromMinutes(expiryMinutes));
                if (!isSet)
                {
                    Console.WriteLine("Failed to save OTP to Redis.");
                }
                else
                {
                    Console.WriteLine($"OTP saved successfully for {key}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Redis error: {ex.Message}");
            }

            return (otp, expiry);
        }

        public bool ValidateOtp(string key, string otp)
        {
            var storedOtp = _redisDb.StringGet(key);
            if (!string.IsNullOrEmpty(storedOtp) && storedOtp == otp)
            {
                _redisDb.KeyDelete(key); // Xóa OTP sau khi dùng
                return true;
            }
            return false;
        }
    }
}
