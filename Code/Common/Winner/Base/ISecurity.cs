using System.Security.Cryptography;
using System.Text;

namespace Winner.Base
{

    public interface ISecurity
    {
      
        /// <summary>
        /// 得到MD5加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string EncryptMd5(string input);
        /// <summary>
        /// SHA1加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string EncryptSha1(string input);
        /// <summary>
        /// 得到3DES
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        string Encrypt3Des(string input, string key);
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        string Decrypt3Des(string input, string key);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputBytes"></param>
        /// <returns></returns>
        byte[] EncryptSha1(byte[] inputBytes);
        /// <summary>
        /// 得到3DES
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string EncryptSign(string input);
        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        string EncryptAes(string input, string key, string vector = null);
        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        string DecryptAes(string input, string key, string vector = null);
        /// <summary>
        /// 验证校验
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="publicKeyPem"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        byte[] EncryptRsa(string content, string publicKey, Encoding encoding,HashAlgorithmName signType);
        /// <summary>
        /// 验证校验
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="publicfileName"></param>
        /// <param name="password"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        byte[] EncryptRsa(string content,string publicfileName, string password, Encoding encoding);
        /// <summary>
        /// 验证校验
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="publicKeyPem"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        byte[] EncryptRsa(byte[] inputBytes, string publicKey,HashAlgorithmName signType);
        /// <summary>
        /// 验证校验
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="publicfileName"></param>
        /// <param name="password"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        byte[] EncryptRsa(byte[] inputBytes, string publicfileName, string password);
        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        byte[] DecryptRsa(string content, string privateKey, Encoding encoding, HashAlgorithmName signType);
        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="content"></param>
        /// <param name="password"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <param name="privateKeyFileName"></param>
        /// <returns></returns>
        byte[] DecryptRsa(string content, string privateKeyFileName, string password, Encoding encoding);

        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        byte[] DecryptRsa(byte[] inputBytes, string privateKey, HashAlgorithmName signType);
        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="content"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        byte[] DecryptRsa(byte[] inputBytes, string privateKeyFileName, string password);
        /// <summary>
        /// 验证校验
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="publicKey"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        bool CheckRsa(string content, string sign, string publicKey, Encoding encoding, HashAlgorithmName signType);
        /// <summary>
        /// 验证校验
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="publicfileName"></param>
        /// <param name="password"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        bool CheckRsa(string content, string sign, string publicfileName, string password, Encoding encoding, HashAlgorithmName signType);

        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="content"></param>
        /// <param name="password"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <param name="privateKeyFileName"></param>
        /// <returns></returns>
        string SignRsa(string content, string privateKeyFileName, string password, Encoding encoding, HashAlgorithmName signType);
        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <param name="privateKeyPem"></param>
        /// <returns></returns>
        string SignRsa(string content, string privateKey, Encoding encoding, HashAlgorithmName signType);
    }
}
