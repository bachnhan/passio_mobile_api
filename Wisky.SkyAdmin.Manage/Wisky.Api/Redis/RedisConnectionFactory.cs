using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace RedisCache
{
    public class RedisConnectionFactory
    {
        private static Lazy<ConnectionMultiplexer> Connection;

        private static RedisConnectionFactory instance;

        public static RedisConnectionFactory Instance
        {
            get
            {
                var connectionString =
                  WebConfigurationManager.AppSettings["RedisConnection"].ToString();

                var options = ConfigurationOptions.Parse(connectionString);

                if (instance == null)
                {
                    instance = new RedisConnectionFactory();
                }

                //var connectiontmp = new Lazy<ConnectionMultiplexer>(
                //    () =>
                //    {
                //        try
                //        {
                //            var connectionMultiplexers = ConnectionMultiplexer.Connect(options);
                //            return connectionMultiplexers;
                //        }
                //        catch (Exception ex)
                //        {
                //            return null;
                //        }
                //    }
                //);

                //if (Connection.Value == null)
                //{
                //    if (connectiontmp.Value != null)
                //    {
                //        ReConnectionFactory();
                //    }
                //}
                //else
                //{
                //    if (connectiontmp.Value == null)
                //    {
                //        ReConnectionFactory();
                //    }
                //}
                return instance;
            }
        }

        static RedisConnectionFactory()
        {
            ReConnectionFactory();
        }

        private static void  ReConnectionFactory()
        {
            var connectionString =
               WebConfigurationManager.AppSettings["RedisConnection"].ToString();

            var options = ConfigurationOptions.Parse(connectionString);

            Connection = new Lazy<ConnectionMultiplexer>(
                () =>
                {
                    try
                    {
                        var connectionMultiplexers = ConnectionMultiplexer.Connect(options);
                        return connectionMultiplexers;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            );
        }

        public ConnectionMultiplexer GetConnection() => Connection.Value;

    }
}
