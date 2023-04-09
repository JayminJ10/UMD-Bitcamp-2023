using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace BitcampCS
{
    internal class EnvLoad
    {
        public static void loadEnv()
        {
            Environment.SetEnvironmentVariable("TWILIO_ACCOUNT_SID", "ACe8e2de1cbd45f9dd57d75c3c1f659039", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("TWILIO_AUTH_TOKEN", "870406c733ff7403db7a491629e0abb2", EnvironmentVariableTarget.Process);
        }
    }
}
