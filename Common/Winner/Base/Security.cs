using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Winner.Base
{
    public class Security : ISecurity
    {


        #region 接口的实现
     
        /// <summary>
        /// 得到MD5加密
        /// </summary>
        /// <returns></returns>
        public virtual string EncryptMd5(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var md5 = MD5.Create(); 
            byte[] bytValue = Encoding.UTF8.GetBytes(input);
            byte[] bytHash = md5.ComputeHash(bytValue);
            var sTemp = new StringBuilder();
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp.Append(bytHash[i].ToString("X").PadLeft(2, '0'));
            }
            return sTemp.ToString().ToLower();
            //var encoded = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", "");
            //return encoded.ToLower();
        }
        /// <summary>
        /// SHA1加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string EncryptSha1(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            var encoded = BitConverter.ToString(EncryptSha1(Encoding.UTF8.GetBytes(input))).Replace("-", "");
            return encoded.ToLower();
        }
        /// <summary>
        /// 得到3DES
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual string Encrypt3Des(string input, string key)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var des = TripleDES.Create();
            des.Key = Encoding.UTF8.GetBytes(key);
            des.Mode = CipherMode.ECB;
            var desEncrypt = des.CreateEncryptor();
            byte[] buffer = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(desEncrypt.TransformFinalBlock(buffer, 0, buffer.Length));
        }
        /// <summary>
        /// 得到3DES
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual string Decrypt3Des(string input, string key)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var des = TripleDES.Create();
            des.Key = Encoding.UTF8.GetBytes(key);
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;
            var desDecrypt = des.CreateDecryptor();
            string result = "";
            try
            {
                byte[] buffer = Convert.FromBase64String(input);
                result = Encoding.UTF8.GetString(desDecrypt.TransformFinalBlock(buffer, 0, buffer.Length));
            }
            catch (Exception)
            {

            }
            return result;
        }
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="inputBytes"></param>
        /// <returns></returns>
        public virtual byte[] EncryptSha1(byte[] inputBytes)
        {
            if (inputBytes==null) return inputBytes;
            var sha1 = SHA1.Create();
            byte[] bytesSha1Out = sha1.ComputeHash(inputBytes);
            return bytesSha1Out;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual string EncryptSign(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length < 10)
                return null;
            input = input.Remove(0, 1);
            input.Insert(3, input.Substring(2, 3));
            var mark = EncryptMd5(input);
            return mark.Remove(0, 1).Insert(0, mark[15].ToString());
        }

        #endregion
        #region AES
        /// <summary>
        /// AES
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public virtual string EncryptAes(string input, string key, string vector = null)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(key)) return null;
            if (string.IsNullOrWhiteSpace(vector)) return EncryptAes(input, key);
            Byte[] plainBytes = Encoding.UTF8.GetBytes(input);

            Byte[] bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length)), bKey, bKey.Length);
            Byte[] bVector = new Byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(vector.PadRight(bVector.Length)), bVector, bVector.Length);

            Byte[] Cryptograph = null; // 加密后的密文

            Rijndael Aes = Rijndael.Create();
            try
            {
                // 开辟一块内存流
                using (MemoryStream Memory = new MemoryStream())
                {
                    // 把内存流对象包装成加密流对象
                    using (CryptoStream Encryptor = new CryptoStream(Memory,
                     Aes.CreateEncryptor(bKey, bVector),
                     CryptoStreamMode.Write))
                    {
                        // 明文数据写入加密流
                        Encryptor.Write(plainBytes, 0, plainBytes.Length);
                        Encryptor.FlushFinalBlock();

                        Cryptograph = Memory.ToArray();
                    }
                }
            }
            catch
            {
                Cryptograph = null;
            }

            return Convert.ToBase64String(Cryptograph);
        }
        public virtual string DecryptAes(string input, string key, string vector = null)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(key)) return null;
            if (string.IsNullOrWhiteSpace(vector)) return DecryptAes(input, key);

            var encryptedBytes = Convert.FromBase64String(input);
            var bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length)), bKey, bKey.Length);
            var bVector = new Byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(vector.PadRight(bVector.Length)), bVector, bVector.Length);

            Byte[] original = null; // 解密后的明文

            var Aes = Rijndael.Create();
            try
            {
                // 开辟一块内存流，存储密文
                using (MemoryStream Memory = new MemoryStream(encryptedBytes))
                {
                    // 把内存流对象包装成加密流对象
                    using (CryptoStream Decryptor = new CryptoStream(Memory,
                    Aes.CreateDecryptor(bKey, bVector),
                    CryptoStreamMode.Read))
                    {
                        // 明文存储区
                        using (MemoryStream originalMemory = new MemoryStream())
                        {
                            Byte[] Buffer = new Byte[1024];
                            Int32 readBytes = 0;
                            while ((readBytes = Decryptor.Read(Buffer, 0, Buffer.Length)) > 0)
                            {
                                originalMemory.Write(Buffer, 0, readBytes);
                            }

                            original = originalMemory.ToArray();
                        }
                    }
                }
            }
            catch
            {
                original = null;
            }
            return Encoding.UTF8.GetString(original);
        }
        protected virtual string EncryptAes(string input, string key)
        {
            MemoryStream mStream = new MemoryStream();
            

            var plainBytes = Encoding.UTF8.GetBytes(input);
            var bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length)), bKey, bKey.Length);
            RijndaelManaged aes = new RijndaelManaged
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7,
                KeySize = 128,
                Key = bKey
            };
            CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            try
            {
                cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                cryptoStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
            finally
            {
                cryptoStream.Close();
                mStream.Close();
                aes.Clear();
            }
        }
        protected virtual string DecryptAes(string input, string key)
        {
            var encryptedBytes = Convert.FromBase64String(input);
            var bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length)), bKey, bKey.Length);

            MemoryStream mStream = new MemoryStream(encryptedBytes);
            
            RijndaelManaged aes = new RijndaelManaged
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7,
                KeySize = 128,
                Key = bKey
            };
            CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            try
            {
                byte[] tmp = new byte[encryptedBytes.Length + 32];
                int len = cryptoStream.Read(tmp, 0, encryptedBytes.Length + 32);
                byte[] ret = new byte[len];
                Array.Copy(tmp, 0, ret, 0, len);
                return Encoding.UTF8.GetString(ret);
            }
            finally
            {
                cryptoStream.Close();
                mStream.Close();
                aes.Clear();
            }
        }
        #endregion
        #region RSA加密

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="publicKey"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        public virtual byte[] EncryptRsa(string content, string publicKey, Encoding encoding,
            HashAlgorithmName signType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(publicKey))
                    return null;
                var rsa = CreateRsaProviderFromPublicKey(publicKey, signType);
                var inputBytes = encoding.GetBytes(content);
                return EncryptRsa(inputBytes, rsa);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="publicKeyPem"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public virtual byte[] EncryptRsa(string content, string publicfileName, string password, Encoding encoding)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(publicfileName))
                    return null;
                X509Certificate2 cert = new X509Certificate2(publicfileName, password,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                using (var rsa = (RSA)cert.PublicKey.Key)
                {
                    var inputBytes = encoding.GetBytes(content);
                    return EncryptRsa(inputBytes, rsa);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="publicKey"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        public virtual byte[] EncryptRsa(byte[] inputBytes, string publicKey,HashAlgorithmName signType)
        {
            try
            {
                if (inputBytes==null || string.IsNullOrWhiteSpace(publicKey))
                    return null;
                var rsa = CreateRsaProviderFromPublicKey(publicKey, signType);
                return EncryptRsa(inputBytes, rsa);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="publicKeyPem"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public virtual byte[] EncryptRsa(byte[] inputBytes, string publicfileName, string password)
        {
            try
            {
                if (inputBytes==null || string.IsNullOrWhiteSpace(publicfileName))
                    return null;
                X509Certificate2 cert = new X509Certificate2(publicfileName, password,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                using (var rsa = (RSA)cert.PublicKey.Key)
                {
                    return EncryptRsa(inputBytes, rsa);
                }
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="rsa"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        protected virtual byte[] EncryptRsa(byte[] inputBytes, RSA rsa)
        {
            try
            {
          
                var len = 100;
                if (inputBytes.Length <= len)
                    return rsa.Encrypt(inputBytes, RSAEncryptionPadding.Pkcs1);
                using (var plaiStream = new MemoryStream(inputBytes))
                {
                    using (var crypStream = new MemoryStream())
                    {
                        var offSet = 0;
                        var inputLen = inputBytes.Length;
                        for (var i = 0; inputLen - offSet > 0; offSet = i * len)
                        {
                            if (inputLen - offSet > len)
                            {
                                var buffer = new byte[len];
                                plaiStream.Read(buffer, 0, len);
                                var cryptograph = rsa.Encrypt(buffer, RSAEncryptionPadding.Pkcs1);
                                crypStream.Write(cryptograph, 0, cryptograph.Length);
                            }
                            else
                            {
                                var buffer = new byte[inputLen - offSet];
                                plaiStream.Read(buffer, 0, inputLen - offSet);
                                var cryptograph = rsa.Encrypt(buffer, RSAEncryptionPadding.Pkcs1);
                                crypStream.Write(cryptograph, 0, cryptograph.Length);
                            }
                            ++i;
                        }
                        crypStream.Position = 0;
                        return crypStream.ToArray();
                    }
                }
              
            }
            catch(Exception ex)
            {
                return null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="privateKey"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        public virtual byte[] DecryptRsa(string content, string privateKey, Encoding encoding,
            HashAlgorithmName signType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(privateKey))
                    return null;
                var inputBytes = encoding.GetBytes(content);
                var rsa = CreateRsaProviderFromPrivateKey(privateKey, signType);
                return DecryptRsa(inputBytes, rsa);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="publicKeyPem"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public virtual byte[] DecryptRsa(string content, string privatefileName, string password, Encoding encoding)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(privatefileName))
                    return null;
                X509Certificate2 cert = new X509Certificate2(privatefileName, password,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                using (var rsa = (RSA)cert.PrivateKey)
                {
                    var inputBytes = encoding.GetBytes(content);
                    return DecryptRsa(inputBytes, rsa);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="privateKey"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        public virtual byte[] DecryptRsa(byte[] inputBytes, string privateKey,HashAlgorithmName signType)
        {
            try
            {
                if (inputBytes==null || string.IsNullOrWhiteSpace(privateKey))
                    return null;
                var rsa = CreateRsaProviderFromPrivateKey(privateKey, signType);
                return DecryptRsa(inputBytes, rsa);
            }
            catch(Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="publicKeyPem"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public virtual byte[] DecryptRsa(byte[] inputBytes, string privatefileName, string password)
        {
            try
            {
                if (inputBytes==null || string.IsNullOrWhiteSpace(privatefileName))
                    return null;
                X509Certificate2 cert = new X509Certificate2(privatefileName, password,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                using (var rsa = (RSA)cert.PrivateKey)
                {
                    return DecryptRsa(inputBytes, rsa);
                }
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="rsa"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        protected virtual byte[] DecryptRsa(byte[] inputBytes, RSA rsa)
        {
            try
            {
                var len = 256;
                if (inputBytes.Length <= len)
                    return rsa.Decrypt(inputBytes, RSAEncryptionPadding.Pkcs1);
                using (var plaiStream = new MemoryStream(inputBytes))
                {
                    using (var decrypStream = new MemoryStream())
                    {
                        var offSet = 0;
                        var inputLen = inputBytes.Length;
                        for (var i = 0; inputLen - offSet > 0; offSet = i * len)
                        {
                            if (inputLen - offSet > len)
                            {
                                var buffer = new byte[len];
                                plaiStream.Read(buffer, 0, len);
                                var decrypData = rsa.Decrypt(buffer, RSAEncryptionPadding.Pkcs1);
                                decrypStream.Write(decrypData, 0, decrypData.Length);
                            }
                            else
                            {
                                var buffer = new byte[inputLen - offSet];
                                plaiStream.Read(buffer, 0, inputLen - offSet);
                                var decrypData = rsa.Decrypt(buffer, RSAEncryptionPadding.Pkcs1);
                                decrypStream.Write(decrypData, 0, decrypData.Length);
                            }
                            ++i;
                        }
                        decrypStream.Position = 0;
                        return decrypStream.ToArray();
                    }
                }
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region RSA签名

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="publicKeyPem"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        public virtual bool CheckRsa(string content, string sign, string publicKeyPem, Encoding encoding,
            HashAlgorithmName signType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(sign))
                    return false;
                var rsa = CreateRsaProviderFromPublicKey(publicKeyPem, signType);
                return CheckRsa(content, sign, rsa, encoding, signType);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="publicfileName"></param>
        /// <param name="password"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        public virtual bool CheckRsa(string content, string sign, string publicfileName,string password, Encoding encoding,
            HashAlgorithmName signType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(sign))
                    return false;
                X509Certificate2 cert = new X509Certificate2(publicfileName, password,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                using (var rsa = (RSA)cert.PublicKey.Key)
                {
                    var f = new RSAPKCS1SignatureDeformatter(rsa);
                    f.SetHashAlgorithm(signType.Name);
                    byte[] key = Convert.FromBase64String(sign);
                    var sha = new SHA1Managed();
                    byte[] name = sha.ComputeHash(Encoding.UTF8.GetBytes(content));
                    return f.VerifySignature(name, key);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sign"></param>
        /// <param name="rsa"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        protected virtual bool CheckRsa(string content, string sign, RSA rsa, Encoding encoding,
            HashAlgorithmName signType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(sign))
                    return false;
                var bVerifyResultOriginal = rsa.VerifyData(encoding.GetBytes(content),
                    Convert.FromBase64String(sign), signType, RSASignaturePadding.Pkcs1);
                return bVerifyResultOriginal;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="privateKeyString"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        public virtual string SignRsa(string content, string privateKeyString, Encoding encoding,HashAlgorithmName signType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                    return null;
                var rsa = CreateRsaProviderFromPrivateKey(privateKeyString, signType);
                return SignRsa(content, rsa, encoding, signType);
            }
            catch (Exception e)
            {
                return null;
            }
         
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="privateKeyFileName"></param>
        /// <param name="password"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        public virtual string SignRsa(string content, string privateKeyFileName,string password, Encoding encoding, HashAlgorithmName signType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                    return null;
                X509Certificate2 cert = new X509Certificate2(privateKeyFileName, password,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                using (var rsaProviderEncrypt = (RSA)cert.PrivateKey)
                {
                    RSAPKCS1SignatureFormatter signFormatter = new RSAPKCS1SignatureFormatter(rsaProviderEncrypt);
                    signFormatter.SetHashAlgorithm(signType.Name);
                    byte[] source = Encoding.UTF8.GetBytes(content);
                    var sha = new SHA1Managed();
                    byte[] result = sha.ComputeHash(source);
                    byte[] b = signFormatter.CreateSignature(result);
                    return Convert.ToBase64String(b);
                }

            }
            catch (Exception e)
            {
                return null;
            }
        
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="rsa"></param>
        /// <param name="encoding"></param>
        /// <param name="signType"></param>
        /// <returns></returns>
        protected virtual string SignRsa(string content, RSA rsa, Encoding encoding, HashAlgorithmName signType)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;
            byte[] signatureBytes = null;
            try
            {
                byte[] dataBytes = encoding.GetBytes(content);
                if (null == rsa)
                    throw new Exception("SignRsa Error");
                signatureBytes = rsa.SignData(dataBytes, signType, RSASignaturePadding.Pkcs1);
            }
            catch (Exception e)
            {
                return null;
            }
            return Convert.ToBase64String(signatureBytes);
        }



        #endregion

        #region RSA创建

        private static RSA CreateRsaProviderFromPrivateKey(string privateKey, HashAlgorithmName signType)
        {
            var privateKeyBits = Convert.FromBase64String(privateKey);

            var rsa = RSA.Create();
            var rsaParameters = new RSAParameters();

            using (BinaryReader binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected value read binr.ReadByte()");

                rsaParameters.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.D = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.P = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.Q = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.DP = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.DQ = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }
            if (signType == HashAlgorithmName.SHA256)
                rsa.KeySize = 2048;
            rsa.ImportParameters(rsaParameters);
            return rsa;
        }

        private static RSA CreateRsaProviderFromPublicKey(string publicKeyString, HashAlgorithmName signType)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] seqOid = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];

            var x509Key = Convert.FromBase64String(publicKeyString);

            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            using (MemoryStream mem = new MemoryStream(x509Key))
            {
                using (BinaryReader binr = new BinaryReader(mem))  //wrap Memory Stream with BinaryReader for easy reading
                {
                    byte bt = 0;
                    ushort twobytes = 0;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    seq = binr.ReadBytes(15);       //read the Sequence OID
                    if (!CompareBytearrays(seq, seqOid))    //make sure Sequence for OID is correct
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8203)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    bt = binr.ReadByte();
                    if (bt != 0x00)     //expect null byte next
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                        lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte(); //advance 2 bytes
                        lowbyte = binr.ReadByte();
                    }
                    else
                        return null;
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                    int modsize = BitConverter.ToInt32(modint, 0);

                    int firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {   //if first byte (highest order) of modulus is zero, don't include it
                        binr.ReadByte();    //skip this null byte
                        modsize -= 1;   //reduce modulus buffer size by 1
                    }

                    byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

                    if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
                        return null;
                    int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
                    byte[] exponent = binr.ReadBytes(expbytes);

                    // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                    var rsa = RSA.Create();
                    rsa.KeySize = signType == HashAlgorithmName.SHA1 ? 1024 : 2048;
                    RSAParameters rsaKeyInfo = new RSAParameters();
                    rsaKeyInfo.Modulus = modulus;
                    rsaKeyInfo.Exponent = exponent;
                    rsa.ImportParameters(rsaKeyInfo);

                    return rsa;
                }

            }
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            var count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02) //expect integer
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
            {
                count = binr.ReadByte(); // data size in next byte
            }
            else if (bt == 0x82)
            {
                highbyte = binr.ReadByte(); // data size in next 2 bytes
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt; // we already have the data size
            }

            while (binr.ReadByte() == 0x00)
                //remove high order zeros in data
                count -= 1;
            binr.BaseStream.Seek(-1, SeekOrigin.Current); //last ReadByte wasn't a removed zero, so back up a byte
            return count;
        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

        
        #endregion
    }
}
