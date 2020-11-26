namespace Service1
{
    static class SimpleCrypto
    {
        public static string Encrypt(string str, ushort secretKey)
        {
            string newStr = "";
            for (int i = 0; i < str.Length; i++)
            {
                newStr += TopSecret((char)(str[i] + i), secretKey);
            }
            return newStr;
        }
        public static string Decrypt(string str, ushort secretKey)
        {
            string newStr = "";
            for (int i = 0; i < str.Length; i++)
            {
                newStr += (char)(TopSecret(str[i], secretKey) - i);
            }
            return newStr;
        }
        public static char TopSecret(char character, ushort secretKey)
        {
            character = (char)(character ^ secretKey);
            return character;
        }
    }
}
