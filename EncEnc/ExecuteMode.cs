using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EncEnc
{
    public enum ExecuteMode
    {
        Encrypt = 0,
        Decrypt
    }

    public static class ExecuteModeExtension
    {
        private static readonly string[] _name = new[]
            {
                "Encrypt",
                "Decrypt"
            };

        public static string DisplayName(this ExecuteMode me)
        {
            return _name[(int)me];
        }
    }
}
